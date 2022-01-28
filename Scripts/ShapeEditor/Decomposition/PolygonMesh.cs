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

        /// <summary>The boolean operator of the polygon mesh used by CSG targets.</summary>
        [SerializeField]
        public PolygonBooleanOperator booleanOperator = PolygonBooleanOperator.Union;

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

            // if the vertex count exceeds 16-bit we switch to 32-bit.
            if (triangleOffset > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);

            return mesh;
        }

        /// <summary>
        /// Creates a set of planes out of the polygons. A convex mesh can consist out of several
        /// faces, each of which can be defined by a plane (of which there must be at least four).
        /// Each plane defines a half space, that is, an infinite set of points that is bounded by a
        /// plane. The intersection of these half spaces forms a convex polyhedron.
        /// </summary>
        /// <returns>The planes that represent the convex mesh.</returns>
        public Plane[] ToPlanes()
        {
            var count = Count;
            var planes = new Plane[count];
            for (int i = 0; i < count; i++)
            {
                var polygon = this[i];
                polygon.RecalculatePlane();
                planes[i] = polygon.plane.flipped;
            }
            return planes;
        }

        /// <summary>Converts the polygons to a list of points (point cloud).</summary>
        /// <returns>The vertex positions of the convex mesh.</returns>
        public List<Vector3> ToPoints()
        {
            var count = Count;
            var points = new List<Vector3>();
            for (int i = 0; i < count; i++)
            {
                var polygon = this[i];
                points.AddRange(polygon.GetVertices());
            }
            return points;
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