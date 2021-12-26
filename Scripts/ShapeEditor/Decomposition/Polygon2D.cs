#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of vertex points that make up a 2D polygon.</summary>
    public class Polygon2D : List<float2>
    {
        public Polygon2D()
        {
        }

        public Polygon2D(int capacity) : base(capacity) { }

        public Polygon2D(IEnumerable<float2> vertices)
        {
            AddRange(vertices);
        }

        /// <summary>Gets the next index. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public int NextIndex(int index)
        {
            return index + 1 > Count - 1 ? 0 : index + 1;
        }

        /// <summary>Gets the next vertex. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public float2 NextVertex(int index)
        {
            return this[NextIndex(index)];
        }

        /// <summary>Gets the previous index. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public int PreviousIndex(int index)
        {
            return index - 1 < 0 ? Count - 1 : index - 1;
        }

        /// <summary>Gets the previous vertex. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public float2 PreviousVertex(int index)
        {
            return this[PreviousIndex(index)];
        }

        /// <summary>
        /// Indicates if the vertices are in counter clockwise order. Warning: If the area of the polygon is 0, it is
        /// unable to determine the winding.
        /// </summary>
        public bool IsCounterClockWise()
        {
            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (Count < 3)
                return false;

            return GetSignedArea() > 0.0f;
        }

        /// <summary>Forces the vertices to be counter clock wise order.</summary>
        public void ForceCounterClockWise()
        {
            // the simplest polygon which can exist in the euclidean plane has 3 sides.
            if (Count < 3)
                return;

            if (!IsCounterClockWise())
                Reverse();
        }

        /// <summary>Gets the signed area. If the area is less than 0, it indicates that the polygon is clockwise winded.</summary>
        /// <returns>The signed area</returns>
        public float GetSignedArea()
        {
            int count = Count;
            Debug.Assert(count >= 3, "Attempted to calculate signed area of a 2D polygon with less than 3 vertices.");

            int i;
            float area = 0;

            for (i = 0; i < count; i++)
            {
                int j = (i + 1) % count;

                float2 vi = this[i];
                float2 vj = this[j];

                area += vi.x * vj.y;
                area -= vi.y * vj.x;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>Winding number test for a point in a polygon.</summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>
        /// -1 if the winding number is zero and the point is outside the polygon, 1 if the point is inside the polygon,
        /// and 0 if the point is on the polygons edge.
        /// </returns>
        public int ContainsPoint(ref float2 point)
        {
            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < Count; i++)
            {
                // Get points
                var p1 = this[i];
                var p2 = this[NextIndex(i)];

                // Test if a point is directly on the edge
                var edge = p2 - p1;
                float area = MathEx.Area(ref p1, ref p2, ref point);
                if (area == 0f && math.dot(point - p1, edge) >= 0f && math.dot(point - p2, edge) <= 0f)
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
    }
}

#endif