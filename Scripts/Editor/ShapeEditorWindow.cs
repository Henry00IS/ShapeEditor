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
            Rect viewportRect = GetViewportRect();

            var renderTexture = RenderTexture.GetTemporary(Mathf.CeilToInt(viewportRect.width), Mathf.CeilToInt(viewportRect.height));
            Graphics.SetRenderTarget(renderTexture);

            var gridMaterial = ShapeEditorResources.temporaryGridMaterial;
            gridMaterial.SetFloat("_offsetX", gridOffset.x);
            gridMaterial.SetFloat("_offsetY", gridOffset.y);
            gridMaterial.SetFloat("_viewportWidth", viewportRect.width);
            gridMaterial.SetFloat("_viewportHeight", viewportRect.height);
            gridMaterial.SetFloat("_scale", gridScale);
            gridMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.LoadIdentity();
            GL.Color(Color.red);
            GLUtilities.DrawRectangle(0, 0, viewportRect.width, viewportRect.height);
            GL.End();
            GL.PopMatrix();

            Graphics.SetRenderTarget(null);

            var drawTextureMaterial = ShapeEditorResources.temporaryDrawTextureMaterial;
            drawTextureMaterial.mainTexture = renderTexture;
            drawTextureMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.LoadIdentity();
            GL.Color(Color.red);
            GLUtilities.DrawRectangle(0, 21, viewportRect.width, viewportRect.height);
            GL.End();
            GL.PopMatrix();

            renderTexture.Release();
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