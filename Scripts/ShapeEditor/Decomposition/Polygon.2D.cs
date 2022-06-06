#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of vertex points that make up a 2D/3D polygon.</summary>
    public partial class Polygon : List<Vertex>
    {
        /// <summary>
        /// [2D] Indicates if the vertices are in counter clockwise order. Warning: If the area of
        /// the polygon is 0, it is unable to determine the winding.
        /// </summary>
        public bool IsCounterClockWise2D()
        {
            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (Count < 3)
                return false;

            return GetSignedArea2D() > 0.0f;
        }

        /// <summary>[2D] Forces the vertices to be counter clock wise order.</summary>
        public void ForceCounterClockWise2D()
        {
            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (Count < 3)
                return;

            if (!IsCounterClockWise2D())
                Reverse();
        }

        /// <summary>
        /// [2D] Gets the signed area. If the area is less than 0, it indicates that the polygon is
        /// clockwise winded.
        /// </summary>
        /// <returns>The signed area.</returns>
        public float GetSignedArea2D()
        {
            int count = Count;

            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (count < 3)
                return 0.0f;

            int i;
            float area = 0f;

            for (i = 0; i < count; i++)
            {
                int j = (i + 1) % count;

                var vi = this[i];
                var vj = this[j];

                area += vi.x * vj.y;
                area -= vi.y * vj.x;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>[2D] Winding number test for a point in a polygon.</summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>
        /// -1 if the winding number is zero and the point is outside the polygon, 1 if the point is inside the polygon,
        /// and 0 if the point is on the polygons edge.
        /// </returns>
        public int ContainsPoint2D(ref Vector3 point, float collinearEpsilon = 0f)
        {
            int count = Count;

            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (count < 3)
                return -1;

            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < count; i++)
            {
                // Get points
                var p1 = this[i];
                var p2 = this[NextIndex(i)];

                // Test if a point is directly on the edge
                var edge = p2.position - p1.position;
                float area = MathEx.Area2D(ref p1.position, ref p2.position, ref point);
                if (Mathf.Abs(area) <= collinearEpsilon && Vector2.Dot(point - p1.position, edge) >= 0f && Vector2.Dot(point - p2.position, edge) <= 0f)
                    return 0;

                // Test edge for intersection with ray from point
                if (p1.y <= point.y)
                {
                    if (p2.y > point.y && area > 0f)
                        ++wn;
                }
                else
                {
                    if (p2.y <= point.y && area < 0f)
                        --wn;
                }
            }
            return wn == 0 ? -1 : 1;
        }

        /// <summary>
        /// [2D] A simple UV algorithm that takes the vertex local position on the X and Y axis and
        /// converts them to U and V coordinates. Since the 2D Shape Editor works in the metric
        /// scale, textures will also cover 1m² in 3D space.
        /// </summary>
        /// <param name="offset">The offset to be added to the UV coordinates.</param>
        public void ApplyXYBasedUV0(Vector2 offset)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                var vertex = this[i];
                this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.x, offset.y + vertex.position.y), vertex.hidden);
            }
        }

        /// <summary>[2D] Converts the vertex positions to an array of vectors.</summary>
        /// <returns>The array of vectors.</returns>
        public Vector2[] GetVertices2D()
        {
            int count = Count;
            var vertices = new Vector2[count];
            for (int i = 0; i < count; i++)
                vertices[i] = this[i].position;
            return vertices;
        }

        /// <summary>[2D] Checks whether the vertices form a simple polygon by checking for edge crossings.</summary>
        public bool IsSimple()
        {
            var count = Count;

            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (count < 3)
                return false;

            for (int i = 0; i < count; ++i)
            {
                var a1 = this[i];
                var a2 = NextVertex(i);
                for (int j = i + 1; j < Count; ++j)
                {
                    var b1 = this[j];
                    var b2 = NextVertex(j);

                    if (MathEx.LineIntersect2(a1.position, a2.position, b1.position, b2.position, out _))
                        return false;
                }
            }
            return true;
        }

        /// <summary>[2D] Returns an AABB that fully contains this polygon.</summary>
        public Bounds GetAABB()
        {
            var count = Count;
            var lowerBound = new Vector2(float.MaxValue, float.MaxValue);
            var upperBound = new Vector2(float.MinValue, float.MinValue);

            for (int i = 0; i < count; i++)
            {
                var x = this[i].x;
                if (x < lowerBound.x)
                    lowerBound.x = x;
                if (x > upperBound.x)
                    upperBound.x = x;

                var y = this[i].y;
                if (y < lowerBound.y)
                    lowerBound.y = y;
                if (y > upperBound.y)
                    upperBound.y = y;
            }

            var aabb = new Bounds();
            aabb.SetMinMax(lowerBound, upperBound);
            return aabb;
        }

        /// <summary>[2D] Removes all collinear points on the polygon.</summary>
        /// <param name="collinearityTolerance">The collinearity tolerance.</param>
        public void CollinearSimplify(float collinearityTolerance = 0)
        {
            var count = Count;

            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (count < 3)
                return;

            Polygon simplified = new Polygon(count);

            for (int i = 0; i < count; i++)
            {
                Vector2 prev = PreviousVertex(i).position;
                Vector2 current = this[i].position;
                Vector2 next = NextVertex(i).position;

                // if they are collinear, continue.
                if (MathEx.IsCollinear(ref prev, ref current, ref next, collinearityTolerance))
                    continue;

                simplified.Add(this[i]);
            }

            Clear();
            AddRange(simplified);
        }

        /// <summary>
        /// [2D] Assuming the polygon is convex, checks whether it fully contains another polygon.
        /// </summary>
        /// <param name="other">The other polygon to check.</param>
        /// <returns>True when the other polygon is fully contained, else false.</returns>
        public bool ConvexContains(Polygon other)
        {
            var otherCount = other.Count;
            for (int i = 0; i < otherCount; i++)
            {
                var pos = other[i].position;
                if (ContainsPoint2D(ref pos) == -1)
                    return false;
            }
            return true;
        }
    }
}

#endif