#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    // contains source code from https://github.com/Genbox/VelcroPhysics/ (see Licenses/VelcroPhysics.txt).

    /// <summary>
    /// Convex decomposition algorithm created by Mark Bayazit.
    ///
    /// Properties:
    /// - Tries to decompose using polygons instead of triangles.
    /// - Tends to produce optimal results with low processing time.
    /// - Running time is O(nr), n = number of vertices, r = reflex vertices.
    /// - Does not support holes.
    ///
    /// For more information about this algorithm, see https://mpen.ca/406/bayazit
    /// </summary>
    public static class BayazitDecomposer
    {
        private static int MaxPolygonVertices = 1024; // henry: used to be 8, but we want less CSG brushes / meshes.
        private const float SingularityEpsilon = 1e-4f;
        private const float SingularityEpsilonSqr = SingularityEpsilon * SingularityEpsilon;

        /// <summary>
        /// [2D] Decompose the polygon into several smaller non-concave polygon. If the polygon is
        /// already convex, it will return the original polygon, unless it is over Settings.MaxPolygonVertices.
        /// </summary>
        public static List<Polygon> ConvexPartition(Polygon vertices)
        {
            Debug.Assert(vertices.Count >= 3);
            Debug.Assert(vertices.IsCounterClockWise2D());

            if (TrySplitAtSingularity(vertices, out var splitPolygons))
            {
                var splitResult = new List<Polygon>();
                for (int i = 0; i < splitPolygons.Count; i++)
                    splitResult.AddRange(ConvexPartition(splitPolygons[i]));
                return splitResult;
            }

            var depth = 800;
            var result = TriangulatePolygon(vertices, ref depth);

            if (depth <= 0)
                Debug.LogError("Prevented Stack Overflow and Editor Crash in BayazitDecomposer! Try to avoid thin lines (like the middle left side of the character 'B'). Internal Boolean operations have probably mistakenly considered your shapes as a single shape with an infinitely thin point connecting them, causing the convex decomposition to fail. The splitting of this type of polygon at the singularity has yet to be fixed. PR is welcome. It could also be that your shape is too complex with hundreds of vertices.");

            return result;
        }

        private static bool TrySplitAtSingularity(Polygon vertices, out List<Polygon> polygons)
        {
            polygons = null;

            if (vertices.Count < 4)
                return false;

            if (TrySplitAtCoincidentVertex(vertices, out polygons))
                return true;

            return TrySplitAtVertexOnEdge(vertices, out polygons);
        }

        private static bool TrySplitAtCoincidentVertex(Polygon vertices, out List<Polygon> polygons)
        {
            polygons = null;
            var count = vertices.Count;

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (AreAdjacent(i, j, count))
                        continue;

                    if (!SamePosition((Vector2)vertices[i].position, (Vector2)vertices[j].position))
                        continue;

                    var first = CopyRange(i, j, vertices);
                    var second = CopyRange(j, i, vertices);
                    if (TryCreateSplitResult(vertices, first, second, out polygons))
                        return true;
                }
            }

            return false;
        }

        private static bool TrySplitAtVertexOnEdge(Polygon vertices, out List<Polygon> polygons)
        {
            polygons = null;
            var count = vertices.Count;

            for (int vertexIndex = 0; vertexIndex < count; vertexIndex++)
            {
                var point = (Vector2)vertices[vertexIndex].position;

                for (int edgeStartIndex = 0; edgeStartIndex < count; edgeStartIndex++)
                {
                    var edgeEndIndex = (edgeStartIndex + 1) % count;
                    if (vertexIndex == edgeStartIndex || vertexIndex == edgeEndIndex)
                        continue;

                    var edgeStart = (Vector2)vertices[edgeStartIndex].position;
                    var edgeEnd = (Vector2)vertices[edgeEndIndex].position;
                    if (!PointIsInsideEdge(point, edgeStart, edgeEnd))
                        continue;

                    var first = CopyRange(vertexIndex, edgeStartIndex, vertices);
                    first.Add(CreateSplitVertex(vertices[vertexIndex], point));

                    var second = new Polygon();
                    second.Add(CreateSplitVertex(vertices[vertexIndex], point));
                    second.AddRange(CopyRange(edgeEndIndex, vertexIndex, vertices));

                    if (TryCreateSplitResult(vertices, first, second, out polygons))
                        return true;
                }
            }

            return false;
        }

        private static bool TryCreateSplitResult(Polygon source, Polygon first, Polygon second, out List<Polygon> polygons)
        {
            polygons = null;

            if (!PrepareSplitPolygon(source, first) || !PrepareSplitPolygon(source, second))
                return false;

            polygons = new List<Polygon> { first, second };
            return true;
        }

        private static bool PrepareSplitPolygon(Polygon source, Polygon polygon)
        {
            RemoveDuplicateVertices(polygon);
            polygon.CollinearSimplify(SingularityEpsilon);

            if (polygon.Count < 3 || Mathf.Abs(polygon.GetSignedArea2D()) <= SingularityEpsilon)
                return false;

            polygon.ForceCounterClockWise2D();
            polygon.booleanOperator = source.booleanOperator;
            return true;
        }

        private static Polygon CopyRange(int start, int end, Polygon vertices)
        {
            var count = vertices.Count;
            while (end < start)
                end += count;

            var result = new Polygon(end - start + 1);
            for (int i = start; i <= end; i++)
            {
                var vertex = vertices[i % count];
                result.Add(new Vertex(vertex.position, vertex.uv0, vertex.hidden, vertex.material));
            }
            return result;
        }

        private static void RemoveDuplicateVertices(Polygon polygon)
        {
            for (int i = polygon.Count - 1; i > 0; i--)
            {
                if (SamePosition((Vector2)polygon[i].position, (Vector2)polygon[i - 1].position))
                    polygon.RemoveAt(i);
            }

            if (polygon.Count > 1 && SamePosition((Vector2)polygon[0].position, (Vector2)polygon[polygon.Count - 1].position))
                polygon.RemoveAt(polygon.Count - 1);
        }

        private static Vertex CreateSplitVertex(Vertex template, Vector2 position)
        {
            return new Vertex(new Vector3(position.x, position.y, template.position.z), template.uv0, template.hidden, template.material);
        }

        private static bool AreAdjacent(int first, int second, int count)
        {
            return Mathf.Abs(first - second) == 1 || Mathf.Abs(first - second) == count - 1;
        }

        private static bool SamePosition(Vector2 first, Vector2 second)
        {
            return SquareDist(first, second) <= SingularityEpsilonSqr;
        }

        private static bool PointIsInsideEdge(Vector2 point, Vector2 edgeStart, Vector2 edgeEnd)
        {
            if (SquareDist(point, edgeStart) <= SingularityEpsilonSqr || SquareDist(point, edgeEnd) <= SingularityEpsilonSqr)
                return false;

            var edge = edgeEnd - edgeStart;
            var pointOffset = point - edgeStart;
            var edgeLengthSqr = edge.sqrMagnitude;
            if (edgeLengthSqr <= SingularityEpsilonSqr)
                return false;

            var t = Vector2.Dot(pointOffset, edge) / edgeLengthSqr;
            if (t <= 0f || t >= 1f)
                return false;

            var closestPoint = edgeStart + edge * t;
            return SquareDist(point, closestPoint) <= SingularityEpsilonSqr;
        }

        private static List<Polygon> TriangulatePolygon(Polygon vertices, ref int depth)
        {
            if (depth-- <= 0) return new List<Polygon> { vertices };

            var list = new List<Polygon>();
            var lowerInt = new Vector3();
            var upperInt = new Vector3(); // intersection points
            int lowerIndex = 0, upperIndex = 0;
            Polygon lowerPoly, upperPoly;

            for (int i = 0; i < vertices.Count; ++i)
            {
                if (Reflex(i, vertices))
                {
                    float upperDist;
                    float lowerDist = upperDist = float.MaxValue;
                    for (int j = 0; j < vertices.Count; ++j)
                    {
                        // if line intersects with an edge
                        float d;
                        Vector3 p;
                        if (Left(At(i - 1, vertices), At(i, vertices), At(j, vertices)) && RightOn(At(i - 1, vertices), At(i, vertices), At(j - 1, vertices)))
                        {
                            // find the point of intersection
                            p = LineIntersect(At(i - 1, vertices), At(i, vertices), At(j, vertices), At(j - 1, vertices));

                            if (Right(At(i + 1, vertices), At(i, vertices), p))
                            {
                                // make sure it's inside the poly
                                d = SquareDist(At(i, vertices), p);
                                if (d < lowerDist)
                                {
                                    // keep only the closest intersection
                                    lowerDist = d;
                                    lowerInt = p;
                                    lowerIndex = j;
                                }
                            }
                        }

                        if (Left(At(i + 1, vertices), At(i, vertices), At(j + 1, vertices)) && RightOn(At(i + 1, vertices), At(i, vertices), At(j, vertices)))
                        {
                            p = LineIntersect(At(i + 1, vertices), At(i, vertices), At(j, vertices), At(j + 1, vertices));

                            if (Left(At(i - 1, vertices), At(i, vertices), p))
                            {
                                d = SquareDist(At(i, vertices), p);
                                if (d < upperDist)
                                {
                                    upperDist = d;
                                    upperIndex = j;
                                    upperInt = p;
                                }
                            }
                        }
                    }

                    // if there are no vertices to connect to, choose a point in the middle
                    if (lowerIndex == (upperIndex + 1) % vertices.Count)
                    {
                        var p = (lowerInt + upperInt) / 2;

                        lowerPoly = Copy(i, upperIndex, vertices);
                        lowerPoly.Add(new Vertex(p));
                        upperPoly = Copy(lowerIndex, i, vertices);
                        upperPoly.Add(new Vertex(p));
                    }
                    else
                    {
                        double highestScore = 0, bestIndex = lowerIndex;
                        while (upperIndex < lowerIndex)
                        {
                            upperIndex += vertices.Count;
                        }

                        for (int j = lowerIndex; j <= upperIndex; ++j)
                        {
                            if (CanSee(i, j, vertices))
                            {
                                double score = 1 / (SquareDist(At(i, vertices), At(j, vertices)) + 1);
                                if (Reflex(j, vertices))
                                {
                                    if (RightOn(At(j - 1, vertices), At(j, vertices), At(i, vertices)) && LeftOn(At(j + 1, vertices), At(j, vertices), At(i, vertices)))
                                        score += 3;
                                    else
                                        score += 2;
                                }
                                else
                                    score += 1;
                                if (score > highestScore)
                                {
                                    bestIndex = j;
                                    highestScore = score;
                                }
                            }
                        }
                        lowerPoly = Copy(i, (int)bestIndex, vertices);
                        upperPoly = Copy((int)bestIndex, i, vertices);
                    }
                    list.AddRange(TriangulatePolygon(lowerPoly, ref depth));
                    list.AddRange(TriangulatePolygon(upperPoly, ref depth));
                    return list;
                }
            }

            // polygon is already convex
            if (vertices.Count > MaxPolygonVertices)
            {
                lowerPoly = Copy(0, vertices.Count / 2, vertices);
                upperPoly = Copy(vertices.Count / 2, 0, vertices);
                list.AddRange(TriangulatePolygon(lowerPoly, ref depth));
                list.AddRange(TriangulatePolygon(upperPoly, ref depth));
            }
            else
                list.Add(vertices);

            return list;
        }

        private static Vector2 At(int i, Polygon vertices)
        {
            int s = vertices.Count;
            return vertices[i < 0 ? s - 1 - ((-i - 1) % s) : i % s].position;
        }

        private static Polygon Copy(int i, int j, Polygon vertices)
        {
            while (j < i)
            {
                j += vertices.Count;
            }

            var p = new Polygon(j);

            for (; i <= j; ++i)
            {
                p.Add(new Vertex(At(i, vertices)));
            }
            return p;
        }

        private static bool CanSee(int i, int j, Polygon vertices)
        {
            if (Reflex(i, vertices))
            {
                if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) && RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)))
                    return false;
            }
            else
            {
                if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) || LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)))
                    return false;
            }
            if (Reflex(j, vertices))
            {
                if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) && RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)))
                    return false;
            }
            else
            {
                if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) || LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)))
                    return false;
            }
            for (int k = 0; k < vertices.Count; ++k)
            {
                if ((k + 1) % vertices.Count == i || k == i || (k + 1) % vertices.Count == j || k == j)
                    continue; // ignore incident edges

                if (LineIntersect(At(i, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices), out _))
                    return false;
            }
            return true;
        }

        private static bool Reflex(int i, Polygon vertices)
        {
            return Right(i, vertices);
        }

        private static bool Right(int i, Polygon vertices)
        {
            return Right(At(i - 1, vertices), At(i, vertices), At(i + 1, vertices));
        }

        private static bool Left(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathEx.Area(ref a, ref b, ref c) > 0;
        }

        private static bool LeftOn(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathEx.Area(ref a, ref b, ref c) >= 0;
        }

        private static bool Right(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathEx.Area(ref a, ref b, ref c) < 0;
        }

        private static bool RightOn(Vector2 a, Vector2 b, Vector2 c)
        {
            return MathEx.Area(ref a, ref b, ref c) <= 0;
        }

        private static float SquareDist(Vector2 a, Vector2 b)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            return dx * dx + dy * dy;
        }

        private static Vector2 LineIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            var i = Vector2.zero;
            float a1 = p2.y - p1.y;
            float b1 = p1.x - p2.x;
            float c1 = a1 * p1.x + b1 * p1.y;
            float a2 = q2.y - q1.y;
            float b2 = q1.x - q2.x;
            float c2 = a2 * q1.x + b2 * q1.y;
            float det = a1 * b2 - a2 * b1;

            if (!FloatEquals(det, 0))
            {
                // lines are not parallel
                i.x = (b2 * c1 - b1 * c2) / det;
                i.y = (a1 * c2 - a2 * c1) / det;
            }
            return i;
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect, and, if so, the point of intersection. Use the
        /// <paramref name="firstIsSegment" /> and <paramref name="secondIsSegment" /> parameters to set whether the intersection
        /// point must be on the first and second line segments. Setting these both to true means you are doing a line-segment to
        /// line-segment intersection. Setting one of them to true means you are doing a line to line-segment intersection test,
        /// and so on. Note: If two line segments are coincident, then no intersection is detected (there are actually infinite
        /// intersection points). Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="point">This is set to the intersection point if an intersection is detected.</param>
        /// <param name="firstIsSegment">Set this to true to require that the intersection point be on the first line segment.</param>
        /// <param name="secondIsSegment">Set this to true to require that the intersection point be on the second line segment.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        private static bool LineIntersect(ref float2 point1, ref float2 point2, ref float2 point3, ref float2 point4, bool firstIsSegment, bool secondIsSegment, out float2 point)
        {
            point = new float2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = point4.y - point3.y;
            float b = point2.x - point1.x;
            float c = point4.x - point3.x;
            float d = point2.y - point1.y;

            // denominator to solution of linear system
            float denom = (a * b) - (c * d);

            // if denominator is 0, then lines are parallel
            if (!(denom >= -Epsilon && denom <= Epsilon))
            {
                float e = point1.y - point3.y;
                float f = point1.x - point3.x;
                float oneOverDenom = 1.0f / denom;

                // numerator of first equation
                float ua = (c * e) - (a * f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= 0.0f && ua <= 1.0f)
                {
                    // numerator of second equation
                    float ub = (b * e) - (d * f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= 0.0f && ub <= 1.0f)
                    {
                        // check if they are coincident (no collision in this case)
                        if (ua != 0f || ub != 0f)
                        {
                            //There is an intersection
                            point.x = point1.x + ua * b;
                            point.y = point1.y + ua * d;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method detects if two line segments intersect, and, if so, the point of intersection. Note: If two line
        /// segments are coincident, then no intersection is detected (there are actually infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        private static bool LineIntersect(float2 point1, float2 point2, float2 point3, float2 point4, out float2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        private const float Epsilon = 1.192092896e-07f;

        private static bool FloatEquals(float value1, float value2)
        {
            return math.abs(value1 - value2) <= Epsilon;
        }
    }
}

#endif