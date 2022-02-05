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
        /// <param name="doubleSided">Whether the polygon is double sided.</param>
        public static Mesh CreatePolygonMesh(List<Polygon> convexPolygons, bool doubleSided)
        {
            var polygonMesh = new PolygonMesh();

            var convexPolygonsCount = convexPolygons.Count;
            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // calculate 2D UV coordinates for the front polygon.
                convexPolygons[i].ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));
                // add the front polygon.
                polygonMesh.Add(convexPolygons[i]);

                if (doubleSided)
                {
                    // add the back polygon.
                    polygonMesh.Add(convexPolygons[i].flipped);
                }
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

                // copy the boolean operator of the 2D polygon into the polygon mesh.
                brush.booleanOperator = convexPolygons[i].booleanOperator;

                // add the front polygon to the mesh.
                brush.Add(convexPolygons[i]);

                // extrude the front polygon.
                foreach (var extrudedPolygon in convexPolygons[i].Extrude(distance))
                {
                    extrudedPolygon.ApplySabreCSGAutoUV0(new Vector2(0.5f, 0.5f));
                    brush.Add(extrudedPolygon);
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

        /// <summary>[Convex] Creates meshes by extruding the convex polygons along a 3 point spline.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="spline">The spline to be followed.</param>
        /// <param name="precision">The spline precision.</param>
        public static List<PolygonMesh> CreateSplineExtrudedPolygonMeshes(List<Polygon> convexPolygons, MathEx.Spline3 spline, int precision)
        {
            var polygonMeshes = new List<PolygonMesh>();

            var convexPolygonsCount = convexPolygons.Count;
            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // calculate 2D UV coordinates for the front polygon.
                // (RealtimeCSG doesn't need this) convexPolygons[i].ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                // extrude it along the spline building brushes.
                polygonMeshes.AddRange(convexPolygons[i].ExtrudeBrushesAlongSpline(spline, precision));
            }

            return polygonMeshes;
        }

        /// <summary>[Concave] Creates a mesh by extruding the convex polygons along a 3 point spline.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="spline">The spline to be followed.</param>
        /// <param name="precision">The spline precision.</param>
        public static Mesh CreateSplineExtrudedMesh(List<Polygon> convexPolygons, MathEx.Spline3 spline, int precision)
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

                // extrude it along the spline building brushes
                foreach (var extrudedPolygon in convexPolygons[i].ExtrudeAlongSpline(spline, precision))
                {
                    extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                    brush.Add(extrudedPolygon);
                }
            }

            var polygonMesh = PolygonMesh.Combine(polygonMeshes);
            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        /// <summary>[Convex] Creates a mesh by revolving extruded convex polygons along a circle.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="precision">The precision is the amount of brushes per step.</param>
        /// <param name="degrees">The revolve degrees between -360 to 360.</param>
        /// <param name="diameter">The inner diameter to revolve around.</param>
        /// <param name="height">The target height to be reached by offsetting the individual meshes.</param>
        public static List<PolygonMesh> CreateRevolveExtrudedPolygonMeshes(PolygonMesh convexPolygons, int precision, float degrees, float diameter, float height)
        {
            var polygonMeshes = new List<PolygonMesh>();

            // offset the polygons by the the vertical project center line as this will let us
            // rotate left or right without self-intersecting or inverting the mesh.
            var projectCenterOffset = degrees > 0f ? new Vector3(-convexPolygons.bounds2D.max.x, 0f) : new Vector3(-convexPolygons.bounds2D.min.x, 0f);

            var convexPolygonsCount = convexPolygons.Count;
            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // calculate 2D UV coordinates for the front polygon.
                // (RealtimeCSG doesn't need this) convexPolygons[i].ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                for (int j = 0; j < precision; j++)
                {
                    // create a new polygon mesh for the front polygon.
                    var brush = new PolygonMesh();
                    polygonMeshes.Add(brush);

                    // copy the boolean operator of the 2D polygon into the polygon mesh.
                    brush.booleanOperator = convexPolygons[i].booleanOperator;

                    var poly = new Polygon(convexPolygons[i]);

                    // ensure that the polygon is always on one side of the vertical project center line.
                    poly.Translate(projectCenterOffset);

                    var nextPoly = new Polygon(poly);
                    var polyVertexCount = poly.Count;

                    // flip the pivot on negative degrees to rotate left.
                    Vector3 pivot = new Vector3(degrees > 0f ? diameter : -diameter, 0.0f, 0.0f);

                    for (int v = 0; v < polyVertexCount; v++)
                    {
                        // calculate the step height.
                        var heightOffset = new Vector3();
                        if (precision >= 2)
                            heightOffset.y = (j / ((float)precision - 1)) * height;

                        poly[v] = new Vertex(heightOffset + MathEx.RotatePointAroundPivot(poly[v].position, pivot, new Vector3(0.0f, Mathf.Lerp(0f, degrees, j / (float)precision), 0.0f)), poly[v].uv0);
                        nextPoly[v] = new Vertex(heightOffset + MathEx.RotatePointAroundPivot(nextPoly[v].position, pivot, new Vector3(0.0f, Mathf.Lerp(0f, degrees, (j + 1) / (float)precision), 0.0f)), nextPoly[v].uv0);
                    }

                    brush.Add(poly);
                    brush.Add(nextPoly.flipped);

                    // fill the gap with quads "extruding" the shape.
                    Polygon extrudedPolygon;
                    for (int k = 0; k < polyVertexCount - 1; k++)
                    {
                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[k],
                            nextPoly[k],
                            nextPoly[k + 1],
                            poly[k + 1],
                        });

                        // (RealtimeCSG doesn't need this) extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                        brush.Add(extrudedPolygon);
                    }

                    // one more face that wraps around to index 0.
                    extrudedPolygon = new Polygon(new Vertex[] {
                        poly[polyVertexCount - 1],
                        nextPoly[polyVertexCount - 1],
                        nextPoly[0],
                        poly[0],
                    });

                    // (RealtimeCSG doesn't need this) extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                    brush.Add(extrudedPolygon);

                    // undo the polygon translation ensuring they were always on one side of the center line.
                    brush.Translate(-projectCenterOffset);
                }
            }

            return polygonMeshes;
        }

        /// <summary>[Concave] Creates a mesh by revolving extruded convex polygons along a circle.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="precision">The precision is the amount of brushes per step.</param>
        /// <param name="degrees">The revolve degrees between -360 to 360.</param>
        /// <param name="diameter">The inner diameter to revolve around.</param>
        /// <param name="height">The target height to be reached by offsetting the individual meshes.</param>
        /// <param name="sloped">Whether the individual meshes are sloped towards the target height.</param>
        public static Mesh CreateRevolveExtrudedMesh(PolygonMesh convexPolygons, int precision, float degrees, float diameter, float height, bool sloped)
        {
            var polygonMeshes = new List<PolygonMesh>();

            var slopedHeightOffset = new Vector3();
            if (sloped && precision >= 2)
                slopedHeightOffset = new Vector3(0.0f, height / precision, 0.0f);

            // the ending height has to be reduced so that it aligns perfectly with the non-sloped version.
            height -= slopedHeightOffset.y;

            // offset the polygons by the the vertical project center line as this will let us
            // rotate left or right without self-intersecting or inverting the mesh.
            var projectCenterOffset = degrees > 0f ? new Vector3(-convexPolygons.bounds2D.max.x, 0f) : new Vector3(-convexPolygons.bounds2D.min.x, 0f);

            var convexPolygonsCount = convexPolygons.Count;
            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // calculate 2D UV coordinates for the front polygon.
                convexPolygons[i].ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                for (int j = 0; j < precision; j++)
                {
                    // create a new polygon mesh for the front polygon.
                    var brush = new PolygonMesh();
                    polygonMeshes.Add(brush);

                    var poly = new Polygon(convexPolygons[i]);

                    // ensure that the polygon is always on one side of the vertical project center line.
                    poly.Translate(projectCenterOffset);

                    var nextPoly = new Polygon(poly);
                    var polyVertexCount = poly.Count;

                    // flip the pivot on negative degrees to rotate left.
                    Vector3 pivot = new Vector3(degrees > 0f ? diameter : -diameter, 0.0f, 0.0f);

                    for (int v = 0; v < polyVertexCount; v++)
                    {
                        // calculate the step height.
                        var heightOffset = new Vector3();
                        if (precision >= 2)
                            heightOffset.y = (j / ((float)precision - 1)) * height;

                        poly[v] = new Vertex(heightOffset + MathEx.RotatePointAroundPivot(poly[v].position, pivot, new Vector3(0.0f, Mathf.Lerp(0f, degrees, j / (float)precision), 0.0f)), poly[v].uv0, poly[v].hidden);
                        nextPoly[v] = new Vertex(heightOffset + slopedHeightOffset + MathEx.RotatePointAroundPivot(nextPoly[v].position, pivot, new Vector3(0.0f, Mathf.Lerp(0f, degrees, (j + 1) / (float)precision), 0.0f)), nextPoly[v].uv0, nextPoly[v].hidden);
                    }

                    if (height == 0f || sloped)
                    {
                        if (j == 0) brush.Add(poly);
                        if (j == precision - 1) brush.Add(nextPoly.flipped);
                    }
                    else
                    {
                        brush.Add(poly);
                        brush.Add(nextPoly.flipped);
                    }

                    // fill the gap with quads "extruding" the shape.
                    Polygon extrudedPolygon;
                    for (int k = 0; k < polyVertexCount - 1; k++)
                    {
                        if (poly[k].hidden) continue;

                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[k],
                            nextPoly[k],
                            nextPoly[k + 1],
                            poly[k + 1],
                        });

                        extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                        brush.Add(extrudedPolygon);
                    }

                    // one more face that wraps around to index 0.
                    if (!poly[polyVertexCount - 1].hidden)
                    {
                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[polyVertexCount - 1],
                            nextPoly[polyVertexCount - 1],
                            nextPoly[0],
                            poly[0],
                        });

                        extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                        brush.Add(extrudedPolygon);
                    }
                }
            }

            var polygonMesh = PolygonMesh.Combine(polygonMeshes);

            // undo the polygon translation ensuring they were always on one side of the center line.
            polygonMesh.Translate(-projectCenterOffset);

            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }
    }
}

#endif