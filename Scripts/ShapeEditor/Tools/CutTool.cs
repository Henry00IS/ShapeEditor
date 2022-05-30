#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class CutTool : Tool
    {
        private bool isSingleUseDone = false;
        private Segment findSegmentLineResult;
        private float2 cutGridPosition;
        private bool cutFound;

        private static readonly Color cutIndicatorColor = new Color(1.0f, 0.5f, 0.0f);

        public override void OnRender()
        {
            var mousePosition = editor.mousePosition;

            // optionally snap to the grid.
            if (editor.isSnapping)
            {
                mousePosition = editor.GridPointToScreen(editor.mouseGridPosition.Snap(editor.gridSnap));
            }

            findSegmentLineResult = editor.FindSegmentLineAtScreenPosition(mousePosition, 64f);
            cutFound = findSegmentLineResult != null;

            if (cutFound)
            {
                var segmentScreenPosition1 = editor.GridPointToScreen(findSegmentLineResult.position);
                var segmentScreenPosition2 = editor.GridPointToScreen(findSegmentLineResult.next.position);
                var cutScreenPosition = MathEx.FindNearestPointOnLine(mousePosition, segmentScreenPosition1, segmentScreenPosition2);
                cutGridPosition = editor.ScreenPointToGrid(cutScreenPosition);

                GLUtilities.DrawGui(() =>
                {
                    var normal = math.normalize(segmentScreenPosition2 - segmentScreenPosition1);
                    var cross = (float2)Vector2.Perpendicular(normal);
                    var top = cutScreenPosition - cross * 10f;
                    var bottom = cutScreenPosition + cross * 10f;

                    GL.Color(cutIndicatorColor);
                    GLUtilities.DrawLine(1.0f, top, bottom);
                    GLUtilities.DrawLine(1.0f, top - normal * 4f, top + (normal * 4f));
                    GLUtilities.DrawLine(1.0f, bottom - normal * 4f, bottom + (normal * 4f));
                });
            }

            editor.SetMouseCursor(ShapeEditorResources.Instance.shapeEditorMouseCursorScissors);
        }

        public override void OnMouseDown(int button)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    isSingleUseDone = true;

                    if (cutFound)
                    {
                        ToolOnCutSegment();
                    }
                }
            }
            else
            {
                if (button == 0)
                {
                    if (cutFound)
                    {
                        ToolOnCutSegment();
                    }
                }
            }
        }

        private void ToolOnCutSegment()
        {
            editor.RegisterUndo("Cut Segment");

            // clear the active selection.
            editor.project.ClearSelection();

            // insert a segment at the cut position.
            var shape = findSegmentLineResult.shape;
            var segment = new Segment(shape, cutGridPosition);
            segment.selected = true;
            shape.InsertSegmentBefore(findSegmentLineResult.next, segment);
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    editor.SwitchTool(parent);
                }
            }
        }

        public override bool IsBusy()
        {
            if (isSingleUse)
            {
                return !isSingleUseDone;
            }
            return false;
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            // these single-use tools are not available while in single-use mode.
            if (isSingleUse) return false;

            switch (keyCode)
            {
                case KeyCode.G:
                    if (editor.selectedSegmentsCount > 0)
                    {
                        editor.UseTool(new TranslateTool());
                        return true;
                    }
                    return false;
            }
            return false;
        }
    }
}

#endif