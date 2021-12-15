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
        private const float pivotScale = 9f;
        private const float halfPivotScale = pivotScale / 2f;

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
            DrawGrid();
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
                    GridResetOffset();
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
    }
}

#endif