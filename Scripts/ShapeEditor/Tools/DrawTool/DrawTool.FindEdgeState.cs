#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class DrawTool : Tool
    {
        /// <summary>This state lets the user pick an edge for drawing.</summary>
        private class FindEdgeState : State
        {
            public override void OnRender()
            {
                // try to find a segment at the mouse position.
                tool.selectedSegment1 = editor.FindSegmentLineAtScreenPosition(editor.mousePosition, 64f);
                tool.selectedSegment2 = null;

                // if a segment was found:
                if (tool.selectedSegment1 != null)
                {
                    tool.selectedSegment2 = tool.selectedSegment1.next;

                    // draw over the segment to indicate that it will be edited.
                    GLUtilities.DrawGui(() =>
                    {
                        var current = editor.GridPointToScreen(tool.selectedSegment1.position);
                        var next = editor.GridPointToScreen(tool.selectedSegment2.position);

                        // erase the line with the grid background color.
                        GL.Color(ShapeEditorWindow.gridBackgroundColor);
                        GLUtilities.DrawLine(1.0f, current, next);

                        // draw the erased line.
                        GL.Color(drawIndicatorColor);
                        GLUtilities.DrawDottedLine(1.0f, current, next);
                    });
                }
            }

            public override void OnMouseDown(int button)
            {
                // must have a selected segment at the mouse position.
                if (tool.selectedSegment1 == null) return;

                if (button == 0)
                {
                    // we are ready to draw on the selected segment.
                    tool.GotoState(new EdgeDrawState());
                }
            }
        }
    }
}

#endif