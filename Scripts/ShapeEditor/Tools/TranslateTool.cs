#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : BoxSelectTool
    {
        private bool isSingleUseDone = false;
        private TranslationWidget translationWidget = new TranslationWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            if (isSingleUse)
            {
                ToolOnBeginTranslating(editor);
            }
            else
            {
                editor.AddWidget(translationWidget);
                translationWidget.onBeginTranslating = () => ToolOnBeginTranslating(editor);
                translationWidget.onMouseDrag = (screenDelta, gridDelta) => ToolOnMouseDrag(editor, gridDelta);
            }
        }

        public override void OnRender()
        {
            base.OnRender();

            if (isSingleUse)
            {
                editor.SetMouseCursor(MouseCursor.MoveArrow);
            }
            else
            {
                if (editor.selectedSegmentsCount > 0)
                {
                    translationWidget.position = editor.selectedSegmentsAveragePosition;
                    translationWidget.visible = true;
                }
                else
                {
                    translationWidget.visible = false;
                }
            }
        }

        public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            if (isSingleUse && !isSingleUseDone)
            {
                ToolOnMouseDrag(editor, gridDelta);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    isSingleUseDone = true;
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

        private float2 deltaAccumulator;

        private void ToolOnBeginTranslating(ShapeEditorWindow editor)
        {
            editor.RegisterUndo("Translate Selection");

            deltaAccumulator = float2.zero;

            // store the initial position of all selected segments.
            foreach (var segment in editor.ForEachSelectedSegment())
                segment.gpVector1 = segment.position;
        }

        private void ToolOnMouseDrag(ShapeEditorWindow editor, float2 gridDelta)
        {
            deltaAccumulator += gridDelta;
            float2 position = deltaAccumulator;

            foreach (var segment in editor.ForEachSelectedSegment())
            {
                // snap to the grid when the control key is being held down.
                if (editor.isCtrlPressed)
                    position = position.Snap(editor.gridSnap);

                segment.position = segment.gpVector1 + position;
            }
        }
    }
}

#endif