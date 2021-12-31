#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of 3D polygons that make up a 3D mesh.</summary>
    public class PolygonMesh : List<Polygon>
    {
        /// <summary>Creates a new empty polygon mesh.</summary>
        public PolygonMesh()
        {
        }

        /// <summary>Creates a new polygon mesh containing the specified polygons.</summary>
        /// <param name="polygons">The initial polygons to add to the new polygon mesh.</param>
        public PolygonMesh(List<Polygon> polygons) : base(polygons)
        {
        }

        /// <summary>Creates a mesh out of the polygons.</summary>
        public Mesh ToMesh()
        {
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            int triangleOffset = 0;
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                var polygon = this[i];

                vertices.AddRange(polygon.GetVertices());
                uvs.AddRange(polygon.GetUV0());
                triangles.AddRange(polygon.GetTriangles(triangleOffset));

                triangleOffset = vertices.Count;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);

            return mesh;
        }

        /// <summary>
        /// Combines the given collection of polygon meshes into a single concave polygon mesh.
        /// </summary>
        /// <param name="polygonMeshes">The collection of polygon meshes to be combined.</param>
        /// <returns>The concave polygon mesh result.</returns>
        public static PolygonMesh Combine(List<PolygonMesh> polygonMeshes)
        {
            var result = new PolygonMesh();

            var polygonMeshesCount = polygonMeshes.Count;
            for (int i = 0; i < polygonMeshesCount; i++)
                result.AddRange(polygonMeshes[i]);

            return result;
        }
    }
}

#endif