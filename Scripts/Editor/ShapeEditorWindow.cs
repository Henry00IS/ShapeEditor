#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// The 2D Shape Editor Window.
    /// </summary>
    public partial class ShapeEditorWindow : EditorWindow
    {
        /// <summary>
        /// The currently loaded project.
        /// </summary>
        private Project project = new Project();

        [MenuItem("Window/2D Shape Editor")]
        public static void Init()
        {
            // get existing open window or if none, make a new one:
            ShapeEditorWindow window = GetWindow<ShapeEditorWindow>();
            window.minSize = new float2(800, 600);
            window.Show();
            window.titleContent = new GUIContent("Shape Editor", ShapeEditorResources.Instance.shapeEditorIcon);
            window.minSize = new float2(128, 128);
        }

        public static ShapeEditorWindow InitAndGetHandle()
        {
            Init();
            return GetWindow<ShapeEditorWindow>();
        }

        private void OnRepaint()
        {
            DrawViewport();

            foreach (Shape shape in project.shapes)
            {
                foreach (Segment segment in shape.segments)
                {
                    // draw pivots of the segments.
                    var segmentScreenPosition = GridPointToScreen(segment.position);
                    Handles.DrawSolidRectangleWithOutline(new Rect(segmentScreenPosition - halfPivotScale, new float2(pivotScale)), Color.white, Color.black);
                }
            }
        }

        private void OnMouseDown(int button)
        {
        }

        private void OnMouseUp(int button)
        {
        }

        private void OnMouseDrag(int button, float2 delta)
        {
            // pan the viewport around with the right mouse button.
            if (isRightMousePressed)
            {
                gridOffset += delta;
            }

            Repaint();
        }

        private void OnMouseScroll(float delta)
        {
            gridScale -= delta;
            Repaint();
        }

        private bool OnKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.H:
                    gridOffset = new float2(0f, 0f);
                    //GridResetOffset();
                    GridResetZoom();
                    Repaint();
                    return true;
            }
            return false;
        }

        private bool OnKeyUp(KeyCode keyCode)
        {
            return false;
        }

        /// <summary>
        /// Gets the next segment.
        /// </summary>
        /// <param name="segment">The segment to find the next segment for.</param>
        /// <returns>The next segment (wraps around).</returns>
        private Segment GetNextSegment(Segment segment)
        {
            Shape parent = segment.shape;
            int index = parent.segments.IndexOf(segment);
            if (index + 1 > parent.segments.Count - 1)
                return parent.segments[0];
            return parent.segments[index + 1];
        }
    }
}

#endif