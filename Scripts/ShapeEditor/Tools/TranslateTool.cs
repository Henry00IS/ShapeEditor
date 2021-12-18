#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public class TranslateTool : Tool
    {
        private TranslationWidget translationWidget = new TranslationWidget();

        public override void OnActivate()
        {
            editor.AddWidget(translationWidget);

            translationWidget.onMouseDrag = (screenDelta, gridDelta) =>
            {
                foreach (var segment in editor.ForEachSelectedSegment())
                    segment.position += gridDelta;
            };
        }

        public override void OnRender()
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
}

#endif