#if UNITY_EDITOR

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

        /// <summary>Creates the default linear generator that builds straight lines.</summary>
        public SegmentGenerator()
        {
            type = SegmentGeneratorType.Linear;
        }

        /// <summary>
        /// Creates a segment generator, the following types are supported with this constructor:
        /// <para><see cref="SegmentGeneratorType.Bezier"/>, <see cref="SegmentGeneratorType.Sine"/></para>
        /// </summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The segment that has the modifier.</param>
        /// <param name="type">The segment generator type to create.</param>
        public SegmentGenerator(ShapeEditorWindow editor, Segment segment, SegmentGeneratorType type)
        {
            this.type = type;

            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_Constructor(editor, segment);
                    break;

                case SegmentGeneratorType.Sine:
                    Sine_Constructor(editor, segment);
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
        /// <param name="segment">The segment that has the modifier.</param>
        public void DrawSegments(ShapeEditorWindow editor, Segment segment)
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    Linear_DrawSegments(editor, segment);
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_DrawSegments(editor, segment);
                    break;

                case SegmentGeneratorType.Sine:
                    Sine_DrawSegments(editor, segment);
                    break;
            }
        }

        /// <summary>Draws the modifier pivots to the screen.</summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The segment that has the modifier.</param>
        public void DrawPivots(ShapeEditorWindow editor, Segment segment)
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_DrawPivots(editor, segment);
                    break;

                case SegmentGeneratorType.Sine:
                    Sine_DrawPivots(editor, segment);
                    break;
            }
        }

        /// <summary>Iterates over the generated segment points in grid coordinates.</summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The segment that has the modifier.</param>
        public IEnumerable<float2> ForEachSegmentPoint(ShapeEditorWindow editor, Segment segment)
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    foreach (var point in Linear_ForEachSegmentPoint(editor, segment))
                        yield return point;
                    break;

                case SegmentGeneratorType.Bezier:
                    foreach (var point in Bezier_ForEachSegmentPoint(editor, segment))
                        yield return point;
                    break;

                case SegmentGeneratorType.Sine:
                    foreach (var point in Sine_ForEachSegmentPoint(editor, segment))
                        yield return point;
                    break;
            }
            yield break;
        }

        /// <summary>Iterates over the enumerable of grid coordinate points to draw segments.</summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The segment that has the modifier.</param>
        /// <param name="iterator">The generated segment points in grid coordinates.</param>
        private void DrawSegments(ShapeEditorWindow editor, Segment segment, IEnumerable<float2> iterator)
        {
            var enumerator = iterator.GetEnumerator();
            enumerator.MoveNext();
            float2 last = editor.GridPointToScreen(enumerator.Current);

            GL.Color(segment.selected ? ShapeEditorWindow.segmentPivotOutlineColor : ShapeEditorWindow.segmentColor);
            while (enumerator.MoveNext())
            {
                float2 next = editor.GridPointToScreen(enumerator.Current);
                GLUtilities.DrawLine(1.0f, last.x, last.y, next.x, next.y);
                last = next;
            }
        }
    }
}

#endif