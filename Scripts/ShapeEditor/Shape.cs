#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A 2D Shape Editor Shape.</summary>
    [Serializable]
    public class Shape
    {
        /// <summary>The segments of the shape.</summary>
        [SerializeField]
        public List<Segment> segments = new List<Segment>();

        /// <summary>The center pivot of the shape.</summary>
        public Pivot pivot = new Pivot();

        /// <summary>Creates a new shape.</summary>
        public Shape()
        {
            // by default we build a box shape.
            ResetToBox();
        }

        /// <summary>Resets the shape to a box shape.</summary>
        public void ResetToBox()
        {
            segments.Clear();
            segments.Add(new Segment(this, -8, -8));
            segments.Add(new Segment(this, 8, -8));
            segments.Add(new Segment(this, 8, 8));
            segments.Add(new Segment(this, -8, 8));
        }

        /// <summary>Calculates the pivot position so that it's centered on the shape.</summary>
        public void CalculatePivotPosition()
        {
            float2 center = new float2();
            foreach (Segment segment in segments)
                center += segment.position;
            pivot.position = new float2(center.x / segments.Count, center.y / segments.Count);
        }

        /// <summary>Clones this shape and returns the copy.</summary>
        /// <returns>A copy of the shape.</returns>
        public Shape Clone()
        {
            // create a copy of the given shape using JSON.
            return JsonUtility.FromJson<Shape>(JsonUtility.ToJson(this));
        }
    }
}

#endif