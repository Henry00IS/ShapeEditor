#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Generates meshes for a given collection of <see cref="Polygon2D"/> (decomposed convex polygons).
    /// </summary>
    public static class MeshGenerator
    {
        /// <summary>Decomposes all of the shapes in the project into convex polygons.</summary>
        /// <param name="project">The project to decompose into convex polygons.</param>
        /// <returns>The list of convex polygon vertices.</returns>
        public static List<Polygon2D> GetProjectPolygons(Project project)
        {
            List<Polygon2D> convexPolygons = new List<Polygon2D>();

            // for every shape in the project:
            var shapesCount = project.shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = project.shapes[i];

                // generate the polygon.
                var concavePolygon = shape.GenerateConcavePolygon();

                // decompose the polygon.
                convexPolygons.AddRange(BayazitDecomposer.ConvexPartition(concavePolygon));
            }

            return convexPolygons;
        }

        /// <summary>Creates a flat mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        public static Mesh CreatePolygonMesh(List<Polygon2D> convexPolygons)
        {
            var polygonMesh = new PolygonMesh();

            foreach (var convexPolygon in convexPolygons)
                polygonMesh.Add(new Polygon3D(convexPolygon));

            return polygonMesh.ToMesh();
        }

        /// <summary>Creates an extruded mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static Mesh CreateExtrudedPolygonMesh(List<Polygon2D> convexPolygons, float distance)
        {
            var polygonMesh = new PolygonMesh();

            foreach (var convexPolygon in convexPolygons)
            {
                // create the front polygon.
                var polygon3D = new Polygon3D(convexPolygon);
                polygonMesh.Add(polygon3D);

                // extrude it to build the sides.
                foreach (var extrudedPolygon in polygon3D.Extrude(distance))
                    polygonMesh.Add(extrudedPolygon);

                // create the back polygon.
                var back = polygon3D.flipped;
                back.RecalculatePlane();
                back.Translate(back.plane.normal * distance);
                polygonMesh.Add(back);
            }

            return polygonMesh.ToMesh();
        }

        /// <summary>Creates a mesh by extrudes the convex polygons along a 3 point spline.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="spline">The spline to be followed.</param>
        /// <param name="precision">The spline precision.</param>
        public static Mesh CreateSplineExtrudeMesh(List<Polygon2D> convexPolygons, MathEx.Spline3 spline, int precision)
        {
            var polygonMesh = new PolygonMesh();

            foreach (var convexPolygon in convexPolygons)
            {
                // create the front polygon.
                var polygon3D = new Polygon3D(convexPolygon);

                // extrude it along the spline to build the sides.
                foreach (var extrudedPolygon in polygon3D.ExtrudeAlongSpline(spline, precision))
                    polygonMesh.Add(extrudedPolygon);

                /*
                for (int i = 0; i < precision; i++)
                {
                    var t = i / (float)precision;
                    var tnext = (i + 1) / (float)precision;

                    var poly = new Polygon3D(polygon3D);

                    var forward = (spline.GetForward(t) + spline.GetForward(tnext)).normalized;

                    poly.Rotate(Quaternion.LookRotation(forward, -spline.GetUp(t)));
                    poly.Translate(spline.GetPoint(t));

                    polygonMesh.Add(poly);
                }*/
            }

            return polygonMesh.ToMesh();
        }

        /// <summary>Creates an extruded mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static Mesh CreateExtrudedPolygonAgainstPlaneMesh(List<Polygon2D> convexPolygons, Plane clippingPlane)
        {
            var polygonMesh = new PolygonMesh();

            foreach (var convexPolygon in convexPolygons)
            {
                // create the front polygon.
                var polygon3D = new Polygon3D(convexPolygon);
                polygonMesh.Add(polygon3D);

                // extrude it to build the sides.
                foreach (var extrudedPolygon in polygon3D.ExtrudeAgainstPlane(clippingPlane))
                    polygonMesh.Add(extrudedPolygon);

                // create the back polygon.
                var back = polygon3D.flipped;
                back.ProjectOnPlane(clippingPlane);
                polygonMesh.Add(back);
            }

            return polygonMesh.ToMesh();
        }
    }
}

#endif