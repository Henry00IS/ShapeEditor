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
        private static readonly Color segmentPivotSelectedColor = new Color(0.9f, 0.45f, 0.0f);
        private static readonly Color segmentPivotOutlineColor = new Color(1.0f, 0.5f, 0.0f);
        private float2 gridOffset;
        internal float gridZoom = 1f;
        internal float gridSnap = 0.125f;
        internal int renderTextureWidth = 1;
        internal int renderTextureHeight = 1;

        /// <summary>After rendering the pivots this variable holds the number of selected segments.</summary>
        internal int selectedSegmentsCount;
        /// <summary>
        /// After rendering the pivots this variable holds the average position of all selected
        /// segments. Only available when the selected segments count is greater than 0.
        /// </summary>
        internal float2 selectedSegmentsAveragePosition;

        /// <summary>
        /// The initialized flag, used to scroll the project into the center of the window.
        /// </summary>
        private bool gridInitialized = false;

        /// <summary>
        /// Converts a point on the grid to the point on the screen.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point on the screen.</returns>
        internal float2 GridPointToScreen(float2 point)
        {
            return (point * screenScale * gridZoom) + gridOffset;
        }

        /// <summary>
        /// Converts a point on the screen to the point on the grid.
        /// </summary>
        /// <param name="point">The point to convert.</param>
        /// <returns>The point on the grid.</returns>
        internal float2 ScreenPointToGrid(float2 point)
        {
            float2 result = (point / screenScale / gridZoom) - (gridOffset / screenScale / gridZoom);
            return new float2(result.x, result.y);
        }

        private void DrawGrid()
        {
            var gridMaterial = ShapeEditorResources.temporaryGridMaterial;
            gridMaterial.SetFloat("_offsetX", gridOffset.x);
            gridMaterial.SetFloat("_offsetY", gridOffset.y);
            gridMaterial.SetFloat("_viewportWidth", renderTextureWidth);
            gridMaterial.SetFloat("_viewportHeight", renderTextureHeight);
            gridMaterial.SetFloat("_zoom", gridZoom);
            gridMaterial.SetFloat("_snap", gridSnap);
            gridMaterial.SetPass(0);

            GL.Begin(GL.QUADS);
            GLUtilities.DrawUvRectangle(docked ? -1 : 0, 0, renderTextureWidth + (docked ? 2 : 0), renderTextureHeight);
            GL.End();
        }

        private void DrawSegments()
        {
            GLUtilities.DrawGui(() =>
            {
                // for every shape in the project:
                var shapesCount = project.shapes.Count;
                for (int i = 0; i < shapesCount; i++)
                {
                    var shape = project.shapes[i];

                    // for every segment in the project:
                    var segmentsCount = shape.segments.Count;
                    for (int j = 0; j < segmentsCount; j++)
                    {
                        // get the current segment and the next segment (wrapping around).
                        var segment = shape.segments[j];
                        var next = shape.segments[j + 1 >= segmentsCount ? 0 : j + 1];

                        if (segment.type == SegmentType.Linear)
                        {
                            Vector2 p1 = GridPointToScreen(segment.position);
                            Vector2 p2 = GridPointToScreen(next.position);
                            GLUtilities.DrawLine(1.0f, p1.x, p1.y, p2.x, p2.y, segment.selected ? segmentPivotOutlineColor : segmentColor, next.selected ? segmentPivotOutlineColor : segmentColor);
                        }
                    }
                }
            });
        }

        private void DrawPivots()
        {
            selectedSegmentsCount = 0;
            selectedSegmentsAveragePosition = float2.zero;

            GLUtilities.DrawGui(() =>
            {
                foreach (Shape shape in project.shapes)
                {
                    foreach (Segment segment in shape.segments)
                    {
                        float2 pos = GridPointToScreen(segment.position);
                        GLUtilities.DrawSolidRectangleWithOutline(pos.x - halfPivotScale, pos.y - halfPivotScale, pivotScale, pivotScale, segment.selected ? segmentPivotSelectedColor : Color.white, segment.selected ? segmentPivotOutlineColor : Color.black);

                        if (segment.selected)
                        {
                            selectedSegmentsCount++;
                            selectedSegmentsAveragePosition += pos;
                        }
                    }
                }
            });

            if (selectedSegmentsCount != 0)
                selectedSegmentsAveragePosition /= selectedSegmentsCount;
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

            // when no tool is active we fallback to the box select tool.
            ValidateTools();

            // create a render texture for the viewport.
            renderTextureWidth = Mathf.FloorToInt(position.width);
            renderTextureHeight = Mathf.FloorToInt(position.height);
            var renderTexture = RenderTexture.GetTemporary(renderTextureWidth, renderTextureHeight, 24);
            Graphics.SetRenderTarget(renderTexture);

            // prepare the clipping pass in the gui material.
            ShapeEditorResources.temporaryGuiMaterial.SetVector("_viewport", new Vector2(renderTextureWidth, renderTextureHeight));

            // draw everything to the render texture.
            GL.Clear(true, true, Color.red);
            GL.PushMatrix();
            GL.LoadPixelMatrix(0f, renderTextureWidth, renderTextureHeight, 0f);
            DrawGrid();
            DrawSegments();
            DrawPivots();
            DrawTool();
            DrawWidgets();
            DrawWindows();
            GL.PopMatrix();

            // finish up and draw the render texture.
            Graphics.SetRenderTarget(null);
            DrawRenderTexture(renderTexture);
            RenderTexture.ReleaseTemporary(renderTexture);
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
        internal Segment FindSegmentAtScreenPosition(float2 position, float maxDistance)
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

        internal struct FindSegmentLineResult
        {
            public Shape shape;
            public Segment segment1;
            public Segment segment2;
            public int segmentIndex1;
            public int segmentIndex2;
        }

        /// <summary>Attempts to find the closest segment line at the specified screen position.</summary>
        /// <param name="position">The screen position to search at.</param>
        /// <returns>The segments if found or null.</returns>
        internal bool FindSegmentLineAtScreenPosition(float2 position, float maxDistance, ref FindSegmentLineResult result)
        {
            bool found = false;
            float closestDistance = float.MaxValue;

            // for every shape in the project:
            var shapesCount = project.shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = project.shapes[i];

                // for every segment in the project:
                var segmentsCount = shape.segments.Count;
                for (int j = 0; j < segmentsCount; j++)
                {
                    // get the current segment and the next segment (wrapping around).
                    var segment = shape.segments[j];
                    var next = shape.segments[j + 1 >= segmentsCount ? 0 : j + 1];

                    if (segment.type == SegmentType.Linear)
                    {
                        Vector2 p1 = GridPointToScreen(segment.position);
                        Vector2 p2 = GridPointToScreen(next.position);

                        var distance = MathEx.PointDistanceFromLine(position, p1, p2);
                        if (distance < maxDistance && distance < closestDistance)
                        {
                            closestDistance = distance;
                            result.shape = shape;
                            result.segment1 = segment;
                            result.segment2 = next;
                            result.segmentIndex1 = j;
                            result.segmentIndex2 = j + 1 >= segmentsCount ? 0 : j + 1;
                            found = true;
                        }
                    }
                }
            }

            return found;
        }

        /// <summary>Iterates over all of the selected segments.</summary>
        internal IEnumerable<Segment> ForEachSelectedSegment()
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

        /// <summary>Iterates over all of the segments within the given rectangle.</summary>
        internal IEnumerable<Segment> ForEachSegmentInGridRect(Rect rect)
        {
            foreach (Shape shape in project.shapes)
            {
                foreach (Segment segment in shape.segments)
                {
                    if (rect.Contains(segment.position))
                        yield return segment;
                }
            }
        }

        /// <summary>Deletes the selected objects from the project.</summary>
        public void DeleteSelection()
        {
            RegisterUndo("Delete Selection");

            // for every shape in the project:
            var shapesCount = project.shapes.Count;
            for (int i = shapesCount; i-- > 0;)
            {
                var shape = project.shapes[i];

                // for every segment in the project:
                var segments = shape.segments;
                var segmentsCount = segments.Count;
                for (int j = segmentsCount; j-- > 0;)
                    if (segments[j].selected)
                        segments.RemoveAt(j);

                // remove the shape if it's empty.
                if (segments.Count == 0)
                    project.shapes.RemoveAt(i);
            }
        }
    }
}

#endif