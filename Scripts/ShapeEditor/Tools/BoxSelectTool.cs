#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class BoxSelectTool : Tool
    {
        private bool isMarqueeActive;
        private bool isMarqueeSubtractive;
        private static readonly Color marqueeColor = new Color(1.0f, 0.5f, 0.0f);
        private QuickTranslationWidget quickTranslationWidget = new QuickTranslationWidget();

        public override void OnActivate()
        {
            isMarqueeActive = false;

            editor.AddWidget(quickTranslationWidget);
            quickTranslationWidget.onMouseDrag = (screenDelta, gridDelta) => CommonAction_TranslateSelectedSegmentsByGridDelta(gridDelta);
        }

        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                if (isMarqueeActive)
                {
                    // draw the marquee selection if active.
                    var marqueeBegin = editor.GridPointToScreen(editor.mouseGridInitialPosition);
                    var marqueeEnd = editor.GridPointToScreen(editor.mouseGridPosition);
                    var marqueeRect = MathEx.RectXYXY(marqueeBegin, marqueeEnd);

                    GLUtilities.DrawRectangleOutline(marqueeRect.x, marqueeRect.y, marqueeRect.width, marqueeRect.height, marqueeColor);
                }
            });
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                // unless the shift key is held down we clear the selection.
                if (!editor.isShiftPressed && !isMarqueeSubtractive)
                    editor.project.ClearSelection();

                if (isMarqueeActive)
                {
                    // iterate over all segments within the marquee.
                    var marqueeRect = MathEx.RectXYXY(editor.mouseGridInitialPosition, editor.mouseGridPosition);
                    foreach (var segment in editor.ForEachSegmentInGridRect(marqueeRect))
                        segment.selected = !isMarqueeSubtractive;
                }
                else
                {
                    // find the closest segment to the click position.
                    var segment = editor.FindSegmentAtScreenPosition(editor.mousePosition, 60.0f);
                    if (segment != null)
                        segment.selected = !segment.selected;
                }

                isMarqueeActive = false;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 0)
            {
                // once the mouse moves a bit from the initial position the marquee activates.
                if (!isMarqueeActive)
                {
                    isMarqueeActive = (math.distance(editor.mouseInitialPosition, editor.mousePosition) > 3.0f);
                    isMarqueeSubtractive = editor.isCtrlPressed;
                }
            }
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.G:
                    quickTranslationWidget.Activate();
                    break;
            }
            return false;
        }
    }
}

#endif