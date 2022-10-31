#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class DrawTool : Tool
    {
        /// <summary>This state lets the user begin free-drawing a new shape.</summary>
        private class FreeDrawState : State
        {
            public override bool IsBusy()
            {
                // this state is always busy.
                return true;
            }

            public override void OnRender()
            {
                var mouseSnappedPosition = tool.mouseSnappedPosition;

                GLUtilities.DrawGui(() =>
                {
                    // mouse crosshair in grid space.
                    GL.Color(drawIndicatorColor);
                    GLUtilities.DrawLine(1.0f, mouseSnappedPosition - new float2(5f, 0f), mouseSnappedPosition + new float2(5f, 0f));
                    GLUtilities.DrawLine(1.0f, mouseSnappedPosition - new float2(0f, 5f), mouseSnappedPosition + new float2(0f, 5f));
                });
            }

            public override void OnMouseDown(int button)
            {
                if (button == 0)
                {
                    var mouseSnappedGridPosition = tool.mouseSnappedGridPosition;

                    // push a copy of the project to the undo stack.
                    editor.RegisterUndo("Draw Shape");

                    var shape = new Shape();
                    shape.segments.Clear();
                    editor.project.shapes.Add(shape);

                    shape.AddSegment(tool.selectedSegment1 = new Segment(shape, mouseSnappedGridPosition));
                    shape.AddSegment(tool.selectedSegment2 = new Segment(shape, mouseSnappedGridPosition));

                    tool.GotoState(new FreeDrawEdgeState());
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
                        return true;
                }
                return false;
            }
        }
    }
}

#endif