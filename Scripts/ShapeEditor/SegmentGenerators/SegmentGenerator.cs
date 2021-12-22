#if UNITY_EDITOR

using System.Collections.Generic;
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
        /// Creates a special segment generator, the following types are supported:
        /// <para><see cref="SegmentGeneratorType.Bezier"/></para>
        /// </summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The first segment (that has the modifier).</param>
        /// <param name="next">The next connected segment.</param>
        /// <param name="type">The segment generator type to create.</param>
        public SegmentGenerator(ShapeEditorWindow editor, Segment segment, Segment next, SegmentGeneratorType type)
        {
            this.type = type;

            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_Constructor(editor, segment, next);
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
                    {
                        foreach (var selectable in Bezier_ForEachSelectableObject())
                            yield return selectable;
                    }

                    yield break;
            }
        }

        /// <summary>Draws the modifier segments to the screen.</summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The first segment (that has the modifier).</param>
        /// <param name="next">The next connected segment.</param>
        public void DrawSegments(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    Linear_DrawSegments(editor, segment, next);
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_DrawSegments(editor, segment, next);
                    break;
            }
        }

        /// <summary>Draws the modifier pivots to the screen.</summary>
        /// <param name="editor">The shape editor window.</param>
        /// <param name="segment">The first segment (that has the modifier).</param>
        /// <param name="next">The next connected segment.</param>
        public void DrawPivots(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            switch (type)
            {
                case SegmentGeneratorType.Linear:
                    break;

                case SegmentGeneratorType.Bezier:
                    Bezier_DrawPivots(editor, segment, next);
                    break;
            }
        }
    }
}

#endif