#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private const float screenScale = 200f;
        private const float pivotScale = 9f;
        private const float halfPivotScale = pivotScale / 2f;
        private static readonly Color segmentColor = new Color(0.7f, 0.7f, 0.7f);
        private float2 gridOffset;
        private float gridZoom = 1f;
        private float gridSnap = 0.125f;

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
            return (point * screenScale * gridZoom) + gridOffset + new float2(viewportRect.x, viewportRect.y);
        }

        /// <summary>
        /// Converts a point on the grid to the point on the screen.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point on the screen.</returns>
        private float2 RenderTextureGridPointToScreen(float2 point)
        {
            return (point * screenScale * gridZoom) + gridOffset;
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
            float2 result = (point / screenScale / gridZoom) - (gridOffset / screenScale / gridZoom);
            return new float2(result.x, result.y);
        }

        private void DrawGrid(RenderTexture renderTexture)
        {
            var gridMaterial = ShapeEditorResources.temporaryGridMaterial;
            gridMaterial.SetFloat("_offsetX", gridOffset.x);
            gridMaterial.SetFloat("_offsetY", gridOffset.y);
            gridMaterial.SetFloat("_viewportWidth", renderTexture.width);
            gridMaterial.SetFloat("_viewportHeight", renderTexture.height);
            gridMaterial.SetFloat("_zoom", gridZoom);
            gridMaterial.SetFloat("_snap", gridSnap);
            gridMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.LoadIdentity();
            GLUtilities.DrawUvRectangle(docked ? -1 : 0, 0, renderTexture.width + (docked ? 2 : 0), renderTexture.height);
            GL.End();
            GL.PopMatrix();
        }

        private void DrawSegments()
        {
            GLUtilities.DrawGui(() =>
            {
                foreach (Shape shape in project.shapes)
                {
                    foreach (Segment segment in shape.segments)
                    {
                        Segment next = GetNextSegment(segment);
                        if (segment.type == SegmentType.Linear)
                        {
                            Vector2 p1 = RenderTextureGridPointToScreen(segment.position);
                            Vector2 p2 = RenderTextureGridPointToScreen(next.position);
                            GL.Color(segmentColor);
                            GLUtilities.DrawLine(1.0f, p1.x, p1.y, p2.x, p2.y);
                        }
                    }
                }
            });
        }

        private void DrawPivots()
        {
            GLUtilities.DrawGui(() =>
            {
                foreach (Shape shape in project.shapes)
                {
                    foreach (Segment segment in shape.segments)
                    {
                        float2 pos = RenderTextureGridPointToScreen(segment.position);
                        GLUtilities.DrawSolidRectangleWithOutline(pos.x - halfPivotScale, pos.y - halfPivotScale, pivotScale, pivotScale, Color.white, segment.selected ? Color.red : Color.black);
                    }
                }
            });
        }

        private void DrawRenderTexture(RenderTexture renderTexture)
        {
            GLUtilities.DrawGuiTextured(renderTexture, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(0, 0, renderTexture.width, renderTexture.height);
            });
        }

        private void DrawViewport()
        {
            // when the editor loads up always scroll to the center of the screen.
            if (!gridInitialized)
            {
                gridInitialized = true;
                GridResetOffset();
            }

            // create a render texture for the viewport.
            Rect viewportRect = GetViewportRect();
            var renderTexture = RenderTexture.GetTemporary(Mathf.FloorToInt(viewportRect.width), Mathf.FloorToInt(viewportRect.height + viewportRect.y));
            Graphics.SetRenderTarget(renderTexture);

            // draw everything to the render texture.
            GL.Clear(true, true, Color.red);
            DrawGrid(renderTexture);
            DrawSegments();
            DrawPivots();
            DrawWindows();

            // finish up and draw the render texture.
            Graphics.SetRenderTarget(null);
            DrawRenderTexture(renderTexture);
            renderTexture.Release();
        }

        /// <summary>Will reset the grid offset to the center of the viewport.</summary>
        private void GridResetOffset()
        {
            var viewportRect = GetViewportRect();
            gridOffset = new float2(Mathf.RoundToInt(viewportRect.width / 2f), Mathf.RoundToInt((viewportRect.height - viewportRect.y) / 2f));
        }

        /// <summary>Will reset the grid zoom.</summary>
        private void GridResetZoom()
        {
            gridZoom = 1f;
        }

        /// <summary>Attempts to find the closest segment at the specified grid position.</summary>
        /// <param name="position">The grid position to search at.</param>
        /// <returns>The segment if found or null.</returns>
        private Segment FindSegmentAtGridPosition(float2 position, float maxDistance)
        {
            float closestDistance = float.MaxValue;
            Segment result = null;

            foreach (Shape shape in project.shapes)
            {
                foreach (Segment segment in shape.segments)
                {
                    var distance = math.distance(position, segment.position);
                    if (distance < maxDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        result = segment;
                    }
                }
            }

            return result;
        }

        /// <summary>Attempts to find the closest segment at the specified screen position.</summary>
        /// <param name="position">The screen position to search at.</param>
        /// <returns>The segment if found or null.</returns>
        private Segment FindSegmentAtScreenPosition(float2 position, float maxDistance)
        {
            float closestDistance = float.MaxValue;
            Segment result = null;

            foreach (Shape shape in project.shapes)
            {
                foreach (Segment segment in shape.segments)
                {
                    var distance = math.distance(position, GridPointToScreen(segment.position));
                    if (distance < maxDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        result = segment;
                    }
                }
            }

            return result;
        }

        /// <summary>Iterates over all of the selected segments.</summary>
        private IEnumerable<Segment> ForEachSelectedSegment()
        {
            foreach (Shape shape in project.shapes)
            {
                foreach (Segment segment in shape.segments)
                {
                    if (segment.selected)
                        yield return segment;
                }
            }
        }
    }
}

#endif