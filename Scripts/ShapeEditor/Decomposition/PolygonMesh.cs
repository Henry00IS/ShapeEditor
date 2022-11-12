﻿#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of 2D/3D polygons that make up a 2D/3D mesh.</summary>
    public class PolygonMesh : List<Polygon>
    {
        /// <summary>Creates a new empty polygon mesh.</summary>
        public PolygonMesh()
        {
        }

        /// <summary>Creates a new empty polygon mesh with the specified initial capacity.</summary>
        /// <param name="capacity">The number of polygons the polygon mesh can hold without resizing.</param>
        public PolygonMesh(int capacity) : base(capacity)
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

        /// <summary>The cached last calculated <see cref="CalculateBounds2D"/> bounds.</summary>
        public Bounds bounds2D;

        private class Submesh // todo: we can optimize this class away by checking List<int>.Count to determine whether it's used.
        {
            public bool used = false;
            public List<int> triangles = new List<int>();
        }

        /// <summary>[2D/3D] Creates a mesh out of the polygons.</summary>
        public Mesh ToMesh()
        {
            var mesh = new Mesh();
            mesh.name = "Generated Mesh";

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var submeshes = new Submesh[8];
            for (int i = 0; i < submeshes.Length; i++)
                submeshes[i] = new Submesh();

            int triangleOffset = 0;
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                var polygon = this[i];
                var submesh = submeshes[polygon.material];

                vertices.AddRange(polygon.GetVertices());
                uvs.AddRange(polygon.GetUV0());
                submesh.used = true;
                submesh.triangles.AddRange(polygon.GetTriangles(triangleOffset));

                triangleOffset = vertices.Count;
            }

            // if the vertex count exceeds 16-bit we switch to 32-bit.
            if (triangleOffset > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);

            // assign submeshes for used material slots.
            int j = 0;
            for (int i = 0; i < submeshes.Length; i++)
            {
                var submesh = submeshes[i];
                if (submesh.used)
                {
                    mesh.subMeshCount = j + 1;
                    mesh.SetTriangles(submesh.triangles, j);
                    j++;
                }
            }

            return mesh;
        }

        /// <summary>
        /// [3D] Creates a set of planes out of the polygons. A convex mesh can consist out of
        /// several faces, each of which can be defined by a plane (of which there must be at least
        /// four). Each plane defines a half space, that is, an infinite set of points that is
        /// bounded by a plane. The intersection of these half spaces forms a convex polyhedron.
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

        /// <summary>
        /// [3D] Creates a set of planes out of the polygons. A convex mesh can consist out of
        /// several faces, each of which can be defined by a plane (of which there must be at least
        /// four). Each plane defines a half space, that is, an infinite set of points that is
        /// bounded by a plane. The intersection of these half spaces forms a convex polyhedron.
        /// </summary>
        /// <returns>The planes that represent the convex mesh with their material indices.</returns>
        public (Plane[] planes, int[] materials) ToMaterialPlanes()
        {
            var count = Count;
            var planes = new Plane[count];
            var materials = new int[count];
            for (int i = 0; i < count; i++)
            {
                var polygon = this[i];
                polygon.RecalculatePlane();
                planes[i] = polygon.plane.flipped;
                materials[i] = polygon.material;
            }
            return (planes, materials);
        }

        /// <summary>[2D/3D] Converts the polygons to a list of points (point cloud).</summary>
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

        /// <summary>[2D] Calculates and returns an AABB that fully contains all polygons.</summary>
        public Bounds CalculateBounds2D()
        {
            var count = Count;
            if (count == 0)
            {
                bounds2D = default;
                return bounds2D;
            }

            bounds2D = this[0].GetAABB();
            for (int i = 1; i < count; i++)
                bounds2D.Encapsulate(this[i].GetAABB());

            return bounds2D;
        }

        /// <summary>[2D/3D] Translates the polygons with the specified vector.</summary>
        /// <param name="value">The vector.</param>
        public void Translate(Vector3 value)
        {
            Translate(ref value);
        }

        /// <summary>[2D/3D] Translates the polygons with the specified vector.</summary>
        /// <param name="value">The vector.</param>
        public void Translate(ref Vector3 value)
        {
            int count = Count;

            for (int i = 0; i < count; i++)
                this[i].Translate(value);
        }

        /// <summary>
        /// [2D/3D] Combines the given collection of polygon meshes into a single concave polygon mesh.
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