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
        /// <returns>The collection of convex polygons.</returns>
        public List<Polygon> GenerateConvexPolygons()
        {
            var shapesCount = shapes.Count;

            // detect whether we are trying to use boolean operations:
            var useBooleanOperations = false;
            for (int i = 0; i < shapesCount; i++)
            {
                if (shapes[i].csgMode == PolyClipType.Difference)
                {
                    useBooleanOperations = true;
                    break;
                }
            }

            // not using boolean operations, use the stable bayazit path:
            if (!useBooleanOperations)
            {
                // use bayazit convex decomposition to build convex polygons for every shape.
                var bayazitConvexPolygons = new List<Polygon>(shapesCount);
                for (int i = 0; i < shapesCount; i++)
                    bayazitConvexPolygons.AddRange(BayazitDecomposer.ConvexPartition(shapes[i].GenerateConcavePolygon(true)));
                return bayazitConvexPolygons;
            }

            // using boolean operations:

            Debug.LogWarning("2D Shape Editor: Booleans are an experimental feature and not stable!");

            // generate concave polygons for all of the shapes.
            var concavePolygons = new List<Polygon>(shapesCount);

            for (int i = 0; i < shapesCount; i++)
            {
                var shape = shapes[i];
                var shapePolygon = shape.GenerateConcavePolygon(true);

                var tempPolygons = new List<Polygon>(concavePolygons);
                var tempPolygonsCount = tempPolygons.Count;
                concavePolygons.Clear();

                if (tempPolygonsCount == 0)
                {
                    concavePolygons.Add(shapePolygon);
                }
                else
                {
                    if (shape.csgMode == PolyClipType.Union)
                    {
                        concavePolygons.AddRange(tempPolygons);
                        concavePolygons.Add(shapePolygon);

                        //for (int j = 0; j < tempPolygonsCount; j++)
                        //{
                        //    concavePolygons.AddRange(YuPengClipper.Union(tempPolygons[j], shapePolygon, out var error));
                        //    if (error != PolyClipError.None)
                        //        Debug.Log(error);
                        //}
                    }
                    else
                    {
                        for (int j = 0; j < tempPolygonsCount; j++)
                        {
                            concavePolygons.AddRange(YuPengClipper.Difference(tempPolygons[j], shapePolygon, out var error));
                            if (error != PolyClipError.None)
                                Debug.Log(error);
                        }
                    }
                }
            }

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
                    // if there are holes, provide every polygon with the list of holes.
                    if (hasHoles)
                    {
                        concavePolygons[i].Holes = new List<Polygon>();
                        foreach (var hole in holes)
                        {
                            if (concavePolygons[i].ConvexContains(hole))
                            {
                                concavePolygons[i].Holes.Add(hole);
                            }
                        }
                    }

                    convexPolygons.AddRange(Delaunay.DelaunayDecomposer.ConvexPartition(concavePolygons[i]));
                }
            }

            return convexPolygons;
        }
    }
}

#endif