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

                case SegmentGeneratorType.Arch:
                    Arch_DrawSegments();
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

                case SegmentGeneratorType.Arch:
                    foreach (var point in Arch_ForEachSegmentPoint())
                        yield return point;
                    yield break;
            }
        }

        /// <summary>Iterates over the enumerable of grid coordinate points to draw segments.</summary>
        /// <param name="iterator">The generated segment points in grid coordinates.</param>
        private void DrawSegments(IEnumerable<float2> iterator)
        {
            float2 last = segment.position;
            var color = (segment.selected && segment.next.selected) ? ShapeEditorWindow.segmentPivotOutlineColor : segment.shape.segmentColor;

            foreach (var point in iterator)
            {
                DrawGridLine(1.0f, last, point, color);
                last = point;
            }

            DrawGridLine(1.0f, last, segment.next.position, color);
        }

        private static readonly float2 mirrorXY = new float2(-1f, -1f);
        private static readonly float2 mirrorX = new float2(-1f, 1f);
        private static readonly float2 mirrorY = new float2(1f, -1f);

        /// <summary>
        /// <see cref="GLUtilities.DrawLine"/> in grid coordinates that takes shape symmetry into account.
        /// </summary>
        private void DrawGridLine(float thickness, float2 from, float2 to, Color beginColor, Color endColor)
        {
            var p1 = editor.GridPointToScreen(from);
            var p2 = editor.GridPointToScreen(to);
            GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y, beginColor, endColor);

            var symmetry = segment.shape.symmetryAxes;
            if (symmetry != SimpleGlobalAxis.None)
            {
                bool flipX = symmetry.HasFlag(SimpleGlobalAxis.Horizontal);
                bool flipY = symmetry.HasFlag(SimpleGlobalAxis.Vertical);

                beginColor.a = 0.75f;
                endColor.a = 0.75f;

                if (flipX && flipY)
                {
                    p1 = editor.GridPointToScreen(from * mirrorXY);
                    p2 = editor.GridPointToScreen(to * mirrorXY);
                    GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y, beginColor, endColor);
                }

                if (flipX)
                {
                    p1 = editor.GridPointToScreen(from * mirrorX);
                    p2 = editor.GridPointToScreen(to * mirrorX);
                    GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y, beginColor, endColor);
                }

                if (flipY)
                {
                    p1 = editor.GridPointToScreen(from * mirrorY);
                    p2 = editor.GridPointToScreen(to * mirrorY);
                    GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y, beginColor, endColor);
                }
            }
        }

        /// <summary>
        /// <see cref="GLUtilities.DrawLine"/> in grid coordinates that takes shape symmetry into account.
        /// </summary>
        private void DrawGridLine(float thickness, float2 from, float2 to, Color color)
        {
            var p1 = editor.GridPointToScreen(from);
            var p2 = editor.GridPointToScreen(to);
            GL.Color(color);
            GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y);

            var symmetry = segment.shape.symmetryAxes;
            if (symmetry != SimpleGlobalAxis.None)
            {
                bool flipX = symmetry.HasFlag(SimpleGlobalAxis.Horizontal);
                bool flipY = symmetry.HasFlag(SimpleGlobalAxis.Vertical);

                color.a = 0.75f;
                GL.Color(color);

                if (flipX && flipY)
                {
                    p1 = editor.GridPointToScreen(from * mirrorXY);
                    p2 = editor.GridPointToScreen(to * mirrorXY);
                    GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y);
                }

                if (flipX)
                {
                    p1 = editor.GridPointToScreen(from * mirrorX);
                    p2 = editor.GridPointToScreen(to * mirrorX);
                    GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y);
                }

                if (flipY)
                {
                    p1 = editor.GridPointToScreen(from * mirrorY);
                    p2 = editor.GridPointToScreen(to * mirrorY);
                    GLUtilities.DrawLine(thickness, p1.x, p1.y, p2.x, p2.y);
                }
            }
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