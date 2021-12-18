#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public class RotateTool : BoxSelectTool
    {
        private RotationWidget rotationWidget = new RotationWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            editor.AddWidget(rotationWidget);
        }

        public override void OnRender()
        {
            base.OnRender();

            if (editor.selectedSegmentsCount > 0)
            {
                rotationWidget.position = editor.selectedSegmentsAveragePosition;
                rotationWidget.visible = true;
            }
            else
            {
                rotationWidget.visible = false;
            }
        }
    }
}

#endif