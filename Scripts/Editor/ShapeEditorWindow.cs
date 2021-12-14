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
        private float2 viewportOffset;

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
            gridMaterial.SetFloat("_offsetX", viewportOffset.x + (docked ? -1f : 0f));
            gridMaterial.SetFloat("_offsetY", viewportOffset.y + (docked ? 13f : 11f));
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
                viewportOffset += delta;
                Debug.Log(viewportOffset);
                Repaint();
            }
        }
    }
}

#endif