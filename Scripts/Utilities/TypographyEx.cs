#if UNITY_EDITOR

using System.Collections.Generic;
using AeternumGames.ShapeEditor.OpenFont;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public static class TypographyEx
    {
        private static List<GlyphPointF>[] GetGlyphRegions(Glyph glyph)
        {
            // todo optimization: we can calculate the amount of points so we don't need to use a list.
            var regions = new List<GlyphPointF>[glyph.EndPoints.Length];
            for (int i = 0; i < regions.Length; i++)
                regions[i] = new List<GlyphPointF>();

            // todo optimization: we should be able to array copy based on the endpoints array instead of this loop.
            var endpoint = 0;
            var points = glyph.GlyphPoints;
            for (int i = 0; i < points.Length; i++)
            {
                regions[endpoint].Add(points[i]);
                if (i == glyph.EndPoints[endpoint])
                    endpoint++;
            }

            return regions;
        }

        public static Shape[] ToShapes(this Glyph glyph)
        {
            // split the points into multiple regions as defined by the end points.
            var regions = GetGlyphRegions(glyph);
            var shapes = new Shape[regions.Length];

            // iterate over every region:
            for (int r = 0; r < regions.Length; r++)
            {
                // create a shape.
                var shape = new Shape();
                shape.segments.Clear();
                shapes[r] = shape;

                // iterate over the compressed set of points with on-curve (regular) and off-curve (control) points.
                var points = regions[r];
                var pointsCount = points.Count;
                Segment lastOnCurve = null;
                float2 lastOffCurve = default;
                bool compressed = false;
                for (int i = 0; i < pointsCount; i++)
                {
                    var point = points[i];
                    var position = new float2(point.X, -point.Y);

                    if (point.onCurve)
                    {
                        compressed = false;
                        shape.AddSegment(lastOnCurve = new Segment(shape, position));
                    }
                    else
                    {
                        // first off-curve control point.
                        if (!compressed)
                        {
                            lastOnCurve.generator = new SegmentGenerator(lastOnCurve, SegmentGeneratorType.Bezier);
                            lastOnCurve.generator.bezierQuadratic = true;
                            lastOnCurve.generator.bezierPivot1.position = position;
                            lastOffCurve = position;
                            compressed = true;
                        }

                        // another off-curve control point with implicit on-curve point.
                        else if (compressed)
                        {
                            var missing_position = math.lerp(lastOffCurve, position, 0.5f);
                            shape.AddSegment(lastOnCurve = new Segment(shape, missing_position));
                            lastOffCurve = position;

                            lastOnCurve.generator = new SegmentGenerator(lastOnCurve, SegmentGeneratorType.Bezier);
                            lastOnCurve.generator.bezierQuadratic = true;
                            lastOnCurve.generator.bezierPivot1.position = position;
                        }
                    }
                }
            }

            return shapes;
        }
    }
}

#endif