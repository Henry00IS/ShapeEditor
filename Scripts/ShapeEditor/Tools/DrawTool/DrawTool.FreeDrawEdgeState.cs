#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class DrawTool : Tool
    {
        /// <summary>This state lets the user free-draw a new shape. Used by <see cref="FreeDrawState"/>.</summary>
        private class FreeDrawEdgeState : EdgeDrawState
        {
            public override void OnStateEnter()
            {
            }

            public override void OnStateExit()
            {
                // always remove the segment that's being moved by the mouse hover.
                DeleteSegmentBeforeSelectedSegment2();

                // detect invalid shapes (unfinished freedraw work).
                if (shape.segments.Count < 3)
                {
                    editor.project.shapes.Remove(shape);
                    editor.DiscardUndo();
                }
            }
        }
    }
}

#endif