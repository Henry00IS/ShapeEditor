#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public class ScaleTool : BoxSelectTool
    {
        private ScaleWidget scaleWidget = new ScaleWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            editor.AddWidget(scaleWidget);

            scaleWidget.onBeginScaling = () =>
            {
                // store the initial position of all selected segments.
                foreach (var segment in editor.ForEachSelectedSegment())
                    segment.scaleToolInitialPosition = segment.position;
            };

            scaleWidget.onMouseDrag = (pivot, scale) =>
            {
                // scale the selected segments using their initial position.
                foreach (var segment in editor.ForEachSelectedSegment())
                {
                    segment.position = MathEx.ScaleAroundPivot(segment.scaleToolInitialPosition, pivot, scale);
                }
            };
        }

        public override void OnRender()
        {
            base.OnRender();

            if (editor.selectedSegmentsCount > 0)
            {
                scaleWidget.position = editor.selectedSegmentsAveragePosition;
                scaleWidget.visible = true;
            }
            else
            {
                scaleWidget.visible = false;
            }
        }
    }
}

#endif