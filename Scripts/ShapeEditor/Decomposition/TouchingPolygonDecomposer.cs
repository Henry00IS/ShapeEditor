#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using AeternumGames.ShapeEditor.PolyBoolCS;
using AeternumGames.ShapeEditor.LibTessDotNet.Double;

namespace AeternumGames.ShapeEditor
{
    internal static class TouchingPolygonDecomposer
    {
        // Relative spacing of single-precision coordinates. PolyBool operates in double
        // precision, but the returned mesh must agree at Unity's float resolution.
        private const double FloatRelativeSpacing = 1.1920928955078125e-7;
        private const double MinimumBoundaryTolerance = 1e-10;

        internal static bool HasContacts(SegmentList segments)
        {
            if (segments.Count < 2) return false;
            var endpoints = new List<Point>(segments.Count * 2);
            var minX = segments[0].start.x;
            var maxX = minX;
            var minY = segments[0].start.y;
            var maxY = minY;
            foreach (var segment in segments)
            {
                endpoints.Add(segment.start);
                endpoints.Add(segment.end);
                minX = Math.Min(minX, Math.Min(segment.start.x, segment.end.x));
                maxX = Math.Max(maxX, Math.Max(segment.start.x, segment.end.x));
                minY = Math.Min(minY, Math.Min(segment.start.y, segment.end.y));
                maxY = Math.Max(maxY, Math.Max(segment.start.y, segment.end.y));
            }

            // Selected boundary segments have two incident endpoints at ordinary vertices.
            // Sort by the longer axis to limit candidates while retaining PolyBool's exact
            // epsilon predicate. Approximate equality is not transitive, so grouping nearby
            // points into one representative would change which vertices count as contacts.
            var useX = maxX - minX >= maxY - minY;
            endpoints.Sort((a, b) => useX ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            for (var i = 0; i < endpoints.Count; i++)
            {
                var point = endpoints[i];
                var degree = 1;
                for (var j = i - 1; j >= 0 && SameCoordinate(point, endpoints[j], useX); j--)
                    if (Epsilon.pointsSame(point, endpoints[j]) && ++degree > 2) return true;
                for (var j = i + 1; j < endpoints.Count && SameCoordinate(point, endpoints[j], useX); j++)
                    if (Epsilon.pointsSame(point, endpoints[j]) && ++degree > 2) return true;
            }
            return false;
        }

        private static bool SameCoordinate(Point a, Point b, bool useX)
        {
            return useX ? Epsilon.pointsSameX(a, b) : Epsilon.pointsSameY(a, b);
        }

        internal static PolygonMesh ConvexPartition(SegmentList segments, bool hideInteriorEdges)
        {
            var tess = new Tess();
            var contours = new PolyBool().polygon(segments).regions;
            foreach (var contour in contours)
            {
                var vertices = new ContourVertex[contour.Count];
                for (var i = 0; i < contour.Count; i++)
                    vertices[i].Position = new Vec3(contour[i].x, contour[i].y, 0.0);
                tess.AddContour(vertices);
            }
            // The selected boundary already encodes the boolean result. Even-odd filling
            // preserves its holes regardless of the chainer's contour winding at contacts.
            // Keep triangles because convex face merging can become concave or acquire
            // duplicate vertices when narrow double-precision features round to floats.
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3, null, new Vec3(0.0, 0.0, 1.0));

            var triangles = new List<Point[]>();
            for (var i = 0; i < tess.ElementCount; i++)
            {
                var points = new Point[3];
                for (var j = 0; j < 3; j++)
                {
                    var index = tess.Elements[i * 3 + j];
                    if (index == Tess.Undef) throw new InvalidOperationException("The tessellator returned an incomplete triangle.");
                    var point = tess.Vertices[index].Position;
                    points[j] = new Point(point.X, point.Y);
                }
                triangles.Add(points);
            }
            // Restore source endpoints in double precision so adjacent cells share the same
            // boundary subdivisions. Splitting after float conversion could move a contact
            // or create two different rounded vertices for one original endpoint.
            if (hideInteriorEdges)
                foreach (var segment in segments)
                {
                    SplitEdgesAtPoint(triangles, segment.start);
                    SplitEdgesAtPoint(triangles, segment.end);
                }

            var result = new PolygonMesh();
            foreach (var points in triangles)
            {
                var polygon = new Polygon();
                foreach (var point in points)
                    polygon.Add(new Vertex((float)point.x, (float)point.y));
                var area = SignedDoubleArea(polygon);
                if (area == 0.0) continue;
                if (area < 0.0)
                {
                    polygon.Reverse();
                    Array.Reverse(points);
                }
                if (hideInteriorEdges)
                    for (var j = 0; j < 3; j++)
                    {
                        var vertex = polygon[j];
                        vertex.hidden = !IsBoundaryEdge(points[j], points[(j + 1) % 3], segments);
                        polygon[j] = vertex;
                    }
                result.Add(polygon);
            }
            // Tolerant boundary coverage handles float rounding. Exact reciprocal edges in
            // the final float mesh are interior by topology, even if tolerance classified
            // them as boundary. Both cells use the same rounded endpoints after splitting.
            if (hideInteriorEdges) HideSharedEdges(result);
            return result;
        }

