#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of 3D polygons that make up a 3D mesh.</summary>
    public class PolygonMesh : List<Polygon3D>
    {
        /// <summary>Creates a mesh out of the polygons.</summary>
        public Mesh ToMesh()
        {
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            var first = 0;

            foreach (var polygon in this)
            {
                vertices.AddRange(polygon);
                uvs.AddRange(polygon.GenerateUV_SabreCSG());

                var vertexCount = polygon.Count;
                int next = first + 1;
                for (int i = 2; i < vertexCount; i++)
                {
                    triangles.Add(first);
                    triangles.Add(next);
                    triangles.Add(first + i);
                    next = first + i;
                }
                first += vertexCount;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }
    }
}

#endif