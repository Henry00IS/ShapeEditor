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
        /// <summary>The currently loaded project.</summary>
        [SerializeField]
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

            // ensure that we are subscribed to undo/redo notifications.
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public static ShapeEditorWindow InitAndGetHandle()
        {
            Init();
            return GetWindow<ShapeEditorWindow>();
        }

        /// <summary>We use the static constructor to handle C# reloads.</summary>
        static ShapeEditorWindow()
        {
            // re-subscribe to undo/redo.
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnRepaint()
        {
            DrawViewport();
        }

        private void OnMouseDown(int button)
        {
            // unless the shift key is held down we clear the selection.
            if (!isShiftPressed)
                project.ClearSelection();

            // find the closest segment to the click position.
            var segment = FindSegmentAtScreenPosition(mousePosition, 60.0f);
            if (segment != null)
                segment.selected = !segment.selected;

            Repaint();
        }

        private void OnMouseUp(int button)
        {
            Repaint();
        }

        private void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            // pan the viewport around with the right mouse button.
            if (isRightMousePressed)
            {
                gridOffset += screenDelta;
            }

            if (isLeftMousePressed)
            {
                foreach (var segment in ForEachSelectedSegment())
                {
                    segment.position += gridDelta;
                }
            }

            Repaint();
        }

        private void OnMouseMove(int button, float2 screenDelta, float2 gridDelta)
        {
        }

        private void OnMouseScroll(float delta)
        {
            var mouseBeforeZoom = ScreenPointToGrid(mousePosition);

            gridZoom *= math.pow(2, -delta / 24.0f); // what about math.exp(-delta / 24.0f); ?

            // recalculate the grid offset to zoom into whatever is under the mouse cursor.
            var mouseAfterZoom = ScreenPointToGrid(mousePosition);
            var mouseDifference = mouseAfterZoom - mouseBeforeZoom;
            gridOffset = RenderTextureGridPointToScreen(mouseDifference);

            Repaint();
        }

        private bool OnKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.H:
                    GridResetOffset();
                    GridResetZoom();
                    Repaint();
                    return true;

                case KeyCode.W:

                    return true;
            }
            return false;
        }

        private bool OnKeyUp(KeyCode keyCode)
        {
            return false;
        }

        private void OnTopToolbarGUI()
        {
            if (GUILayout.Button(new GUIContent(ShapeEditorResources.Instance.shapeEditorNew, "New Project (N)"), ShapeEditorResources.toolbarButtonStyle))
            {
                Undo.RecordObject(this, "New Project");
                project = new Project();
                Repaint();
            }

            GUILayout.FlexibleSpace();
        }

        private void OnBottomToolbarGUI()
        {
            GUILayout.Label("2D Shape Editor");

            GUILayout.FlexibleSpace();

            GUILayout.Label("Snap");
            gridSnap = EditorGUILayout.FloatField(gridSnap, GUILayout.Width(64f));

            GUILayout.Label("Zoom");
            gridZoom = EditorGUILayout.FloatField(gridZoom, GUILayout.Width(64f));
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