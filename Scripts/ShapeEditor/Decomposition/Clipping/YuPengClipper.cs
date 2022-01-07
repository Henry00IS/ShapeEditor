#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Provides "A new algorithm for Boolean operations on general polygons" with 2D CSG.
    /// </summary>
    public static class YuPengClipper
    {
        private const float ClipperEpsilonSquared = 1.192092896e-07f;

        public static List<Polygon> Union(Polygon polygon1, Polygon polygon2, out PolyClipError error)
        {
            return Execute(polygon1, polygon2, PolyClipType.Union, out error);
        }

        public static List<Polygon> Difference(Polygon polygon1, Polygon polygon2, out PolyClipError error)
        {
            return Execute(polygon1, polygon2, PolyClipType.Difference, out error);
        }

        public static List<Polygon> Intersect(Polygon polygon1, Polygon polygon2, out PolyClipError error)
        {
            return Execute(polygon1, polygon2, PolyClipType.Intersect, out error);
        }

        /// <summary>
        /// Implements "A new algorithm for Boolean operations on general polygons" available here:
        /// http://liama.ia.ac.cn/wiki/_media/user:dong:dong_cg_05.pdf Merges two polygons, a subject and a clip with the specified
        /// operation. Polygons may not be self-intersecting. Warning: May yield incorrect results or even crash if polygons
        /// contain collinear points.
        /// </summary>
        /// <param name="subject">The subject polygon.</param>
        /// <param name="clip">The clip polygon, which is added, substracted or intersected with the subject</param>
        /// <param name="clipType">The operation to be performed. Either Union, Difference or Intersection.</param>
        /// <param name="error">The error generated (if any).</param>
        /// <returns>
        /// A list of closed polygons, which make up the result of the clipping operation. Outer contours are ordered
        /// counter clockwise, holes are ordered clockwise.
        /// </returns>
        private static List<Polygon> Execute(Polygon subject, Polygon clip, PolyClipType clipType, out PolyClipError error)
        {
            Debug.Assert(subject.IsSimple() && clip.IsSimple(), "Input polygons must be simple (cannot intersect themselves).");

            // Copy polygons

            // Calculate the intersection and touch points between
            // subject and clip and add them to both
            CalculateIntersections(subject, clip, out Polygon slicedSubject, out Polygon slicedClip);

            // Translate polygons into upper right quadrant
            // as the algorithm depends on it
            Vector2 lbSubject = subject.GetAABB().min;
            Vector2 lbClip = clip.GetAABB().min;
            Vector2 translate = Vector2.Min(lbSubject, lbClip);
            translate = Vector2.one - translate;
            if (translate != Vector2.zero)
            {
                slicedSubject.Translate(translate);
                slicedClip.Translate(translate);
            }

            // Enforce counterclockwise contours
            slicedSubject.ForceCounterClockWise2D();
            slicedClip.ForceCounterClockWise2D();

            // Build simplical chains from the polygons and calculate the
            // the corresponding coefficients
            CalculateSimplicalChain(slicedSubject, out List<float> subjectCoeff, out List<Edge> subjectSimplices);
            CalculateSimplicalChain(slicedClip, out List<float> clipCoeff, out List<Edge> clipSimplices);

            // Determine the characteristics function for all non-original edges
            // in subject and clip simplical chain and combine the edges contributing
            // to the result, depending on the clipType
            CalculateResultChain(subjectCoeff, subjectSimplices, clipCoeff, clipSimplices, clipType,
                out List<Edge> resultSimplices);

            // Convert result chain back to polygon(s)
            error = BuildPolygonsFromChain(resultSimplices, out List<Polygon> result);

            // Reverse the polygon translation from the beginning
            // and remove collinear points from output
            translate *= -1f;
            for (int i = 0; i < result.Count; ++i)
            {
                result[i].Translate(translate);
                result[i].CollinearSimplify(); // henry: this doesn't do anything in VelcroPhysics but I fixed it here.
            }
            return result;
        }

        /// <summary>Calculates all intersections between two polygons.</summary>
        /// <param name="polygon1">The first polygon.</param>
        /// <param name="polygon2">The second polygon.</param>
        /// <param name="slicedPoly1">Returns the first polygon with added intersection points.</param>
        /// <param name="slicedPoly2">Returns the second polygon with added intersection points.</param>
        private static void CalculateIntersections(Polygon polygon1, Polygon polygon2, out Polygon slicedPoly1, out Polygon slicedPoly2)
        {
            slicedPoly1 = new Polygon(polygon1);
            slicedPoly2 = new Polygon(polygon2);

            // Iterate through polygon1's edges
            for (int i = 0; i < polygon1.Count; i++)
            {
                // Get edge vertices
                Vector2 a = polygon1[i].position;
                Vector2 b = polygon1[polygon1.NextIndex(i)].position;

                // Get intersections between this edge and polygon2
                for (int j = 0; j < polygon2.Count; j++)
                {
                    Vector2 c = polygon2[j].position;
                    Vector2 d = polygon2[polygon2.NextIndex(j)].position;

                    // Check if the edges intersect
                    if (MathEx.LineIntersect(a, b, c, d, out Vector2 intersectionPoint))
                    {
                        // calculate alpha values for sorting multiple intersections points on a edge
                        float alpha = GetAlpha(a, b, intersectionPoint);

                        // Insert intersection point into first polygon
                        if (alpha > 0f && alpha < 1f)
                        {
                            int index = slicedPoly1.IndexOf(new Vertex(a)) + 1; // henry: DOES THIS WORK?!
                            while (index < slicedPoly1.Count &&
                                   GetAlpha(a, b, slicedPoly1[index].position) <= alpha)
                            {
                                ++index;
                            }
                            slicedPoly1.Insert(index, new Vertex(intersectionPoint));
                        }

                        // Insert intersection point into second polygon
                        alpha = GetAlpha(c, d, intersectionPoint);
                        if (alpha > 0f && alpha < 1f)
                        {
                            int index = slicedPoly2.IndexOf(new Vertex(c)) + 1; // henry: DOES THIS WORK?!
                            while (index < slicedPoly2.Count &&
                                   GetAlpha(c, d, slicedPoly2[index].position) <= alpha)
                            {
                                ++index;
                            }
                            slicedPoly2.Insert(index, new Vertex(intersectionPoint));
                        }
                    }
                }
            }

            // Check for very small edges
            for (int i = 0; i < slicedPoly1.Count; ++i)
            {
                int iNext = slicedPoly1.NextIndex(i);

                //If they are closer than the distance remove vertex
                if ((slicedPoly1[iNext].position - slicedPoly1[i].position).sqrMagnitude <= ClipperEpsilonSquared)
                {
                    slicedPoly1.RemoveAt(i);
                    --i;
                }
            }
            for (int i = 0; i < slicedPoly2.Count; ++i)
            {
                int iNext = slicedPoly2.NextIndex(i);

                //If they are closer than the distance remove vertex
                if ((slicedPoly2[iNext].position - slicedPoly2[i].position).sqrMagnitude <= ClipperEpsilonSquared)
                {
                    slicedPoly2.RemoveAt(i);
                    --i;
                }
            }
        }

        /// <summary>Calculates the simplical chain corresponding to the input polygon.</summary>
        /// <remarks>Used by method <c>Execute()</c>.</remarks>
        private static void CalculateSimplicalChain(Polygon poly, out List<float> coeff, out List<Edge> simplicies)
        {
            simplicies = new List<Edge>();
            coeff = new List<float>();
            for (int i = 0; i < poly.Count; ++i)
            {
                simplicies.Add(new Edge(poly[i].position, poly[poly.NextIndex(i)].position));
                coeff.Add(CalculateSimplexCoefficient(Vector2.zero, poly[i].position, poly[poly.NextIndex(i)].position));
            }
        }

        /// <summary>
        /// Calculates the characteristics function for all edges of the given simplical chains and builds the result
        /// chain.
        /// </summary>
        /// <remarks>Used by method <c>Execute()</c>.</remarks>
        private static void CalculateResultChain(List<float> poly1Coeff, List<Edge> poly1Simplicies, List<float> poly2Coeff, List<Edge> poly2Simplicies, PolyClipType clipType, out List<Edge> resultSimplices)
        {
            resultSimplices = new List<Edge>();

            for (int i = 0; i < poly1Simplicies.Count; ++i)
            {
                float edgeCharacter = 0;
                if (poly2Simplicies.Contains(poly1Simplicies[i]))
                    edgeCharacter = 1f;
                else if (poly2Simplicies.Contains(-poly1Simplicies[i]) && clipType == PolyClipType.Union)
                    edgeCharacter = 1f;
                else
                {
                    for (int j = 0; j < poly2Simplicies.Count; ++j)
                    {
                        if (!poly2Simplicies.Contains(-poly1Simplicies[i]))
                        {
                            edgeCharacter += CalculateBeta(poly1Simplicies[i].GetCenter(),
                                poly2Simplicies[j], poly2Coeff[j]);
                        }
                    }
                }
                if (clipType == PolyClipType.Intersect)
                {
                    if (edgeCharacter == 1f)
                        resultSimplices.Add(poly1Simplicies[i]);
                }
                else
                {
                    if (edgeCharacter == 0f)
                        resultSimplices.Add(poly1Simplicies[i]);
                }
            }
            for (int i = 0; i < poly2Simplicies.Count; ++i)
            {
                float edgeCharacter = 0f;
                if (!resultSimplices.Contains(poly2Simplicies[i]) &&
                    !resultSimplices.Contains(-poly2Simplicies[i]))
                {
                    if (poly1Simplicies.Contains(-poly2Simplicies[i]) && clipType == PolyClipType.Union)
                        edgeCharacter = 1f;
                    else
                    {
                        edgeCharacter = 0f;
                        for (int j = 0; j < poly1Simplicies.Count; ++j)
                        {
                            if (!poly1Simplicies.Contains(poly2Simplicies[i]) && !poly1Simplicies.Contains(-poly2Simplicies[i]))
                            {
                                edgeCharacter += CalculateBeta(poly2Simplicies[i].GetCenter(),
                                    poly1Simplicies[j], poly1Coeff[j]);
                            }
                        }
                        if (clipType == PolyClipType.Intersect || clipType == PolyClipType.Difference)
                        {
                            if (edgeCharacter == 1f)
                                resultSimplices.Add(-poly2Simplicies[i]);
                        }
                        else
                        {
                            if (edgeCharacter == 0f)
                                resultSimplices.Add(poly2Simplicies[i]);
                        }
                    }
                }
            }
        }

        /// <summary>Calculates the polygon(s) from the result simplical chain.</summary>
        /// <remarks>Used by method <c>Execute()</c>.</remarks>
        private static PolyClipError BuildPolygonsFromChain(List<Edge> simplicies, out List<Polygon> result)
        {
            result = new List<Polygon>();
            PolyClipError errVal = PolyClipError.None;

            while (simplicies.Count > 0)
            {
                Polygon output = new Polygon();
                output.Add(new Vertex(simplicies[0].EdgeStart));
                output.Add(new Vertex(simplicies[0].EdgeEnd));
                simplicies.RemoveAt(0);
                bool closed = false;
                int index = 0;
                int count = simplicies.Count; // Needed to catch infinite loops
                while (!closed && simplicies.Count > 0)
                {
                    if (VectorEqual(output[output.Count - 1].position, simplicies[index].EdgeStart))
                    {
                        if (VectorEqual(simplicies[index].EdgeEnd, output[0].position))
                            closed = true;
                        else
                            output.Add(new Vertex(simplicies[index].EdgeEnd));
                        simplicies.RemoveAt(index);
                        --index;
                    }
                    else if (VectorEqual(output[output.Count - 1].position, simplicies[index].EdgeEnd))
                    {
                        if (VectorEqual(simplicies[index].EdgeStart, output[0].position))
                            closed = true;
                        else
                            output.Add(new Vertex(simplicies[index].EdgeStart));
                        simplicies.RemoveAt(index);
                        --index;
                    }
                    if (!closed)
                    {
                        if (++index == simplicies.Count)
                        {
                            if (count == simplicies.Count)
                            {
                                result = new List<Polygon>();
                                Debug.LogWarning("Undefined error while building result polygon(s).");
                                return PolyClipError.BrokenResult;
                            }
                            index = 0;
                            count = simplicies.Count;
                        }
                    }
                }
                if (output.Count < 3)
                {
                    errVal = PolyClipError.DegeneratedOutput;
                    Debug.LogWarning("Degenerated output polygon produced (vertices < 3).");
                }
                result.Add(output);
            }
            return errVal;
        }

        /// <summary>Needed to calculate the characteristics function of a simplex.</summary>
        /// <remarks>Used by method <c>CalculateEdgeCharacter()</c>.</remarks>
        private static float CalculateBeta(Vector2 point, Edge e, float coefficient)
        {
            float result = 0f;
            if (PointInSimplex(point, e))
                result = coefficient;
            if (PointOnLineSegment(Vector2.zero, e.EdgeStart, point) ||
                PointOnLineSegment(Vector2.zero, e.EdgeEnd, point))
                result = .5f * coefficient;
            return result;
        }

        /// <summary>Needed for sorting multiple intersections points on the same edge.</summary>
        /// <remarks>Used by method <c>CalculateIntersections()</c>.</remarks>
        private static float GetAlpha(Vector2 start, Vector2 end, Vector2 point)
        {
            return (point - start).sqrMagnitude / (end - start).sqrMagnitude;
        }

        /// <summary>Returns the coefficient of a simplex.</summary>
        /// <remarks>Used by method <c>CalculateSimplicalChain()</c>.</remarks>
        private static float CalculateSimplexCoefficient(Vector2 a, Vector2 b, Vector2 c)
        {
            float isLeft = MathEx.Area(ref a, ref b, ref c);
            if (isLeft < 0f)
                return -1f;

            if (isLeft > 0f)
                return 1f;

            return 0f;
        }

        /// <summary>Winding number test for a point in a simplex.</summary>
        /// <param name="point">The point to be tested.</param>
        /// <param name="edge">The edge that the point is tested against.</param>
        /// <returns>False if the winding number is even and the point is outside the simplex and True otherwise.</returns>
        private static bool PointInSimplex(Vector2 point, Edge edge)
        {
            Polygon polygon = new Polygon();
            polygon.Add(new Vertex(Vector2.zero));
            polygon.Add(new Vertex(edge.EdgeStart));
            polygon.Add(new Vertex(edge.EdgeEnd));
            Vector3 pointv3 = point;
            return polygon.ContainsPoint2D(ref pointv3) == 1;
        }

        /// <summary>Tests if a point lies on a line segment.</summary>
        /// <remarks>Used by method <c>CalculateBeta()</c>.</remarks>
        private static bool PointOnLineSegment(Vector2 start, Vector2 end, Vector2 point)
        {
            Vector2 segment = end - start;
            return MathEx.Area(ref start, ref end, ref point) == 0f &&
                   Vector2.Dot(point - start, segment) >= 0f &&
                   Vector2.Dot(point - end, segment) <= 0f;
        }

        private static bool VectorEqual(Vector2 vec1, Vector2 vec2)
        {
            return (vec2 - vec1).sqrMagnitude <= ClipperEpsilonSquared;
        }

        /// <summary>Specifies an Edge. Edges are used to represent simplicies in simplical chains</summary>
        private sealed class Edge
        {
            public Edge(Vector2 edgeStart, Vector2 edgeEnd)
            {
                EdgeStart = edgeStart;
                EdgeEnd = edgeEnd;
            }

            public Vector2 EdgeStart { get; private set; }
            public Vector2 EdgeEnd { get; private set; }

            public Vector2 GetCenter()
            {
                return (EdgeStart + EdgeEnd) / 2f;
            }

            public static Edge operator -(Edge e)
            {
                return new Edge(e.EdgeEnd, e.EdgeStart);
            }

            public override bool Equals(object obj)
            {
                // If parameter is null return false.
                if (obj == null)
                    return false;

                // If parameter cannot be cast to Point return false.
                return Equals(obj as Edge);
            }

            public bool Equals(Edge e)
            {
                // If parameter is null return false:
                if (e == null)
                    return false;

                // Return true if the fields match
                return VectorEqual(EdgeStart, e.EdgeStart) && VectorEqual(EdgeEnd, e.EdgeEnd);
            }

            public override int GetHashCode()
            {
                return EdgeStart.GetHashCode() ^ EdgeEnd.GetHashCode();
            }
        }
    }
}

#endif