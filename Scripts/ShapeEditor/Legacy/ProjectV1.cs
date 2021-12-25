#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a legacy V1 project from the SabreCSG 2D Shape Editor.</summary>
    [Serializable]
    public class ProjectV1
    {
        /// <summary>
        /// The project version, in case of 'severe' future updates.
        /// </summary>
        [SerializeField]
        public int version = 1;

        /// <summary>
        /// The shapes in the project.
        /// </summary>
        [SerializeField]
        public List<Shape> shapes = new List<Shape>()
        {
            new Shape()
        };

        /// <summary>
        /// The global pivot in the project.
        /// </summary>
        [SerializeField]
        public Pivot globalPivot = new Pivot();

        /// <summary>
        /// Whether the project was flipped horizontally.
        /// </summary>
        [SerializeField]
        public bool flipHorizontally = false;

        /// <summary>
        /// Whether the project was flipped vertically.
        /// </summary>
        [SerializeField]
        public bool flipVertically = false;

        /// <summary>
        /// The extrude depth used on the most recent extrude.
        /// </summary>
        [SerializeField]
        public float extrudeDepth = 1.0f;

        /// <summary>
        /// The extrude clip depth used on the most recent extrude.
        /// </summary>
        [SerializeField]
        public float extrudeClipDepth = 0.5f;

        /// <summary>
        /// The scale modifier values used on the most recent extrude.
        /// </summary>
        [SerializeField]
        public Vector2 extrudeScale = Vector2.one;

        /// <summary>
        /// The how many steps it takes to revolve 360 degrees.
        /// </summary>
        [SerializeField]
        public int revolve360 = 8;

        /// <summary>
        /// The amount of steps used.
        /// </summary>
        [SerializeField]
        public int revolveSteps = 4;

        /// <summary>
        /// The revolve distance as determined by the project's global pivot.
        /// </summary>
        [SerializeField]
        public int revolveDistance = 1;

        /// <summary>
        /// The revolve radius as determined by the project's global pivot.
        /// </summary>
        [SerializeField]
        public int revolveRadius = 1;

        /// <summary>
        /// The revolve direction (true is right, false is left).
        /// </summary>
        [SerializeField]
        public bool revolveDirection = true;

        /// <summary>
        /// Whether the spiral is like stairs or a smooth slope.
        /// </summary>
        [SerializeField]
        public bool revolveSpiralSloped = false;

        /// <summary>
        /// Whether the shape uses Convex Decomposition or Concave Shapes.
        /// </summary>
        [SerializeField]
        public bool convexBrushes = true;

        /// <summary>
        /// Clones this project and returns the copy.
        /// </summary>
        /// <returns>A copy of the project.</returns>
        public ProjectV1 Clone()
        {
            // create a copy of the given project using JSON.
            return JsonUtility.FromJson<ProjectV1>(JsonUtility.ToJson(this));
        }

        /// <summary>
        /// A 2D Shape Editor Shape.
        /// </summary>
        [Serializable]
        public class Shape
        {
            /// <summary>
            /// The segments of the shape.
            /// </summary>
            [SerializeField]
            public List<Segment> segments = new List<Segment>() {
                new Segment(-8, -8),
                new Segment( 8, -8),
                new Segment( 8,  8),
                new Segment(-8,  8),
            };

            /// <summary>
            /// The center pivot of the shape.
            /// </summary>
            public Pivot pivot = new Pivot();

            /// <summary>
            /// Calculates the pivot position so that it's centered on the shape.
            /// </summary>
            public void CalculatePivotPosition()
            {
                Vector2Int center = new Vector2Int();
                foreach (Segment segment in segments)
                    center += segment.position;
                pivot.position = new Vector2Int(center.x / segments.Count, center.y / segments.Count);
            }

            /// <summary>
            /// Clones this shape and returns the copy.
            /// </summary>
            /// <returns>A copy of the shape.</returns>
            public Shape Clone()
            {
                // create a copy of the given shape using JSON.
                return JsonUtility.FromJson<Shape>(JsonUtility.ToJson(this));
            }
        }

        /// <summary>
        /// A 2D Shape Editor Segment.
        /// </summary>
        [Serializable]
        public class Segment : ISelectable
        {
            /// <summary>
            /// The position of the segment on the grid.
            /// </summary>
            [SerializeField]
            private Vector2Int _position;

            /// <summary>
            /// The position of the segment on the grid.
            /// </summary>
            public Vector2Int position
            {
                get { return _position; }
                set { _position = value; }
            }

            /// <summary>
            /// The segment type.
            /// </summary>
            [SerializeField]
            public SegmentType type = SegmentType.Linear;

            /// <summary>
            /// The first bezier pivot (see <see cref="SegmentType.Bezier"/>).
            /// </summary>
            [SerializeField]
            public Pivot bezierPivot1 = new Pivot();

            /// <summary>
            /// The second bezier pivot (see <see cref="SegmentType.Bezier"/>).
            /// </summary>
            [SerializeField]
            public Pivot bezierPivot2 = new Pivot();

            [SerializeField]
            public int bezierDetail = 3;

            /// <summary>
            /// Initializes a new instance of the <see cref="Segment"/> class.
            /// </summary>
            /// <param name="x">The x-coordinate on the grid.</param>
            /// <param name="y">The y-coordinate on the grid.</param>
            public Segment(int x, int y)
            {
                this.position = new Vector2Int(x, y);
            }
        }

        /// <summary>
        /// The type of 2D segment.
        /// </summary>
        public enum SegmentType
        {
            Linear,
            Bezier
        }

        /// <summary>
        /// A 2D Shape Editor Pivot.
        /// </summary>
        [Serializable]
        public class Pivot : ISelectable
        {
            /// <summary>
            /// The position of the pivot on the grid.
            /// </summary>
            [SerializeField]
            private Vector2Int _position;

            /// <summary>
            /// The position of the pivot on the grid.
            /// </summary>
            public Vector2Int position
            {
                get { return _position; }
                set { _position = value; }
            }
        }

        /// <summary>
        /// Any object that can be selected in the 2D Shape Editor.
        /// </summary>
        public interface ISelectable
        {
            /// <summary>
            /// The position of the object on the grid.
            /// </summary>
            Vector2Int position { get; set; }
        }
    }
}

#endif