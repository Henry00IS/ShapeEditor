#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private const float screenScale = 200f;
        internal const float pivotScale = 9f;
        internal const float halfPivotScale = pivotScale / 2f;
        internal static readonly Color segmentColor = new Color(0.7f, 0.7f, 0.7f);
        internal static readonly Color segmentPivotSelectedColor = new Color(0.9f, 0.45f, 0.0f);
        internal static readonly Color segmentPivotOutlineColor = new Color(1.0f, 0.5f, 0.0f);
        private float2 gridOffset;
        internal float gridZoom = 1f;
        internal float gridSnap = 0.125f;
        internal float angleSnap = 15f;
        internal int renderTextureWidth = 1;
        internal int renderTextureHeight = 1;

        /// <summary>
        /// Whether grid snapping is enabled by default. When not enabled the user has to hold down
        /// the control key to temporarily snap objects to grid increments.
        /// </summary>
        internal bool snapEnabled = true;

        /// <summary>
        /// Gets whether the user is currently snapping. Through settings or the control key.
        /// </summary>
        internal bool isSnapping => snapEnabled ? !isCtrlPressed : isCtrlPressed;

        /// <summary>After rendering the pivots this variable holds the total number of segments.</summary>
        internal int totalSegmentsCount;

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

                    // for every segment in the shape:
                    var segmentsCount = shape.segments.Count;
                    for (int j = 0; j < segmentsCount; j++)
                    {
                        // have the segment generator draw the segments.
                        shape.segments[j].generator.DrawSegments(this);
                    }
                }
            });
        }

        private void DrawPivots()
        {
            totalSegmentsCount = 0;
            selectedSegmentsCount = 0;
            selectedSegmentsAveragePosition = float2.zero;

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

                        float2 pos = GridPointToScreen(segment.position);
                        GLUtilities.DrawSolidRectangleWithOutline(pos.x - halfPivotScale, pos.y - halfPivotScale, pivotScale, pivotScale, segment.selected ? segmentPivotSelectedColor : Color.white, segment.selected ? segmentPivotOutlineColor : Color.black);

                        totalSegmentsCount++;
                        if (segment.selected)
                        {
                            selectedSegmentsCount++;
                            selectedSegmentsAveragePosition += pos;
                        }

                        // have the segment generator draw additional pivots.
                        segment.generator.DrawPivots(this);
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
            // add 19 pixels because of the thick top toolbar window.
            var viewportRect = GetViewportRect();
            gridOffset = new float2(Mathf.RoundToInt(viewportRect.width / 2f), Mathf.RoundToInt((viewportRect.height - viewportRect.y + 19f) / 2f));
        }

        /// <summary>Will reset the grid zoom.</summary>
        private void GridResetZoom()
        {
            gridZoom = 1f;
        }

        /// <summary>Attempts to find the closest segment at the specified screen position.</summary>
        /// <param name="position">The screen position to search at.</param>
        /// <returns>The segment if found or null.</returns>
        internal ISelectable FindSegmentAtScreenPosition(float2 position, float maxDistance)
        {
            float closestDistance = float.MaxValue;
            ISelectable result = null;

            foreach (var shape in project.shapes)
            {
                foreach (Segment segment in shape.segments)
                {
                    var distance = math.distance(position, GridPointToScreen(segment.position));
                    if (distance < maxDistance && distance < closestDistance)
                    {
                        closestDistance = distance;
                        result = segment;
                    }

                    // check for additional selectable objects in the segment generator.
                    foreach (var modifierSelectable in segment.generator.ForEachSelectableObject())
                    {
                        distance = math.distance(position, GridPointToScreen(modifierSelectable.position));
                        if (distance < maxDistance && distance < closestDistance)
                        {
                            closestDistance = distance;
                            result = modifierSelectable;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Attempts to find the shape at the specified screen position.</summary>
        /// <param name="position">The screen position to search at.</param>
        /// <param name="cycle">For cycling through shapes, the last found shape.</param>
        /// <returns>The shape if found or null.</returns>
        internal Shape FindShapeAtGridPosition(float2 position, Shape cycle = null)
        {
            bool foundLast = cycle == null;
            Shape candidate = null;

            foreach (var shape in ForEachShapeAtGridPosition(position))
            {
                // the candidate is always the first shape found.
                if (candidate == null)
                    candidate = shape;

                // if we encountered the last shape we found the next shape.
                if (foundLast)
                    return shape;

                // check whether this was the last shape.
                if (shape == cycle)
                    foundLast = true;
            }

            return candidate;
        }

        /// <summary>Iterates over all of the shapes at the specified screen position in reverse order.</summary>
        /// <param name="position">The screen position to search at.</param>
        internal IEnumerable<Shape> ForEachShapeAtGridPosition(float2 position)
        {
            var shapes = project.shapes;
            var shapesCount = shapes.Count;

            for (int i = shapesCount; i-- > 0;)
            {
                var shape = shapes[i];

                if (shape.ContainsPoint(new Vector3(position.x, position.y)) >= 0)
                    yield return shape;
            }
        }

        /// <summary>Attempts to find the closest segment line at the specified screen position.</summary>
        /// <param name="position">The screen position to search at.</param>
        /// <returns>The segments if found or null.</returns>
        internal Segment FindSegmentLineAtScreenPosition(float2 position, float maxDistance)
        {
            Segment result = null;
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
                    var p1 = GridPointToScreen(segment.position);
                    var p2 = GridPointToScreen(segment.next.position);

                    if (segment.generator.type == SegmentGeneratorType.Linear)
                    {
                        var distance = MathEx.PointDistanceFromLine(position, p1, p2);
                        if (distance < maxDistance && distance < closestDistance)
                        {
                            closestDistance = distance;
                            result = segment;
                        }
                    }
                    else
                    {
                        // check generated segments from the segment generator.
                        foreach (var point in segment.generator.ForEachAdditionalSegmentPoint())
                        {
                            p2 = GridPointToScreen(point);

                            var distance = MathEx.PointDistanceFromLine(position, p1, p2);
                            if (distance < maxDistance && distance < closestDistance)
                            {
                                closestDistance = distance;
                                result = segment;
                            }

                            p1 = p2;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Iterates over all of the selected objects.</summary>
        internal IEnumerable<ISelectable> ForEachSelectedObject()
        {
            foreach (var shape in project.shapes)
            {
                foreach (var segment in shape.segments)
                {
                    if (segment.selected)
                        yield return segment;

                    // check for additional selectable objects in the segment generator.
                    foreach (var modifierSelectable in segment.generator.ForEachSelectableObject())
                        if (modifierSelectable.selected)
                            yield return modifierSelectable;
                }
            }
        }

        /// <summary>Iterates over all of the selectable objects within the given rectangle.</summary>
        internal IEnumerable<ISelectable> ForEachSelectableInGridRect(Rect rect)
        {
            foreach (var shape in project.shapes)
            {
                foreach (var segment in shape.segments)
                {
                    if (rect.Contains(segment.position))
                        yield return segment;

                    // check for additional selectable objects in the segment generator.
                    foreach (var modifierSelectable in segment.generator.ForEachSelectableObject())
                        if (rect.Contains(modifierSelectable.position))
                            yield return modifierSelectable;
                }
            }
        }

        /// <summary>Iterates over all selected edges and returns the first segment.</summary>
        internal IEnumerable<Segment> ForEachSelectedEdge()
        {
            foreach (var shape in project.shapes)
                foreach (var segment in shape.segments)
                    if (segment.selected && segment.next.selected)
                        yield return segment;
        }
    }
}

#endif