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
            var convexPolygonsCount = convexPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>();

            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // create a new polygon mesh for the front polygon.
                var brush = new PolygonMesh();
                polygonMeshes.Add(brush);

                // copy the boolean operator of the 2D polygon into the polygon mesh.
                brush.booleanOperator = convexPolygons[i].booleanOperator;

                var poly = new Polygon(convexPolygons[i]);

                var nextPoly = new Polygon(convexPolygons[i]);
                nextPoly.Translate(new Vector3(0.0f, 0.0f, distance));

                brush.Add(poly);
                brush.Add(nextPoly.flipped);

                // fill the gap with quads "extruding" the shape.
                Polygon extrudedPolygon;
                var polyVertexCount = poly.Count;
                for (int k = 0; k < polyVertexCount - 1; k++)
                {
                    extrudedPolygon = new Polygon(new Vertex[] {
                        poly[k],
                        nextPoly[k],
                        nextPoly[k + 1],
                        poly[k + 1],
                    });

                    brush.Add(extrudedPolygon);
                }

                // one more face that wraps around to index 0.
                extrudedPolygon = new Polygon(new Vertex[] {
                    poly[polyVertexCount - 1],
                    nextPoly[polyVertexCount - 1],
                    nextPoly[0],
                    poly[0],
                });

                brush.Add(extrudedPolygon);
            }

            return polygonMeshes;
        }

        /// <summary>[Concave] Creates an extruded mesh out of convex polygons.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static Mesh CreateExtrudedPolygonMesh(List<Polygon> convexPolygons, float distance)
        {
            var convexPolygonsCount = convexPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>();

            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // create a new polygon mesh for the front polygon.
                var brush = new PolygonMesh();
                polygonMeshes.Add(brush);

                var poly = new Polygon(convexPolygons[i]);

                // calculate 2D UV coordinates for the front polygon.
                poly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                var nextPoly = new Polygon(convexPolygons[i]);
                nextPoly.Translate(new Vector3(0.0f, 0.0f, distance));

                // calculate 2D UV coordinates for the back polygon.
                nextPoly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                brush.Add(poly);
                brush.Add(nextPoly.flipped);

                // fill the gap with quads "extruding" the shape.
                Polygon extrudedPolygon;
                var polyVertexCount = poly.Count;
                for (int k = 0; k < polyVertexCount - 1; k++)
                {
                    if (poly[k].hidden) continue;

                    extrudedPolygon = new Polygon(new Vertex[] {
                        poly[k],
                        nextPoly[k],
                        nextPoly[k + 1],
                        poly[k + 1],
                    });

                    extrudedPolygon.ApplySabreCSGAutoUV0(new Vector2(0.5f, 0.5f));
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

                    extrudedPolygon.ApplySabreCSGAutoUV0(new Vector2(0.5f, 0.5f));
                    brush.Add(extrudedPolygon);
                }
            }

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
        public static List<PolygonMesh> CreateRevolveExtrudedPolygonMeshes(PolygonMesh convexPolygons, int precision, float degrees, float diameter, float height, bool sloped)
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
                        nextPoly[v] = new Vertex(heightOffset + slopedHeightOffset + MathEx.RotatePointAroundPivot(nextPoly[v].position, pivot, new Vector3(0.0f, Mathf.Lerp(0f, degrees, (j + 1) / (float)precision), 0.0f)), nextPoly[v].uv0);
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

                        // split non-planar polygons for slopes to prevent cracking quads.
                        if (height != 0f && sloped && extrudedPolygon.SplitNonPlanar4(out var planarPolygons))
                            brush.AddRange(planarPolygons);
                        else
                            brush.Add(extrudedPolygon);
                    }

                    // one more face that wraps around to index 0.
                    {
                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[polyVertexCount - 1],
                            nextPoly[polyVertexCount - 1],
                            nextPoly[0],
                            poly[0],
                        });

                        // split non-planar polygons for slopes to prevent cracking quads.
                        if (height != 0f && sloped && extrudedPolygon.SplitNonPlanar4(out var planarPolygons))
                            brush.AddRange(planarPolygons);
                        else
                            brush.Add(extrudedPolygon);
                    }

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

        /// <summary>[Convex] Creates a mesh by placing extruded convex polygons along a linear slope.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="precision">The precision is the amount of brushes per step.</param>
        /// <param name="distance">The total length or distance of the staircase in meters.</param>
        /// <param name="height">The target height to be reached by offsetting the individual meshes.</param>
        /// <param name="sloped">Whether the individual meshes are sloped towards the target height.</param>
        public static List<PolygonMesh> CreateLinearStaircaseMeshes(PolygonMesh convexPolygons, int precision, float distance, float height, bool sloped)
        {
            var convexPolygonsCount = convexPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>();

            // sloped stairs (a ramp) can only exist out of one brush.
            var slopedHeightOffset = new Vector3();
            if (sloped)
            {
                precision = 1;
                slopedHeightOffset = new Vector3(0.0f, height, 0.0f);
            }

            // the ending height has to be reduced so that it aligns perfectly with the non-sloped version.
            height -= slopedHeightOffset.y;

            for (int j = 0; j < precision; j++)
            {
                // calculate the step forward distance.
                var forward = new Vector3(0.0f, 0.0f, (j / (float)precision) * distance);
                var forwardNext = new Vector3(0.0f, 0.0f, ((j + 1) / (float)precision) * distance);

                // calculate the step height.
                var heightOffset = new Vector3();
                if (precision >= 2)
                    heightOffset.y = (j / ((float)precision - 1)) * height;

                for (int i = 0; i < convexPolygonsCount; i++)
                {
                    // create a new polygon mesh for the front polygon.
                    var brush = new PolygonMesh();
                    polygonMeshes.Add(brush);

                    // copy the boolean operator of the 2D polygon into the polygon mesh.
                    brush.booleanOperator = convexPolygons[i].booleanOperator;

                    var poly = new Polygon(convexPolygons[i]);
                    var polyVertexCount = poly.Count;
                    poly.Translate(forward + heightOffset);

                    var nextPoly = new Polygon(convexPolygons[i]);
                    nextPoly.Translate(forwardNext + heightOffset + slopedHeightOffset);

                    brush.Add(poly);
                    brush.Add(nextPoly.flipped);

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

                        brush.Add(extrudedPolygon);
                    }
                }
            }

            return polygonMeshes;
        }

        /// <summary>[Concave] Creates a mesh by placing extruded convex polygons along a linear slope.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="precision">The precision is the amount of brushes per step.</param>
        /// <param name="distance">The total length or distance of the staircase in meters.</param>
        /// <param name="height">The target height to be reached by offsetting the individual meshes.</param>
        /// <param name="sloped">Whether the individual meshes are sloped towards the target height.</param>
        public static Mesh CreateLinearStaircaseMesh(PolygonMesh convexPolygons, int precision, float distance, float height, bool sloped)
        {
            var convexPolygonsCount = convexPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>();

            // sloped stairs (a ramp) can only exist out of one brush.
            var slopedHeightOffset = new Vector3();
            if (sloped)
            {
                precision = 1;
                slopedHeightOffset = new Vector3(0.0f, height, 0.0f);
            }

            // the ending height has to be reduced so that it aligns perfectly with the non-sloped version.
            height -= slopedHeightOffset.y;

            for (int j = 0; j < precision; j++)
            {
                // calculate the step forward distance.
                var forward = new Vector3(0.0f, 0.0f, (j / (float)precision) * distance);
                var forwardNext = new Vector3(0.0f, 0.0f, ((j + 1) / (float)precision) * distance);

                // calculate the step height.
                var heightOffset = new Vector3();
                if (precision >= 2)
                    heightOffset.y = (j / ((float)precision - 1)) * height;

                for (int i = 0; i < convexPolygonsCount; i++)
                {
                    // create a new polygon mesh for the front polygon.
                    var brush = new PolygonMesh();
                    polygonMeshes.Add(brush);

                    var poly = new Polygon(convexPolygons[i]);
                    var polyVertexCount = poly.Count;
                    poly.Translate(forward + heightOffset);
                    poly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                    var nextPoly = new Polygon(convexPolygons[i]);
                    nextPoly.Translate(forwardNext + heightOffset + slopedHeightOffset);
                    nextPoly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

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

                        if (sloped)
                            extrudedPolygon.ApplySabreCSGAutoUV0(new Vector2(0.5f, 0.5f));
                        else
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

                        if (sloped)
                            extrudedPolygon.ApplySabreCSGAutoUV0(new Vector2(0.5f, 0.5f));
                        else
                            extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                        brush.Add(extrudedPolygon);
                    }
                }
            }

            var polygonMesh = PolygonMesh.Combine(polygonMeshes);
            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>[Convex] Creates a mesh by scaling extruded convex polygons to a point or clipped.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        /// <param name="beginScale">The scale of the front polygon.</param>
        /// <param name="endScale">The scale of the back polygon.</param>
        /// <param name="offset">The offset from the center to scale towards.</param>
        public static List<PolygonMesh> CreateScaleExtrudedMeshes(PolygonMesh convexPolygons, float distance, float beginScale, float endScale, Vector2 offset)
        {
            if (beginScale == 0f && endScale == 0f)
                return new List<PolygonMesh>();

            var convexPolygonsCount = convexPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>();

            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // create a new polygon mesh for the front polygon.
                var brush = new PolygonMesh();
                polygonMeshes.Add(brush);

                // copy the boolean operator of the 2D polygon into the polygon mesh.
                brush.booleanOperator = convexPolygons[i].booleanOperator;

                var poly = new Polygon(convexPolygons[i]);
                poly.Scale(new Vector3(beginScale, beginScale, 1.0f));

                var nextPoly = new Polygon(convexPolygons[i]);
                nextPoly.Scale(new Vector3(endScale, endScale, 1.0f));
                nextPoly.Translate(new Vector3(offset.x, offset.y) + new Vector3(0.0f, 0.0f, distance));

                if (beginScale != 0.0f) brush.Add(poly);
                if (endScale != 0.0f) brush.Add(nextPoly.flipped);

                // fill the gap with quads "extruding" the shape.
                Polygon extrudedPolygon;
                var polyVertexCount = poly.Count;
                for (int k = 0; k < polyVertexCount - 1; k++)
                {
                    extrudedPolygon = new Polygon(new Vertex[] {
                        poly[k],
                        nextPoly[k],
                        nextPoly[k + 1],
                        poly[k + 1],
                    });

                    brush.Add(extrudedPolygon);
                }

                // one more face that wraps around to index 0.
                extrudedPolygon = new Polygon(new Vertex[] {
                    poly[polyVertexCount - 1],
                    nextPoly[polyVertexCount - 1],
                    nextPoly[0],
                    poly[0],
                });

                brush.Add(extrudedPolygon);
            }

            return polygonMeshes;
        }

        /// <summary>[Concave] Creates a mesh by scaling extruded convex polygons to a point or clipped.</summary>
        /// <param name="convexPolygons">The decomposed convex polygons.</param>
        /// <param name="distance">The distance to extrude by.</param>
        /// <param name="beginScale">The scale of the front polygon.</param>
        /// <param name="endScale">The scale of the back polygon.</param>
        /// <param name="offset">The offset from the center to scale towards.</param>
        public static Mesh CreateScaleExtrudedMesh(PolygonMesh convexPolygons, float distance, float beginScale, float endScale, Vector2 offset)
        {
            if (beginScale == 0f && endScale == 0f)
                return new Mesh();

            var convexPolygonsCount = convexPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>();

            for (int i = 0; i < convexPolygonsCount; i++)
            {
                // create a new polygon mesh for the front polygon.
                var brush = new PolygonMesh();
                polygonMeshes.Add(brush);

                var poly = new Polygon(convexPolygons[i]);
                poly.Scale(new Vector3(beginScale, beginScale, 1.0f));

                // calculate 2D UV coordinates for the front polygon.
                poly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                var nextPoly = new Polygon(convexPolygons[i]);
                nextPoly.Scale(new Vector3(endScale, endScale, 1.0f));
                nextPoly.Translate(new Vector3(offset.x, offset.y) + new Vector3(0.0f, 0.0f, distance));

                // calculate 2D UV coordinates for the back polygon.
                nextPoly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                if (beginScale != 0.0f) brush.Add(poly);
                if (endScale != 0.0f) brush.Add(nextPoly.flipped);

                // fill the gap with quads "extruding" the shape.
                Polygon extrudedPolygon;
                var polyVertexCount = poly.Count;
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

            var polygonMesh = PolygonMesh.Combine(polygonMeshes);
            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>[Convex] Creates a mesh by revolving chopped convex polygons along a circle.</summary>
        /// <param name="convexPolygons">The chopped and decomposed convex polygons.</param>
        /// <param name="degrees">The revolve degrees between -360 to 360.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static List<PolygonMesh> CreateRevolveChoppedMeshes(PolygonMeshes choppedPolygons, float degrees, float distance)
        {
            int precision = choppedPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>(precision);

            Bounds projectBounds = choppedPolygons.bounds2D;

            // offset the polygons by the the vertical project center line as this will let us
            // rotate left or right without self-intersecting or inverting the mesh.
            var projectCenterOffset = new Vector3(projectBounds.min.x, 0f, 0f);

            var innerCircle = MathEx.Circle.GetCircleThatFitsCircumference(projectBounds.size.x * Mathf.Sign(degrees), Mathf.Abs(degrees) / 360f);
            var outerCircle = new MathEx.Circle(innerCircle.radius + distance * Mathf.Sign(degrees));

            // iterate over all chops in the project:
            for (int i = 0; i < precision; i++)
            {
                var convexPolygons = choppedPolygons[i];
                var convexPolygonsCount = convexPolygons.Count;

                var stepLength = projectBounds.size.x / precision;

                float s1 = i * stepLength;
                float s2 = (i + 1) * stepLength;
                float t1 = s1 / innerCircle.circumference;
                float t2 = s2 / innerCircle.circumference;

                var innerCirclepos1 = innerCircle.GetCirclePosition(t1) + projectCenterOffset;
                var innerCirclepos2 = innerCircle.GetCirclePosition(t2) + projectCenterOffset;
                var outerCirclepos1 = outerCircle.GetCirclePosition(t1) + projectCenterOffset;
                var outerCirclepos2 = outerCircle.GetCirclePosition(t2) + projectCenterOffset;

                // iterate over all decomposed convex polygons of the chop:
                for (int j = 0; j < convexPolygonsCount; j++)
                {
                    var poly = new Polygon(convexPolygons[j]);

                    // create a new polygon mesh for the front polygon.
                    var brush = new PolygonMesh();
                    polygonMeshes.Add(brush);

                    // copy the boolean operator of the 2D polygon into the polygon mesh.
                    brush.booleanOperator = convexPolygons[j].booleanOperator;

                    // ensure that the polygon is always on one side of the vertical project center line.
                    poly.Translate(-projectCenterOffset);

                    var backPoly = new Polygon(convexPolygons[j]);
                    var polyCount = poly.Count;

                    // iterate over all cloned vertices:
                    for (int v = 0; v < polyCount; v++)
                    {
                        var vertex = poly[v];

                        var innerVertexPos = Vector3.Lerp(
                            new Vector3(innerCirclepos1.x, vertex.position.y, innerCirclepos1.z),
                            new Vector3(innerCirclepos2.x, vertex.position.y, innerCirclepos2.z),
                            Mathf.InverseLerp(s1, s2, vertex.position.x)
                        );

                        var outerVertexPos = Vector3.Lerp(
                            new Vector3(outerCirclepos1.x, vertex.position.y, outerCirclepos1.z),
                            new Vector3(outerCirclepos2.x, vertex.position.y, outerCirclepos2.z),
                            Mathf.InverseLerp(s1, s2, vertex.position.x)
                        );

                        innerVertexPos.z -= innerCircle.radius;
                        outerVertexPos.z -= innerCircle.radius;

                        if (degrees < 0f)
                        {
                            innerVertexPos.z += distance;
                            outerVertexPos.z += distance;
                        }

                        poly[v] = new Vertex(innerVertexPos, poly[v].uv0, poly[v].hidden);
                        backPoly[v] = new Vertex(outerVertexPos, poly[v].uv0, poly[v].hidden);
                    }

                    if (degrees < 0f)
                    {
                        brush.Add(poly.flipped);
                        brush.Add(backPoly);
                    }
                    else
                    {
                        brush.Add(poly);
                        brush.Add(backPoly.flipped);
                    }

                    // fill the gap with quads "extruding" the shape.
                    Polygon extrudedPolygon;
                    for (int k = 0; k < polyCount - 1; k++)
                    {
                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[k],
                            backPoly[k],
                            backPoly[k + 1],
                            poly[k + 1],
                        });

                        if (degrees < 0)
                            extrudedPolygon = extrudedPolygon.flipped;

                        if (extrudedPolygon.SplitNonPlanar4(out var planarPolygons))
                            brush.AddRange(planarPolygons);
                        else
                            brush.Add(extrudedPolygon);
                    }

                    // one more face that wraps around to index 0.
                    {
                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[polyCount - 1],
                            backPoly[polyCount - 1],
                            backPoly[0],
                            poly[0],
                        });

                        if (degrees < 0)
                            extrudedPolygon = extrudedPolygon.flipped;

                        if (extrudedPolygon.SplitNonPlanar4(out var planarPolygons))
                            brush.AddRange(planarPolygons);
                        else
                            brush.Add(extrudedPolygon);
                    }
                }
            }

            return polygonMeshes;
        }

        /// <summary>[Concave] Creates a mesh by revolving chopped convex polygons along a circle.</summary>
        /// <param name="convexPolygons">The chopped and decomposed convex polygons.</param>
        /// <param name="degrees">The revolve degrees between -360 to 360.</param>
        /// <param name="distance">The distance to extrude by.</param>
        public static Mesh CreateRevolveChoppedMesh(PolygonMeshes choppedPolygons, float degrees, float distance)
        {
            int precision = choppedPolygons.Count;
            var polygonMeshes = new List<PolygonMesh>(precision);

            Bounds projectBounds = choppedPolygons.bounds2D;

            // offset the polygons by the the vertical project center line as this will let us
            // rotate left or right without self-intersecting or inverting the mesh.
            var projectCenterOffset = new Vector3(projectBounds.min.x, 0f, 0f);

            var innerCircle = MathEx.Circle.GetCircleThatFitsCircumference(projectBounds.size.x * Mathf.Sign(degrees), Mathf.Abs(degrees) / 360f);
            var outerCircle = new MathEx.Circle(innerCircle.radius + distance * Mathf.Sign(degrees));

            // iterate over all chops in the project:
            for (int i = 0; i < precision; i++)
            {
                var convexPolygons = choppedPolygons[i];
                var convexPolygonsCount = convexPolygons.Count;
                var brush = new PolygonMesh(convexPolygonsCount);

                var stepLength = projectBounds.size.x / precision;

                float s1 = i * stepLength;
                float s2 = (i + 1) * stepLength;
                float t1 = s1 / innerCircle.circumference;
                float t2 = s2 / innerCircle.circumference;

                var innerCirclepos1 = innerCircle.GetCirclePosition(t1) + projectCenterOffset;
                var innerCirclepos2 = innerCircle.GetCirclePosition(t2) + projectCenterOffset;
                var outerCirclepos1 = outerCircle.GetCirclePosition(t1) + projectCenterOffset;
                var outerCirclepos2 = outerCircle.GetCirclePosition(t2) + projectCenterOffset;

                // iterate over all decomposed convex polygons of the chop:
                for (int j = 0; j < convexPolygonsCount; j++)
                {
                    var poly = new Polygon(convexPolygons[j]);

                    // calculate 2D UV coordinates for the front polygon.
                    poly.ApplyXYBasedUV0(new Vector2(0.5f, 0.5f));

                    // ensure that the polygon is always on one side of the vertical project center line.
                    poly.Translate(-projectCenterOffset);

                    var backPoly = new Polygon(convexPolygons[j]);
                    var polyCount = poly.Count;

                    // iterate over all cloned vertices:
                    for (int v = 0; v < polyCount; v++)
                    {
                        var vertex = poly[v];

                        var innerVertexPos = Vector3.Lerp(
                            new Vector3(innerCirclepos1.x, vertex.position.y, innerCirclepos1.z),
                            new Vector3(innerCirclepos2.x, vertex.position.y, innerCirclepos2.z),
                            Mathf.InverseLerp(s1, s2, vertex.position.x)
                        );

                        var outerVertexPos = Vector3.Lerp(
                            new Vector3(outerCirclepos1.x, vertex.position.y, outerCirclepos1.z),
                            new Vector3(outerCirclepos2.x, vertex.position.y, outerCirclepos2.z),
                            Mathf.InverseLerp(s1, s2, vertex.position.x)
                        );

                        innerVertexPos.z -= innerCircle.radius;
                        outerVertexPos.z -= innerCircle.radius;

                        if (degrees < 0f)
                        {
                            innerVertexPos.z += distance;
                            outerVertexPos.z += distance;
                        }

                        poly[v] = new Vertex(innerVertexPos, poly[v].uv0, poly[v].hidden);
                        backPoly[v] = new Vertex(outerVertexPos, poly[v].uv0, poly[v].hidden);
                    }

                    if (degrees < 0f)
                    {
                        brush.Add(poly.flipped);
                        brush.Add(backPoly);
                    }
                    else
                    {
                        brush.Add(poly);
                        brush.Add(backPoly.flipped);
                    }

                    // fill the gap with quads "extruding" the shape.
                    Polygon extrudedPolygon;
                    for (int k = 0; k < polyCount - 1; k++)
                    {
                        if (poly[k].hidden) continue;

                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[k],
                            backPoly[k],
                            backPoly[k + 1],
                            poly[k + 1],
                        });

                        extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                        brush.Add(degrees < 0f ? extrudedPolygon.flipped : extrudedPolygon);
                    }

                    // one more face that wraps around to index 0.
                    if (!poly[polyCount - 1].hidden)
                    {
                        extrudedPolygon = new Polygon(new Vertex[] {
                            poly[polyCount - 1],
                            backPoly[polyCount - 1],
                            backPoly[0],
                            poly[0],
                        });

                        extrudedPolygon.ApplyPositionBasedUV0(new Vector2(0.5f, 0.5f));
                        brush.Add(degrees < 0f ? extrudedPolygon.flipped : extrudedPolygon);
                    }
                }

                polygonMeshes.Add(brush);
            }

            var polygonMesh = PolygonMesh.Combine(polygonMeshes);
            var mesh = polygonMesh.ToMesh();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}

#endif