#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Generates meshes for a given collection of <see cref="Vertices"/> (decomposed convex polygons).
    /// </summary>
    public static class MeshGenerator
    {
        /// <summary>Creates a flat mesh out of the convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        public static Mesh CreatePolygonMesh(List<Vertices> convexPolygons)
        {
            var mesh = new Mesh();
            mesh.name = "2DSE Polygon";

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var first = 0;

            foreach (var polygon in convexPolygons)
            {
                var vertexCount = polygon.Count;
                Debug.Log(vertexCount);

                for (int i = 0; i < vertexCount; i++)
                {
                    vertices.Add(new Vector3(polygon[i].x, -polygon[i].y));
                }

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

            return mesh;
        }
    }
}

#endif