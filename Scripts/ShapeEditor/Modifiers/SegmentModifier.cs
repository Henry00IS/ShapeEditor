#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A segment modifier that generates additional points between two segments.</summary>
    [System.Serializable]
    public partial class SegmentModifier
    {
        // this has been written as partial classes because unity's serialization system did not
        // play along with polymorphism.

        [SerializeField]
        public SegmentModifierType type = SegmentModifierType.Nothing;

        public SegmentModifier()
        {
            type = SegmentModifierType.Nothing;
        }

        public SegmentModifier(ShapeEditorWindow editor, Segment segment, Segment next, SegmentModifierType type)
        {
            this.type = type;

            switch (type)
            {
                case SegmentModifierType.Nothing:
                    break;

                case SegmentModifierType.Bezier:
                    Bezier_Constructor(editor, segment, next);
                    break;
            }
        }

        /// <summary>Iterates over all of the selectable objects.</summary>
        public IEnumerable<ISelectable> ForEachSelectableObject()
        {
            switch (type)
            {
                case SegmentModifierType.Nothing:
                    yield break;

                case SegmentModifierType.Bezier:
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
                case SegmentModifierType.Nothing:
                    break;

                case SegmentModifierType.Bezier:
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
                case SegmentModifierType.Nothing:
                    break;

                case SegmentModifierType.Bezier:
                    Bezier_DrawPivots(editor, segment, next);
                    break;
            }
        }
    }
}

#endif