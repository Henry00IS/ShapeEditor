#if UNITY_EDITOR

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
    }
}

#endif