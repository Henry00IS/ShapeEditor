#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private float2 gridOffset;
        private float gridScale = 16f;

        /// <summary>
        /// The initialized flag, used to scroll the project into the center of the window.
        /// </summary>
        private bool gridInitialized = false;

        /// <summary>
        /// Converts a point on the grid to the point on the screen.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point on the screen.</returns>
        private float2 GridPointToScreen(float2 point)
        {
            Rect viewportRect = GetViewportRect();
            return (point * gridScale) + gridOffset + new float2(viewportRect.x, viewportRect.y);
        }

        /// <summary>
        /// Converts a point on the screen to the point on the grid.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point on the grid.</returns>
        private float2 ScreenPointToGrid(float2 point)
        {
            Rect viewportRect = GetViewportRect();
            point -= new float2(viewportRect.x, viewportRect.y);
            float2 result = (point / gridScale) - (gridOffset / gridScale);
            return new float2(result.x, result.y);
        }

        private void DrawGrid()
        {
            // when the editor loads up always scroll to the center of the screen.
            if (!gridInitialized)
            {
                gridInitialized = true;
                GridResetOffset();
            }

            Rect viewportRect = GetViewportRect();

            var renderTexture = RenderTexture.GetTemporary(Mathf.CeilToInt(viewportRect.width), Mathf.CeilToInt(viewportRect.height));
            Graphics.SetRenderTarget(renderTexture);

            var gridMaterial = ShapeEditorResources.temporaryGridMaterial;
            gridMaterial.SetFloat("_offsetX", gridOffset.x);
            gridMaterial.SetFloat("_offsetY", gridOffset.y);
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
            GLUtilities.DrawRectangle(viewportRect.x, viewportRect.y, viewportRect.width, viewportRect.height);
            GL.End();
            GL.PopMatrix();

            renderTexture.Release();

            Handles.DrawSolidRectangleWithOutline(new Rect(GridPointToScreen(new float2(0f, 0f)) - halfPivotScale, new float2(pivotScale)), Color.white, Color.black);

            Handles.DrawSolidRectangleWithOutline(new Rect(GridPointToScreen(ScreenPointToGrid(mousePosition)) - halfPivotScale, new float2(pivotScale)), Color.white, Color.black);
        }

        /// <summary>Will reset the camera offset to the center of the viewport.</summary>
        private void GridResetOffset()
        {
            var viewportRect = GetViewportRect();
            gridOffset = new float2(viewportRect.width / 2f, (viewportRect.height - viewportRect.y) / 2f);
        }

        /// <summary>Will reset the camera zoom of the viewport.</summary>
        private void GridResetZoom()
        {
            gridScale = 16f;
        }
    }
}

#endif