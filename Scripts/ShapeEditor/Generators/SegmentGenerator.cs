#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A segment generator that generates additional points between two segments.</summary>
    [System.Serializable]
    public partial class SegmentGenerator
    {
        // this has been written as partial classes because unity's serialization system did not
        // play along with polymorphism.

        [SerializeField]
        public SegmentGeneratorType type = SegmentGeneratorType.Linear;

        /// <summary>The segment using this generator (read only).</summary>
        [NonSerialized]
        public Segment segment;

        /// <summary>The shape editor window.</summary>
        [NonSerialized]
        public ShapeEditorWindow editor;

        /// <summary>Creates the default linear generator that builds straight lines.</summary>
        /// <param name="segment">The segment that has the modifier.</param>
        public SegmentGenerator(Segment segment)
        {
            this.segment = segment;
            type = SegmentGeneratorType.Linear;
        }

        /// <summary>
        /// Creates a segment generator, the following types are supported with this constructor:
        /// <para><see cref="SegmentGeneratorType.Linear"/>, <see cref="SegmentGeneratorType.Bezier"/>, <see cref="SegmentGeneratorType.Sine"/></para>
        /// </summary>
        /// <param name="segment">The segment that has the modifier.</param>
        /// <param name="type">The segment generator type to create.</param>
        public SegmentGenerator(Segment segment, SegmentGeneratorType type)
        {
            this.segment = segment;
            this.type = type;

            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_Constructor();
                    break;

                case SegmentGeneratorType.Sine:
                    Sine_Constructor();
                    break;
            }
        }

        /// <summary>Iterates over all of the selectable objects.</summary>
        public IEnumerable<ISelectable> ForEachSelectableObject()
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    yield break;

                case SegmentGeneratorType.Bezier:
                    foreach (var selectable in Bezier_ForEachSelectableObject())
                        yield return selectable;
                    yield break;

                case SegmentGeneratorType.Sine:
                    foreach (var selectable in Sine_ForEachSelectableObject())
                        yield return selectable;
                    yield break;
            }
        }

        /// <summary>Draws the modifier segments to the screen.</summary>
        /// <param name="editor">The shape editor window.</param>
        public void DrawSegments(ShapeEditorWindow editor)
        {
            this.editor = editor;

            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    Linear_DrawSegments();
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_DrawSegments();
                    break;

                case SegmentGeneratorType.Sine:
                    Sine_DrawSegments();
                    break;

                case SegmentGeneratorType.Repeat:
                    Repeat_DrawSegments();
                    break;
            }
        }

        /// <summary>Draws the modifier pivots to the screen.</summary>
        /// <param name="editor">The shape editor window.</param>
        public void DrawPivots(ShapeEditorWindow editor)
        {
            this.editor = editor;

            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_DrawPivots();
                    break;

                case SegmentGeneratorType.Sine:
                    Sine_DrawPivots();
                    break;
            }
        }

        /// <summary>Iterates over the generated segment points in grid coordinates.</summary>
        public IEnumerable<float2> ForEachAdditionalSegmentPoint()
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    yield break;

                case SegmentGeneratorType.Bezier:
                    foreach (var point in Bezier_ForEachSegmentPoint())
                        yield return point;
                    yield break;

                case SegmentGeneratorType.Sine:
                    foreach (var point in Sine_ForEachSegmentPoint())
                        yield return point;
                    yield break;

                case SegmentGeneratorType.Repeat:
                    foreach (var point in Repeat_ForEachSegmentPoint())
                        yield return point;
                    yield break;
            }
        }

        /// <summary>Iterates over the enumerable of grid coordinate points to draw segments.</summary>
        /// <param name="iterator">The generated segment points in grid coordinates.</param>
        private void DrawSegments(IEnumerable<float2> iterator)
        {
            float2 last = editor.GridPointToScreen(segment.position);
            GL.Color((segment.selected && segment.next.selected) ? ShapeEditorWindow.segmentPivotOutlineColor : segment.shape.segmentColor);

            foreach (var point in iterator)
            {
                float2 next = editor.GridPointToScreen(point);
                GLUtilities.DrawLine(1.0f, last.x, last.y, next.x, next.y);
                last = next;
            }

            float2 final = editor.GridPointToScreen(segment.next.position);
            GLUtilities.DrawLine(1.0f, last.x, last.y, final.x, final.y);
        }

        /// <summary>Applies a generator by inserting the generated points as new segments.</summary>
        public void ApplyGenerator()
        {
            var next = segment.next;

            foreach (var point in ForEachAdditionalSegmentPoint())
                segment.shape.InsertSegmentBefore(next, new Segment(segment.shape, point));
        }
    }
}

#endif