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
            mesh.SetUVs(0, GenerateUV0_SabreCSG(vertices));

            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        private static Vector2[] GenerateUV0_SabreCSG(List<Vector3> vertices)
        {
            var results = new Vector2[vertices.Count];

            var plane = new Plane(vertices[0], vertices[1], vertices[2]);

            Vector3 planeNormal = plane.normal;
            Quaternion cancellingRotation = Quaternion.Inverse(Quaternion.LookRotation(-planeNormal));
            // Sets the UV at each point to the position on the plane
            for (int i = 0; i < results.Length; i++)
            {
                Vector3 position = vertices[i];
                Vector2 uv = new Vector3(0.5f, 0.5f, 0f) + (cancellingRotation * position);
                results[i] = uv;
            }

            return results;
        }
    }
}

#endif