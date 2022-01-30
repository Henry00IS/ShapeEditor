#if UNITY_EDITOR

using System;
using System.Collections.Generic;
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
        public List<Polygon> GenerateConvexPolygons(bool useHoles = true)
        {
            var shapesCount = shapes.Count;

            // using boolean operations:

            // generate concave polygons for all of the shapes.

            var polyBool = new PolyBoolCS.PolyBool();
            var finalSegmentList = new PolyBoolCS.SegmentList();

            for (int i = 0; i < shapesCount; i++)
            {
                var shape = shapes[i];
                var shapePolygon = shape.GenerateConcavePolygon(true);
                var shapePolyboolPolygon = shapePolygon.ToPolybool();

                if (shape.booleanOperator == PolygonBooleanOperator.Union)
                {
                    var seg2 = polyBool.segments(shapePolyboolPolygon);
                    var comb = polyBool.combine(finalSegmentList, seg2);
                    finalSegmentList = polyBool.selectUnion(comb);
                }
                else
                {
                    var seg2 = polyBool.segments(shapePolyboolPolygon);
                    var comb = polyBool.combine(finalSegmentList, seg2);
                    finalSegmentList = polyBool.selectDifference(comb);
                }
            }

            var concavePolygons = polyBool.polygon(finalSegmentList).ToPolygons(polyBool);
            var concavePolygonsCount = concavePolygons.Count;

            // find clockwise polygons (holes):
            var holes = new List<Polygon>();
            for (int i = 0; i < concavePolygonsCount; i++)
                if (!concavePolygons[i].IsCounterClockWise2D())
                    holes.Add(concavePolygons[i]);
            var hasHoles = holes.Count > 0;

            // use convex decomposition to build convex polygons out of the concave polygons.
            var convexPolygons = new List<Polygon>();
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
                MarkHiddenSurfaces(convexPolygons);
            }

            return convexPolygons;
        }

        /// <summary>
        /// The hidden surface removal algorithm, preventing interior 3D polygons. It iterates over
        /// all convex polygons and marks whether this and the next vertex are part of a hidden edge
        /// that should not be extruded.
        /// </summary>
        /// <param name="convexPolygons">The collection of 2D polygons of <see cref="GenerateConvexPolygons"/></param>
        private void MarkHiddenSurfaces(List<Polygon> convexPolygons)
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

                    // iterate over every 2d convex polygon:
                    for (int k = 0; k < convexPolygonsCount; k++)
                    {
                        // skip if we are about to compare the polygon against the same polygon.
                        if (j == k) continue;

                        // check whether the other polygon contains the center point.
                        if (convexPolygons[k].ContainsPoint2D(ref center) >= 0)
                        {
                            // mark the edge as hidden.
                            convexPolygons[j][i] = new Vertex(thisVertex.position, thisVertex.uv0, true);
                            break;
                        }
                    }
                }
            }
        }
    }
}

#endif