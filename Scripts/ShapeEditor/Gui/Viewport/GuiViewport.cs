#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;
using static AeternumGames.ShapeEditor.GLUtilities;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a 3D viewport control inside of a window.</summary>
    public abstract partial class GuiViewport : GuiControl
    {
        /// <summary>Gets or sets the clear color used before rendering.</summary>
        public Color clearColor = Color.black;

        /// <summary>
        /// Gets or sets whether to draw an infinite metric grid that fades into the camera clear
        /// color. It highlights the global X and Y coordinates.
        /// </summary>
        public bool gridEnabled = true;

        /// <summary>Gets the camera instance in control of this viewport.</summary>
        public abstract Camera camera { get; }

        public GuiViewport(float2 position, float2 size) : base(position, size)
        {
        }

        public GuiViewport(float2 size) : base(float2.zero, size)
        {
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            if (camera == null) return;

            if (isActive && editor.isRightMousePressed)
            {
                editor.SetMouseCursor(UnityEditor.MouseCursor.FPS);
                editor.SetMouseScreenWrapping();
            }

            camera.width = drawRect.width;
            camera.height = drawRect.height;

            // overridable pre-render callback.
            OnPreRender();

            var temporaryRenderTexture = GLUtilities.DrawTemporaryRenderTexture((int)drawRect.width, (int)drawRect.height, 24, OnRenderViewport);
            GLUtilities.DrawGuiTextured(temporaryRenderTexture, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawRect.x, drawRect.y, drawRect.width, drawRect.height);
            });
            RenderTexture.ReleaseTemporary(temporaryRenderTexture);

            // overridable post-render callback.
            OnPostRender();
        }

        /// <summary>Called when the custom viewport render texture is rendered.</summary>
        private void OnRenderViewport(RenderTexture renderTexture)
        {
            if (camera == null) return;

            // update the camera.
            if (camera.OnRender())
                editor.Repaint();

            // clear the temporary texture which may be anything.
            GL.Clear(true, true, clearColor);

            // overridable 2D render pass before drawing the 3D world.
            GL.LoadPixelMatrix(0f, drawRect.width, drawRect.height, 0f);
            OnPreRender2D();

            // main 3D render pass.
            camera.LoadMatricesIntoGL();

            // draw the grid if enabled.
            if (gridEnabled)
                RenderGrid();

            // overridable 3D render pass.
            OnRender3D();

            // overridable 2D render pass after drawing the 3D world.
            GL.LoadPixelMatrix(0f, drawRect.width, drawRect.height, 0f);
            OnPostRender2D();
        }

        /// <summary>
        /// Draws an infinite metric grid that fades into the camera clear color. It highlights the
        /// global X and Y coordinates.
        /// </summary>
        private void RenderGrid()
        {
            if (camera == null) return;

            GLUtilities3D.DrawGuiLines(() =>
            {
                int gridSegments = 100;
                float halfSegments = gridSegments / 2f;

                var campos = camera.transform.position;
                var camdist = Vector3.Magnitude(new Vector3(campos.x, 0f, campos.z));

                // we keep the grid centered at the camera position, then with modulo the grid is
                // moved in reverse within 1m giving the illusion of an infinite grid.
                var offset = new float3(campos.x - campos.x % 1f, 0f, campos.z - campos.z % 1f);

                for (int i = 0; i < gridSegments; i++)
                {
                    var dist = Mathf.InverseLerp(halfSegments, 0.0f, Mathf.Abs(-halfSegments + i)) * 0.206f;
                    var fade = new Color(dist, dist, dist);

                    GLUtilities3D.DrawLine(offset + new float3(-halfSegments + i, 0f, -0.25f), offset + new float3(-halfSegments + i, 0f, halfSegments), fade, Color.black);
                    GLUtilities3D.DrawLine(offset + new float3(-halfSegments + i, 0f, 0.25f), offset + new float3(-halfSegments + i, 0f, -halfSegments), fade, Color.black);

                    GLUtilities3D.DrawLine(offset + new float3(0f, 0f, -halfSegments + i + 0.25f), offset + new float3(halfSegments, 0f, -halfSegments + i + 0.25f), fade, Color.black);
                    GLUtilities3D.DrawLine(offset + new float3(0f, 0f, -halfSegments + i + 0.25f), offset + new float3(-halfSegments, 0f, -halfSegments + i + 0.25f), fade, Color.black);
                }

                // global x line:
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0.25f), new float3(halfSegments + camdist, 0f, 0.25f), ShapeEditorWindow.gridCenterLineXColor, Color.black);
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0.25f), new float3(-halfSegments - camdist, 0f, 0.25f), ShapeEditorWindow.gridCenterLineXColor, Color.black);

                // global y line:
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0f), new float3(0f, 0f, halfSegments + camdist), ShapeEditorWindow.gridCenterLineYColor, Color.black);
                GLUtilities3D.DrawLine(new float3(0f, 0f, 0f), new float3(0f, 0f, -halfSegments - camdist), ShapeEditorWindow.gridCenterLineYColor, Color.black);
            });
        }

        public override void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (camera == null) return;

            camera.OnGlobalMouseDrag(button, screenDelta, gridDelta);
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            if (camera == null) return false;

            if (camera.OnKeyDown(keyCode))
                return true;
            return false;
        }

        public override bool OnKeyUp(KeyCode keyCode)
        {
            if (camera == null) return false;

            if (camera.OnKeyUp(keyCode))
                return true;
            return false;
        }

        /// <summary>
        /// Called at the beginning of the control's <see cref="OnRender"/> function. This draws
        /// on the normal screen.
        /// </summary>
        protected virtual void OnPreRender()
        { }

        /// <summary>
        /// Called before drawing the 3D world on the render texture with a 2D pixel matrix.
        /// </summary>
        protected virtual void OnPreRender2D()
        { }

        /// <summary>
        /// Called when the 3D world is to be drawn the render texture with a 3D projection matrix.
        /// </summary>
        protected virtual void OnRender3D()
        { }

        /// <summary>
        /// Called after drawing the 3D world on the render texture with a 2D pixel matrix.
        /// </summary>
        protected virtual void OnPostRender2D()
        { }

        /// <summary>
        /// Called at the end of the control's <see cref="OnRender"/> function. This draws
        /// on the normal screen.
        /// </summary>
        protected virtual void OnPostRender()
        { }
    }
}

#endif