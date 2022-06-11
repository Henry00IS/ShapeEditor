#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of 2D/3D polygon meshes.</summary>
    public class PolygonMeshes : List<PolygonMesh>
    {
        /// <summary>Creates a new empty collection of polygon meshes.</summary>
        public PolygonMeshes()
        {
        }

        /// <summary>Creates a new empty collection of polygon meshes with the specified initial capacity.</summary>
        /// <param name="capacity">The number of polygon meshes the collection can hold without resizing.</param>
        public PolygonMeshes(int capacity) : base(capacity)
        {
        }

        /// <summary>Creates a new polygon mesh collection containing the specified polygon meshes.</summary>
        /// <param name="polygons">The initial polygon meshes to add to the new collection.</param>
        public PolygonMeshes(List<PolygonMesh> polygonMeshes) : base(polygonMeshes)
        {
        }

        /// <summary>The cached last calculated <see cref="CalculateBounds2D"/> bounds.</summary>
        public Bounds bounds2D;

        /// <summary>[2D] Calculates and returns an AABB that fully contains all polygon meshes.</summary>
        public Bounds CalculateBounds2D()
        {
            var count = Count;
            bounds2D = default;

            if (count == 0)
                return bounds2D;

            bool first = true;
            for (int i = 0; i < count; i++)
            {
                // chopped polygon meshes may be empty.
                if (this[i].Count > 0)
                {
                    if (first)
                    {
                        first = false;
                        bounds2D = this[i].CalculateBounds2D();
                        continue;
                    }
                    bounds2D.Encapsulate(this[i].CalculateBounds2D());
                }
            }

            return bounds2D;
        }
    }
}

#endif