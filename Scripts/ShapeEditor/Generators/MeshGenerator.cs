#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Generates meshes for a given collection decomposed convex <see cref="Polygon"/>.
    /// </summary>
    public static class MeshGenerator
    {
        /// <summary>Creates a flat mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        public static Mesh CreatePolygonMesh(List<Polygon> convexPolygons)
        {
            var polygonMesh = new PolygonMesh();

            var convexPolygonsCount = convexPolygons.Count;
            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // calculate 2D UV coordinates for the front polygon.
                convexPolygons[i].ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));
                // add the front polygon.
                polygonMesh.Add(convexPolygons[i]);
            }

            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        /// <summary>[Convex] Creates extruded meshes out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static List<PolygonMesh> CreateExtrudedPolygonMeshes(List<Polygon> convexPolygons, float distance)
        {
            var polygonMeshes = new List<PolygonMesh>();

            var convexPolygonsCount = convexPolygons.Count;
            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // calculate 2D UV coordinates for the front polygon.
                convexPolygons[i].ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                // create a new polygon mesh for the front polygon.
                var brush = new PolygonMesh();
                polygonMeshes.Add(brush);

                // add the front polygon to the mesh.
                brush.Add(convexPolygons[i]);

                // extrude the front polygon.
                foreach (var extudedPolygon in convexPolygons[i].Extrude(distance))
                {
                    extudedPolygon.ApplySabreCSGAutoUV0(Vector2.zero);
                    brush.Add(extudedPolygon);
                }

                // add a flipped back polygon to the mesh.
                var p = convexPolygons[i].flipped;
                p.Translate(new Vector3(0f, 0f, distance));
                // todo: parameter- can choose to mirror back by using SabreCSG AutoUV here.
                brush.Add(p);
            }

            // have a collection of convex extruded brushes.
            return polygonMeshes;
        }

        /// <summary>[Concave] Creates an extruded mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static Mesh CreateExtrudedPolygonMesh(List<Polygon> convexPolygons, float distance)
        {
            var polygonMeshes = CreateExtrudedPolygonMeshes(convexPolygons, distance);

            var polygonMesh = PolygonMesh.Combine(polygonMeshes);
            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        /// <summary>Creates a mesh by extrudes the convex polygons along a 3 point spline.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="spline">The spline to be followed.</param>
        /// <param name="precision">The spline precision.</param>
        public static Mesh CreateSplineExtrudeMesh(List<Polygon> convexPolygons, MathEx.Spline3 spline, int precision)
        {
            var polygonMesh = new PolygonMesh();
            /*
            foreach (var convexPolygon in convexPolygons)
            {
                // create the front polygon.
                var polygon3D = new Polygon(convexPolygon);

                // extrude it along the spline to build the sides.
                foreach (var extrudedPolygon in polygon3D.ExtrudeAlongSpline(spline, precision))
                    polygonMesh.Add(extrudedPolygon);
            }*/

            return polygonMesh.ToMesh();
        }

        /// <summary>Creates an extruded mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static Mesh CreateExtrudedPolygonAgainstPlaneMesh(List<Polygon> convexPolygons, Plane clippingPlane)
        {
            var polygonMesh = new PolygonMesh();

            /*
            foreach (var convexPolygon in convexPolygons)
            {
                // create the front polygon.
                var polygon3D = new Polygon(convexPolygon);
                polygonMesh.Add(polygon3D);

                // extrude it to build the sides.
                foreach (var extrudedPolygon in polygon3D.ExtrudeAgainstPlane(clippingPlane))
                    polygonMesh.Add(extrudedPolygon);

                // create the back polygon.
                var back = polygon3D.flipped;
                back.ProjectOnPlane(clippingPlane);
                polygonMesh.Add(back);
            }*/

            return polygonMesh.ToMesh();
        }
    }
}

#endif