#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;
using FindSegmentLineResult = AeternumGames.ShapeEditor.ShapeEditorWindow.FindSegmentLineResult;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a keyboard shortcut cut widget.</summary>
    public class QuickCutWidget : Widget
    {
        private bool _wantsActive;
        private bool isDone;

        private FindSegmentLineResult findSegmentLineResult;
        private float2 cutGridPosition;
        private bool cutFound;

        private static readonly Color cutIndicatorColor = new Color(1.0f, 0.5f, 0.0f);

        public override bool wantsActive => _wantsActive;

        /// <summary>Activates this keyboard shortcut widget.</summary>
        public void Activate()
        {
            editor.activeWidget = this;
            _wantsActive = true;
            isDone = false;
        }

        public override void OnRender()
        {
            if (!isActive || isDone) return;

            if (cutFound = editor.FindSegmentLineAtScreenPosition(editor.mousePosition, 64f, ref findSegmentLineResult))
            {
                var segmentScreenPosition1 = editor.GridPointToScreen(findSegmentLineResult.segment1.position);
                var segmentScreenPosition2 = editor.GridPointToScreen(findSegmentLineResult.segment2.position);
                var cutScreenPosition = MathEx.FindNearestPointOnLine(editor.mousePosition, segmentScreenPosition1, segmentScreenPosition2);
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
            if (!isActive || isDone) return;

            if (button == 0)
            {
                isDone = true;

                if (cutFound)
                {
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

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                _wantsActive = false;
            }
        }
    }
}

#endif