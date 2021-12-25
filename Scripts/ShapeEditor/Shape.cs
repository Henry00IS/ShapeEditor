#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A 2D Shape Editor Shape.</summary>
    [Serializable]
    public class Shape
    {
        /// <summary>The segments of the shape. WARNING: DO NOT MODIFY DIRECTLY!</summary>
        [SerializeField]
        public List<Segment> segments = new List<Segment>();

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
            AddSegment(new Segment(this, -0.5f, -0.5f));
            AddSegment(new Segment(this, 0.5f, -0.5f));
            AddSegment(new Segment(this, 0.5f, 0.5f));
            AddSegment(new Segment(this, -0.5f, 0.5f));
        }

        /// <summary>Clears the selection of all selectable objects in the shape.</summary>
        public void ClearSelection()
        {
            var segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++)
            {
                var segment = segments[i];
                segment.selected = false;

                if (segment.generator.type != SegmentGeneratorType.Linear)
                    foreach (var modifierSelectable in segment.generator.ForEachSelectableObject())
                        modifierSelectable.selected = false;
            }
        }

        /// <summary>
        /// Adds a segment to the end of the shape. This is usually used while generating shapes.
        /// </summary>
        /// <param name="segment">The segment to be added to the shape.</param>
        public void AddSegment(Segment segment)
        {
            var segmentsCount = segments.Count;

            if (segmentsCount == 0)
            {
                segment.previous = segment;
                segment.next = segment;
            }
            else
            {
                var last = segments[segmentsCount - 1];

                last.next = segment;
                segments[0].previous = segment;
                segment.previous = last;
                segment.next = segments[0];
            }

            segments.Add(segment);
        }

        /// <summary>Inserts a segment in front of another segment.</summary>
        /// <param name="current">The segment the new segment will be inserted in front of.</param>
        /// <param name="segment">The segment to be inserted into the shape.</param>
        public void InsertSegmentBefore(Segment current, Segment segment)
        {
            var segmentsCount = segments.Count;

            if (segmentsCount == 0)
            {
                Debug.LogWarning("Tried to insert a segment before a segment that isn't part of the shape!");
                AddSegment(segment);
            }
            else
            {
                var index = segments.IndexOf(current);
                if (index == -1)
                {
                    Debug.LogWarning("Tried to insert a segment before a segment that isn't part of the shape!");
                    return;
                }

                current.previous.next = segment;
                segment.previous = current.previous;
                segment.next = current;
                current.previous = segment;

                segments.Insert(index, segment);
            }
        }

        /// <summary>Removes a segment from the shape.</summary>
        /// <param name="segment">The segment to be removed.</param>
        public void RemoveSegment(Segment segment)
        {
            var index = segments.IndexOf(segment);
            if (index == -1)
            {
                Debug.LogWarning("Tried to remove a segment that isn't part of the shape!");
                return;
            }

            segment.next.previous = segment.previous;
            segment.previous.next = segment.next;

            segments.RemoveAt(index);
        }

        /// <summary>Ensures all data in the shape is ready to go (especially after C# reloads).</summary>
        public void Validate()
        {
            // for every segment in the shape:
            var segmentsCount = segments.Count;
            for (int i = 0; i < segmentsCount; i++)
            {
                // fill the segment with references.

                var segment = segments[i];
                segment.shape = this;

                // recalculate the segment indices.

                var previous = segments[i - 1 < 0 ? segmentsCount - 1 : i - 1];
                var next = segments[i + 1 >= segmentsCount ? 0 : i + 1];

                segment.previous = previous;
                segment.next = next;

                // fill the generators with references.

                if (segment.generator == null)
                    segment.generator = new SegmentGenerator(segment);
                else
                    segment.generator.segment = segment;
            }
        }

        /// <summary>Generates the vertices representing this shape for convex decomposition.</summary>
        /// <returns>The collection of vertices.</returns>
        public Vertices GenerateVertices()
        {
            Vertices vertices = new Vertices();

            // for every segment in the shape:
            var segmentsCount = segments.Count;
            for (int j = 0; j < segmentsCount; j++)
            {
                // add the segment point.
                var segment = segments[j];
                vertices.Add(segment.position);

                // have the segment generator add additional points.
                foreach (var point in segments[j].generator.ForEachAdditionalSegmentPoint())
                    vertices.Add(point);
            }

            // ensure the vertices are counter clockwise.
            vertices.ForceCounterClockWise();

            return vertices;
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