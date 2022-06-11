#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A 2D Shape Editor Project.</summary>
    [Serializable]
    public class Project
    {
        /// <summary>The project version, in case of 'severe' future updates.</summary>
        [SerializeField]
        public int version = 2; // 1 is SabreCSG's 2D Shape Editor.

        /// <summary>The shapes in the project.</summary>
        [SerializeField]
        public List<Shape> shapes = new List<Shape>()
        {
            new Shape()
        };

        /// <summary>The global pivot in the project.</summary>
        [SerializeField]
        public Pivot globalPivot = new Pivot();

        /// <summary>Clones this project and returns the copy.</summary>
        /// <returns>A copy of the project.</returns>
        public Project Clone()
        {
            // create a copy of the given project using JSON.
            return JsonUtility.FromJson<Project>(JsonUtility.ToJson(this));
        }

        /// <summary>Selects all of the selectable objects in the project.</summary>
        public void SelectAll()
        {
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
                shapes[i].SelectAll();
        }

        /// <summary>Clears the selection of all selectable objects in the project.</summary>
        public void ClearSelection()
        {
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
                shapes[i].ClearSelection();
        }

        /// <summary>Inverts the selection all of the selectable objects in the project.</summary>
        public void InvertSelection()
        {
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
                shapes[i].InvertSelection();
        }

        /// <summary>Gets an AABB that fully contains the project's segments.</summary>
        /// <param name="flipY">Whether to mirror the shapes vertically.</param>
        public Bounds GetAABB(bool flipY)
        {
            var count = shapes.Count;
            if (count == 0)
                return default;

            Bounds bounds = shapes[0].GetAABB(flipY);
            for (int i = 1; i < count; i++)
                bounds.Encapsulate(shapes[i].GetAABB(flipY));

            return bounds;
        }

        [NonSerialized]
        private bool isValid = false;

        /// <summary>Ensures all data in the project is ready to go (especially after C# reloads).</summary>
        /// <param name="editor">The shape editor window.</param>
        public void Validate()
        {
            if (!isValid)
            {
                isValid = true;

                // validate every shape in the project:
                var shapesCount = shapes.Count;
                for (int i = 0; i < shapesCount; i++)
                    shapes[i].Validate();
            }
        }

        /// <summary>
        /// Generates concave polygons for all shapes and applies their boolean operators into a
        /// segment list representing this project.
        /// </summary>
        private PolyBoolCS.SegmentList GenerateConcaveSegmentList(PolyBoolCS.PolyBool polyBool)
        {
            var result = new PolyBoolCS.SegmentList();

            // iterate over every shape in the project:
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = shapes[i];

                // generate concave polygons:
                var shapePolygons = shape.GenerateConcavePolygons(true);
                for (int j = 0; j < shapePolygons.Length; j++)
                {
                    // turn the concave polygons into a polybool segment list:
                    var shapePolygon = shapePolygons[j];
                    var shapePolyboolPolygon = shapePolygon.ToPolybool();

                    // apply the boolean operation (union or difference) on the final result.
                    if (shape.booleanOperator == PolygonBooleanOperator.Union)
                    {
                        var seg2 = polyBool.segments(shapePolyboolPolygon);
                        var comb = polyBool.combine(result, seg2);
                        result = polyBool.selectUnion(comb);
                    }
                    else
                    {
                        var seg2 = polyBool.segments(shapePolyboolPolygon);
                        var comb = polyBool.combine(result, seg2);
                        result = polyBool.selectDifference(comb);
                    }
                }
            }

            return result;
        }

        /// <summary>Decomposes a segment list into multiple convex polygons.</summary>
        private PolygonMesh SegmentListToConvexPolygonMesh(PolyBoolCS.PolyBool polyBool, PolyBoolCS.SegmentList segmentList, bool useHoles)
        {
            var concavePolygons = polyBool.polygon(segmentList).ToPolygons(polyBool);
            var concavePolygonsCount = concavePolygons.Count;

            // find clockwise polygons (holes):
            var holes = new List<Polygon>();
            for (int i = 0; i < concavePolygonsCount; i++)
                if (!concavePolygons[i].IsCounterClockWise2D())
                    holes.Add(concavePolygons[i]);
            var hasHoles = holes.Count > 0;

            // use convex decomposition to build convex polygons out of the concave polygons.
            var convexPolygons = new PolygonMesh();
            for (int i = 0; i < concavePolygonsCount; i++)
            {
                if (concavePolygons[i].IsCounterClockWise2D())
                {
                    if (useHoles)
                    {
                        // if there are holes, provide every polygon with the list of holes.
                        if (hasHoles)
                        {
                            concavePolygons[i].Holes = new List<Polygon>();
                            foreach (var hole in holes)
                            {
                                if (concavePolygons[i].ConvexContains(hole)) // fixme: this is surely wrong?
                                {
                                    concavePolygons[i].Holes.Add(hole);
                                }
                            }
                        }

                        if (concavePolygons[i].Holes?.Count > 0)
                        {
                            convexPolygons.AddRange(Delaunay.DelaunayDecomposer.ConvexPartition(concavePolygons[i]));
                        }
                        else
                        {
                            // use bayazit whenever we can because it's fast and gives great results.
                            convexPolygons.AddRange(BayazitDecomposer.ConvexPartition(concavePolygons[i]));
                        }
                    }
                    else
                    {
                        convexPolygons.AddRange(BayazitDecomposer.ConvexPartition(concavePolygons[i]));
                    }
                }
            }

            if (!useHoles)
            {
                // for every hole:
                var holesCount = holes.Count;
                for (int i = 0; i < holesCount; i++)
                {
                    // holes are guaranteed to be clockwise, but we need them counter-clockwise.
                    holes[i].Reverse();

                    // decompose the hole into convex polygons:
                    var holeConvexPolygons = new List<Polygon>();
                    holeConvexPolygons.AddRange(BayazitDecomposer.ConvexPartition(holes[i]));
                    var holeConvexPolygonsCount = holeConvexPolygons.Count;

                    for (int j = 0; j < holeConvexPolygonsCount; j++)
                    {
                        // set the boolean operator for the CSG target:
                        holeConvexPolygons[j].booleanOperator = PolygonBooleanOperator.Difference;

                        // add it to the results.
                        convexPolygons.Add(holeConvexPolygons[j]);
                    }
                }
            }
            else
            {
                // mark hidden edges in 2d to prevent building interior 3d polygons. in the extrude
                // functions the vertices are always visited from index zero upwards, so we can mark
                // the first vertex of an edge as being a hidden surface.
                MarkHiddenSurfaces(convexPolygons, segmentList);
            }

            // cleanup step that removes degenerate polygons and polygons with less than 3 sides.
            CleanupPolygons(convexPolygons);

            return convexPolygons;
        }

        /// <summary>
        /// [2D] Decomposes all shapes into convex polygons representing this project. Then
        /// horizontally chops the project into multiple slices using intersect operations.
        /// <para>The Y coordinate will be flipped to match X and Y in 3D space.</para>
        /// </summary>
        /// <param name="useHoles">
        /// Whether holes are used in the convex decomposition algorithm. If false then holes will
        /// be added to the result with their boolean operator set to <see
        /// cref="PolygonBooleanOperator.Difference"/> for use by CSG targets. The use of holes with
        /// convex decomposition leads to many brushes, which can be avoided by using the
        /// subtractive brushes of the CSG algorithm.
        /// </param>
        /// <returns>The collection of chopped polygon meshes with convex polygons.</returns>
        public PolygonMeshes GenerateChoppedPolygons(int chopCount, bool useHoles = true)
        {
            var meshes = new PolygonMeshes(chopCount);

            // build a segment list representing this project.
            var polyBool = new PolyBoolCS.PolyBool();
            var projectSegmentList = GenerateConcaveSegmentList(polyBool);
            var projectPolygons = polyBool.polygon(projectSegmentList);

            // we chop it horizontally by using multiple intersect operations.
            var projectBounds = GetAABB(true);
            var chopWidth = projectBounds.size.x / chopCount;

            for (int i = 0; i < chopCount; i++)
            {
                var chopX1 = projectBounds.min.x + (chopWidth * i);
                var chopX2 = chopX1 + chopWidth;

                var intersectPolygon = new PolyBoolCS.Polygon()
                {
                    regions = new List<PolyBoolCS.PointList>() {
                        new PolyBoolCS.PointList() {
                            new PolyBoolCS.Point(chopX1, projectBounds.min.y),
                            new PolyBoolCS.Point(chopX2, projectBounds.min.y),
                            new PolyBoolCS.Point(chopX2, projectBounds.max.y),
                            new PolyBoolCS.Point(chopX1, projectBounds.max.y),
                        }
                    }
                };

                // build convex polygons out of the intersect segment list.
                var combine = polyBool.combine(polyBool.segments(projectPolygons), polyBool.segments(intersectPolygon));
                meshes.Add(SegmentListToConvexPolygonMesh(polyBool, polyBool.selectIntersect(combine), useHoles));
            }

            return meshes;
        }

        /// <summary>
        /// [2D] Decomposes all shapes into convex polygons representing this project.
        /// <para>The Y coordinate will be flipped to match X and Y in 3D space.</para>
        /// </summary>
        /// <param name="useHoles">
        /// Whether holes are used in the convex decomposition algorithm. If false then holes will
        /// be added to the result with their boolean operator set to <see
        /// cref="PolygonBooleanOperator.Difference"/> for use by CSG targets. The use of holes with
        /// convex decomposition leads to many brushes, which can be avoided by using the
        /// subtractive brushes of the CSG algorithm.
        /// </param>
        /// <returns>The collection of convex polygons.</returns>
        public PolygonMesh GenerateConvexPolygons(bool useHoles = true)
        {
            // build a segment list representing this project.
            var polyBool = new PolyBoolCS.PolyBool();
            var projectSegmentList = GenerateConcaveSegmentList(polyBool);

            // build convex polygons out of the segment list.
            return SegmentListToConvexPolygonMesh(polyBool, projectSegmentList, useHoles);
        }

        /// <summary>
        /// A cleanup step that removes degenerate polygons and polygons with less than 3 sides.
        /// </summary>
        /// <param name="convexPolygons">The collection of 2D polygons of <see cref="GenerateConvexPolygons"/></param>
        private void CleanupPolygons(List<Polygon> convexPolygons)
        {
            // iterate over every 2d convex polygon:
            var convexPolygonsCount = convexPolygons.Count;
            for (int j = convexPolygonsCount; j-- > 0;)
            {
                var vertices = convexPolygons[j];
                var vertexCount = vertices.Count;

                // remove polygons with less than 3 sides.
                if (vertexCount < 3)
                {
                    convexPolygons.RemoveAt(j);
                    continue;
                }

                // remove degenerate polygons.
                if (vertices.GetSignedArea2D() == 0f)
                {
                    convexPolygons.RemoveAt(j);
                    continue;
                }
            }
        }

        /// <summary>
        /// The hidden surface removal algorithm, preventing interior 3D polygons. It iterates over
        /// all convex polygons and marks whether this and the next vertex are part of a hidden edge
        /// that should not be extruded.
        /// </summary>
        /// <param name="convexPolygons">The collection of 2D polygons of <see cref="GenerateConvexPolygons"/></param>
        private void MarkHiddenSurfaces(List<Polygon> convexPolygons, PolyBoolCS.SegmentList segmentList)
        {
            // iterate over every 2d convex polygon:
            var convexPolygonsCount = convexPolygons.Count;
            for (int j = 0; j < convexPolygonsCount; j++)
            {
                // for every vertex in the polygon:
                var vertices = convexPolygons[j];
                var vertexCount = vertices.Count;
                for (int i = 0; i < vertexCount; i++)
                {
                    // find the center position of the edge.
                    var thisVertex = vertices[i];
                    var nextVertex = vertices.NextVertex(i);
                    var center = Vector3.Lerp(thisVertex.position, nextVertex.position, 0.5f);

                    bool hide = true;
                    foreach (var segment in segmentList)
                    {
                        if (MathEx.IsPointOnLine2(
                            new float2(center.x, center.y),
                            new float2((float)segment.start.x, (float)segment.start.y),
                            new float2((float)segment.end.x, (float)segment.end.y),
                            0.0001403269f
                        ))
                        {
                            hide = false;
                            break;
                        }
                    }
                    // mark the edge as hidden.
                    if (hide)
                        convexPolygons[j][i] = new Vertex(thisVertex.position, thisVertex.uv0, true);
                }
            }
        }
    }
}

#endif