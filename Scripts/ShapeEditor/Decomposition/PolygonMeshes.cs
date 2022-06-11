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
            if (count == 0)
            {
                bounds2D = default;
                return bounds2D;
            }

            bounds2D = this[0].CalculateBounds2D();
            for (int i = 1; i < count; i++)
                bounds2D.Encapsulate(this[i].CalculateBounds2D());

            return bounds2D;
        }
    }
}

#endif