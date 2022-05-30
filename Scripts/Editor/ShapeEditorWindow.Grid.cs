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
        internal static readonly Color gridBackgroundColor = new Color(0.118f, 0.118f, 0.118f);
        internal static readonly Color gridLinesColor = new Color(0.206f, 0.206f, 0.206f);
        internal static readonly Color gridSectionLinesColor = new Color(0.250f, 0.250f, 0.250f);
        internal static readonly Color gridCenterLineXColor = new Color(0.400f, 0.218f, 0.218f);
        internal static readonly Color gridCenterLineYColor = new Color(0.218f, 0.400f, 0.218f);
        internal static readonly Color segmentColor = new Color(0.7f, 0.7f, 0.7f);
        internal static readonly Color segmentColorDifference = new Color(1.0f, 0.5f, 0.5f);
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
            if (gridSnap <= 0f) return; // prevent infinite loop.

            // an adaptive grid that can be enlarged indefinitely. Lines that become too small are
            // hidden at the right time, so that the outer lines become the inner ones again.

            var modifiedGridSnap = gridSnap;
            var screenPoint1 = GridPointToScreen(modifiedGridSnap);
            var screenPoint2 = GridPointToScreen(modifiedGridSnap * 2f);
            var factor = Mathf.Clamp01(Mathf.Round(screenPoint2.x - screenPoint1.x) / 8f);
            while (factor <= 0.25f)
            {
                modifiedGridSnap *= 4f;

                screenPoint1 = GridPointToScreen(modifiedGridSnap);
                screenPoint2 = GridPointToScreen(modifiedGridSnap * 2f);
                factor = Mathf.Clamp01(Mathf.Round(screenPoint2.x - screenPoint1.x) / 8f);
            }

            // an interpolant to determine when the next grid size has been reached. the zoom
            // formula never gets to 0.25f so we switch to 0.0f earlier.
            factor = Mathf.InverseLerp(0.35f, 1.0f, factor);

            var bounds = new float4(ScreenPointToGrid(new float2(0f, 0f)), ScreenPointToGrid(new float2(renderTextureWidth, renderTextureHeight)));
            float x = bounds.x.Snap(modifiedGridSnap);
            float y = bounds.y.Snap(modifiedGridSnap);
            var gridSnap_x4 = modifiedGridSnap * 4f;
            var gridSnap_x8 = modifiedGridSnap * 8f;

            Color sectionLinesColor = Color.Lerp(gridSectionLinesColor, gridLinesColor, 1f - factor);
            Color smallLinesColor = new Color(gridLinesColor.r, gridLinesColor.g, gridLinesColor.b, factor);
            Color nextSectionLinesColor = Color.Lerp(gridSectionLinesColor, gridLinesColor, factor);

            GLUtilities.DrawGui(() =>
            {
                while (x < bounds.z)
                {
                    var mainColor = math.fmod(x, gridSnap_x4) == 0f ? sectionLinesColor : smallLinesColor;
                    var nextColor = math.fmod(x, gridSnap_x8) == 0f ? nextSectionLinesColor : mainColor;
                    mainColor = Color.Lerp(mainColor, nextColor, 0.5f);

                    GLUtilities.DrawGridLine(GridPointToScreen(new float2(x, bounds.w)), GridPointToScreen(new float2(x, bounds.y)), mainColor);
                    x += modifiedGridSnap;
                }

                while (y < bounds.w)
                {
                    var mainColor = math.fmod(y, gridSnap_x4) == 0f ? sectionLinesColor : smallLinesColor;
                    var nextColor = math.fmod(y, gridSnap_x8) == 0f ? nextSectionLinesColor : mainColor;
                    mainColor = Color.Lerp(mainColor, nextColor, 0.5f);

                    GLUtilities.DrawGridLine(GridPointToScreen(new float2(bounds.x, y)), GridPointToScreen(new float2(bounds.z, y)), mainColor);
                    y += modifiedGridSnap;
                }

                GLUtilities.DrawGridLine(GridPointToScreen(new float2(bounds.x, 0f)), GridPointToScreen(new float2(bounds.z, 0f)), gridCenterLineXColor);
                GLUtilities.DrawGridLine(GridPointToScreen(new float2(0f, bounds.w)), GridPointToScreen(new float2(0f, bounds.y)), gridCenterLineYColor);
            });
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
            var previousGridPosition = new float2(float.NegativeInfinity);
            List<float2> duplicateSegmentPositions = null;

            GLUtilities.DrawGui(() =>
            {
                // for every shape in the project:
                var shapesCount = project.shapes.Count;
                for (int i = 0; i < shapesCount; i++)
                {
                    var shape = project.shapes[i];
                    var segmentsCount = shape.segments.Count;

                    // begin detecting duplicate segments from the last segment.
                    if (segmentsCount > 0)
                        previousGridPosition = shape.segments[0].previous.position;

                    // for every segment in the project:
                    for (int j = 0; j < segmentsCount; j++)
                    {
                        // get the current segment and the next segment (wrapping around).
                        var segment = shape.segments[j];

                        float2 gridPosition = segment.position;
                        float2 screenPosition = GridPointToScreen(gridPosition);
                        GLUtilities.DrawSolidRectangleWithOutline(screenPosition.x - halfPivotScale, screenPosition.y - halfPivotScale, pivotScale, pivotScale, segment.selected ? segmentPivotSelectedColor : Color.white, segment.selected ? segmentPivotOutlineColor : Color.black);

                        // detect duplicate segments at the same position as the last segment.
                        if (gridPosition.Equals(previousGridPosition))
                        {
                            // only allocate a list when this condition occurs.
                            if (duplicateSegmentPositions == null) duplicateSegmentPositions = new List<float2>();
                            var normal = math.normalize(gridPosition - segment.previous.previous.position);
                            duplicateSegmentPositions.Add(math.floor(screenPosition + normal * 10f));
                        }
                        previousGridPosition = gridPosition;

                        totalSegmentsCount++;
                        if (segment.selected)
                        {
                            selectedSegmentsCount++;
                            selectedSegmentsAveragePosition += screenPosition;
                        }

                        // have the segment generator draw additional pivots.
                        segment.generator.DrawPivots(this);
                    }
                }
            });

            // draw warning icons at duplicate segments.
            if (duplicateSegmentPositions != null)
            {
                GLUtilities.DrawGuiTextured(ShapeEditorResources.Instance.shapeEditorSegmentDuplicateWarning, () =>
                {
                    var duplicateSegmentPositionsCount = duplicateSegmentPositions.Count;
                    for (int i = 0; i < duplicateSegmentPositionsCount; i++)
                    {
                        var duplicateSegmentPosition = duplicateSegmentPositions[i];
                        GLUtilities.DrawFlippedUvRectangle(duplicateSegmentPosition.x - 4, duplicateSegmentPosition.y - 4, 7, 7);
                    }
                });
            }

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
            GL.Clear(true, true, gridBackgroundColor);
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