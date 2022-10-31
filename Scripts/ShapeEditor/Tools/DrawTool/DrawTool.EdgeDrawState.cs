#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class DrawTool : Tool
    {
        /// <summary>This state lets the user draw segments on an existing edge.</summary>
        private class EdgeDrawState : State
        {
            protected Shape shape => tool.selectedSegment1.shape;
            private int drawnSegmentsCount = 0;

            public override void OnStateEnter()
            {
                // push a copy of the project to the undo stack.
                editor.RegisterUndo("Draw Edge");

                // insert the first segment to be moved by the mouse cursor.
                InsertSegmentBeforeSelectedSegment2(tool.mouseSnappedGridPosition);
            }

            public override void OnStateExit()
            {
                // always remove the segment that's being moved by the mouse hover.
                DeleteSegmentBeforeSelectedSegment2();

                // if the edge is unchanged, discard the undo operation.
                if (drawnSegmentsCount == 0)
                {
                    editor.DiscardUndo();
                }
            }

            public override bool IsBusy()
            {
                // this state is always busy.
                return true;
            }

            public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
            {
                // move the next to last segment to the (optionally snapped) mouse position.
                tool.selectedSegment2.previous.position = tool.mouseSnappedGridPosition;
            }

            public override void OnMouseDown(int button)
            {
                if (button == 0)
                {
                    // insert another segment at the (optionally snapped) mouse position.
                    // this leaves the current segment at the last position, thus placing it.
                    InsertSegmentBeforeSelectedSegment2(tool.mouseSnappedGridPosition);
                }
            }

            public override bool OnKeyDown(KeyCode keyCode)
            {
                switch (keyCode)
                {
                    case KeyCode.Escape:
                        if (tool.isSingleUse)
                        {
                            ExitSingleUseTool();
                        }
                        else
                        {
                            tool.GotoState(new FindEdgeState());
                        }
                        return true;
                }
                return false;
            }

            public override void OnRender()
            {
                GLUtilities.DrawGui(() =>
                {
                    var current = editor.GridPointToScreen(tool.selectedSegment2.previous.position);
                    var last = editor.GridPointToScreen(tool.selectedSegment2.position);
                    var segmentCount = shape.segments.Count;

                    if (segmentCount > 2)
                    {
                        GL.Color(drawIndicatorColor);
                        GLUtilities.DrawLine(1.0f, current, last);
                    }
                });
            }

            protected void InsertSegmentBeforeSelectedSegment2(float2 position)
            {
                drawnSegmentsCount++;
                shape.InsertSegmentBefore(tool.selectedSegment2, new Segment(shape, position));
            }

            protected void DeleteSegmentBeforeSelectedSegment2()
            {
                drawnSegmentsCount--;
                shape.RemoveSegment(tool.selectedSegment2.previous);
            }
        }
    }
}

#endif