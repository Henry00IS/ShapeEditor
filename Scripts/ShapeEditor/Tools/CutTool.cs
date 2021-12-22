#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;
using FindSegmentLineResult = AeternumGames.ShapeEditor.ShapeEditorWindow.FindSegmentLineResult;

namespace AeternumGames.ShapeEditor
{
    public class CutTool : BoxSelectTool
    {
        private bool isSingleUseDone = false;
        private FindSegmentLineResult findSegmentLineResult;
        private float2 cutGridPosition;
        private bool cutFound;

        private static readonly Color cutIndicatorColor = new Color(1.0f, 0.5f, 0.0f);

        public override void OnRender()
        {
            base.OnRender();

            if (isSingleUse)
            {
                var mousePosition = editor.mousePosition;

                // snap to the grid when the control key is being held down.
                if (editor.isCtrlPressed)
                {
                    mousePosition = editor.GridPointToScreen(editor.mouseGridPosition.Snap(editor.gridSnap));
                }

                if (cutFound = editor.FindSegmentLineAtScreenPosition(mousePosition, 64f, ref findSegmentLineResult))
                {
                    var segmentScreenPosition1 = editor.GridPointToScreen(findSegmentLineResult.segment1.position);
                    var segmentScreenPosition2 = editor.GridPointToScreen(findSegmentLineResult.segment2.position);
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
                        editor.RegisterUndo("Cut Segment");

                        // clear the active selection.
                        editor.project.ClearSelection();

                        // insert a segment at the cut position.
                        var shape = findSegmentLineResult.shape;
                        var segment = new Segment(shape, cutGridPosition);
                        segment.selected = true;
                        shape.segments.Insert(findSegmentLineResult.segmentIndex2, segment);
                    }
                }
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    // we do not want the marquee in this mode.
                    return;
                }
            }

            base.OnMouseDrag(button, screenDelta, gridDelta);
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
            else
            {
                base.OnGlobalMouseUp(button);
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
    }
}

#endif