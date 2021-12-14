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
        private float2 gridOffset;
        private float gridScale = 16f;

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

        private void DrawGrid()
        {
            var gridMaterial = ShapeEditorResources.temporaryGridMaterial;
            gridMaterial.SetFloat("_offsetX", gridOffset.x + (docked ? 1f : 0f));
            gridMaterial.SetFloat("_offsetY", gridOffset.y + (docked ? 3f : 5f));// + (docked ? 13f : 11f));
            gridMaterial.SetFloat("_scale", gridScale);
            gridMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            GL.LoadIdentity();
            Rect viewportRect = GetViewportRect();
            GL.Color(Color.red);
            GLUtilities.DrawRectangle(viewportRect.x, viewportRect.y, viewportRect.width, viewportRect.height);
            GL.End();
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
                Repaint();
            }
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
                    gridScale = 16f;
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