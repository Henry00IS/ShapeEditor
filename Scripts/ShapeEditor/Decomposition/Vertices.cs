#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class Vertices : List<float2>
    {
        public Vertices()
        { }

        public Vertices(int capacity) : base(capacity) { }

        public Vertices(IEnumerable<float2> vertices)
        {
            AddRange(vertices);
        }

        /// <summary>
        /// Indicates if the vertices are in counter clockwise order. Warning: If the area of the polygon is 0, it is
        /// unable to determine the winding.
        /// </summary>
        public bool IsCounterClockWise()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return false;

            return GetSignedArea() > 0.0f;
        }

        /// <summary>Gets the signed area. If the area is less than 0, it indicates that the polygon is clockwise winded.</summary>
        /// <returns>The signed area</returns>
        public float GetSignedArea()
        {
            //The simplest polygon which can exist in the Euclidean plane has 3 sides.
            if (Count < 3)
                return 0;

            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;

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