        private static void HideSharedEdges(PolygonMesh polygons)
        {
            var edges = new Dictionary<(float, float, float, float), List<(Polygon polygon, int vertex)>>();
            foreach (var polygon in polygons)
                for (var i = 0; i < polygon.Count; i++)
                {
                    var a = polygon[i];
                    var b = polygon[(i + 1) % polygon.Count];
                    var sorted = a.x < b.x || (a.x == b.x && a.y < b.y);
                    var key = sorted ? (a.x, a.y, b.x, b.y) : (b.x, b.y, a.x, a.y);
                    List<(Polygon polygon, int vertex)> previous;
                    if (!edges.TryGetValue(key, out previous))
                    {
                        previous = new List<(Polygon polygon, int vertex)>();
                        edges.Add(key, previous);
                    }
                    foreach (var edge in previous)
                    {
                        var otherA = edge.polygon[edge.vertex];
                        var otherB = edge.polygon[(edge.vertex + 1) % edge.polygon.Count];
                        if (a.x != otherB.x || a.y != otherB.y || b.x != otherA.x || b.y != otherA.y) continue;
                        otherA.hidden = true;
                        edge.polygon[edge.vertex] = otherA;
                        a.hidden = true;
                        polygon[i] = a;
                    }
                    previous.Add((polygon, i));
                }
        }

        private static void SplitEdgesAtPoint(List<Point[]> triangles, Point point)
        {
            for (var i = 0; i < triangles.Count; i++)
            {
                var triangle = triangles[i];
                for (var edge = 0; edge < 3; edge++)
                {
                    var a = triangle[edge];
                    var b = triangle[(edge + 1) % 3];
                    var c = triangle[(edge + 2) % 3];
                    if (SameFloatPosition(point, a) || SameFloatPosition(point, b) || !OnLine(point, a, b)) continue;
                    var useX = Math.Abs(b.x - a.x) >= Math.Abs(b.y - a.y);
                    var length = useX ? b.x - a.x : b.y - a.y;
                    if (length == 0.0) continue;
                    var fraction = (useX ? point.x - a.x : point.y - a.y) / length;
                    if (fraction <= 0.0 || fraction >= 1.0) continue;
                    triangles[i] = new[] { a, point, c };
                    triangles.Add(new[] { point, b, c });
                    break;
                }
            }
        }

        private static bool SameFloatPosition(Point a, Point b)
        {
            // A distinct double endpoint needs no subdivision when Unity merges it with
            // an existing float vertex. The resulting zero-width interval has no surface.
            return (float)a.x == (float)b.x && (float)a.y == (float)b.y;
        }

        private static double SignedDoubleArea(Polygon polygon)
        {
            return ((double)polygon[1].x - polygon[0].x) * ((double)polygon[2].y - polygon[0].y) -
                ((double)polygon[1].y - polygon[0].y) * ((double)polygon[2].x - polygon[0].x);
        }

        private static bool IsBoundaryEdge(Point a, Point b, SegmentList segments)
        {
            var useX = Math.Abs(b.x - a.x) >= Math.Abs(b.y - a.y);
            var start = useX ? a.x : a.y;
            var length = (useX ? b.x : b.y) - start;
            if (length == 0.0) return false;
            var tolerance = Math.Max(FloatTolerance(a), FloatTolerance(b));
            var fractionTolerance = tolerance / Math.Abs(length);
            var ranges = new List<(double lower, double upper)>();
            foreach (var segment in segments)
            {
                if (!OnBoundaryLine(segment.start, a, b, tolerance) || !OnBoundaryLine(segment.end, a, b, tolerance)) continue;
                var first = ((useX ? segment.start.x : segment.start.y) - start) / length;
                var last = ((useX ? segment.end.x : segment.end.y) - start) / length;
                var lower = Math.Max(0.0, Math.Min(first, last));
                var upper = Math.Min(1.0, Math.Max(first, last));
                if (upper > lower) ranges.Add((lower, upper));
            }
            ranges.Sort((x, y) => x.lower.CompareTo(y.lower));
            var covered = 0.0;
            foreach (var range in ranges)
            {
                if (range.lower > covered + fractionTolerance) return false;
                covered = Math.Max(covered, range.upper);
                if (covered >= 1.0 - fractionTolerance) return true;
            }
            return false;
        }

        private static double FloatTolerance(Point point)
        {
            // Distance in project-coordinate units, conservatively bounded by the less
            // precise axis. Inputs must be finite and features distinguishable as floats.
            return Math.Max(MinimumBoundaryTolerance,
                Math.Max(Math.Abs(point.x), Math.Abs(point.y)) * FloatRelativeSpacing);
        }

        private static bool OnBoundaryLine(Point point, Point a, Point b, double tolerance)
        {
            var dx = b.x - a.x;
            var dy = b.y - a.y;
            var distance = Math.Abs(dx * (point.y - a.y) - dy * (point.x - a.x));
            return distance <= Math.Max(tolerance, FloatTolerance(point)) * Math.Sqrt(dx * dx + dy * dy);
        }

        private static bool OnLine(Point point, Point a, Point b)
        {
            // Subdivision follows PolyBool's double-precision cross-product tolerance.
            // Float-distance tolerance is reserved for final boundary visibility.
            return Epsilon.pointAboveOrOnLine(point, a, b) && Epsilon.pointAboveOrOnLine(point, b, a);
        }

    }
}

#endif
