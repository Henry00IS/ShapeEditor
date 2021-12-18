#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : BoxSelectTool
    {
        private TranslationWidget translationWidget = new TranslationWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            editor.AddWidget(translationWidget);

            translationWidget.onMouseDrag = (screenDelta, gridDelta) => CommonAction_TranslateSelectedSegmentsByGridDelta(gridDelta);
        }

        public override void OnRender()
        {
            base.OnRender();

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
}

#endif