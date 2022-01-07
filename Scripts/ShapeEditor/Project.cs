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

            // generate concave polygons for all of the shapes.
            var concavePolygons = new List<Polygon>(shapesCount);
            for (int i = 0; i < shapesCount; i++)
                concavePolygons.Add(shapes[i].GenerateConcavePolygon(true));

            // apply csg operations on the concave polygons.
            var finalPolygons = new List<Polygon>(shapesCount);
            for (int i = 0; i < shapesCount; i++)
            {
                /*finalPolygons.AddRange(
                    YuPengClipper.Union(concavePolygons[i], new Shape().GenerateConcavePolygon(true), out var error)
                );*/

                finalPolygons.Add(concavePolygons[i]);
            }

            // use convex decomposition to build convex polygons out of the final polygons.
            var convexPolygons = new List<Polygon>();
            var finalPolygonsCount = finalPolygons.Count;
            for (int i = 0; i < finalPolygonsCount; i++)
                convexPolygons.AddRange(BayazitDecomposer.ConvexPartition(finalPolygons[i]));

            return convexPolygons;
        }
    }
}

#endif