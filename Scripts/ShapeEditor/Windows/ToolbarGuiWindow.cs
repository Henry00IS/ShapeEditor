#if UNITY_EDITOR

using System;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class ToolbarGuiWindow : GuiWindow
    {
        private GuiButton selectBoxButton;
        private GuiButton translateButton;
        private GuiButton rotateButton;
        private GuiButton scaleButton;
        private GuiButton cutButton;

        public ToolbarGuiWindow(float2 position) : base(position, float2.zero) { }

        private GuiVerticalLayout verticalLayout;

        public override void OnActivate()
        {
            verticalLayout = new GuiVerticalLayout(this);

            verticalLayout.AddControl(selectBoxButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, 28, () =>
            {
                editor.SwitchToBoxSelectTool();
            }));

            verticalLayout.AddControl(translateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorTranslate, 28, () =>
            {
                editor.SwitchToTranslateTool();
            }));

            verticalLayout.AddControl(rotateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorRotate, 28, () =>
            {
                editor.SwitchToRotateTool();
            }));

            verticalLayout.AddControl(scaleButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorScale, 28, () =>
            {
                editor.SwitchToScaleTool();
            }));

            verticalLayout.AddControl(cutButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorCut, 28, () =>
            {
                editor.SwitchToCutTool();
            }));

            size = verticalLayout.windowSize;
        }

        public override void OnRender()
        {
            Type type = editor.activeTool.GetType();
            selectBoxButton.isChecked = type == typeof(BoxSelectTool);
            translateButton.isChecked = type == typeof(TranslateTool);
            rotateButton.isChecked = type == typeof(RotateTool);
            scaleButton.isChecked = type == typeof(ScaleTool);
            cutButton.isChecked = type == typeof(CutTool);

            base.OnRender();
        }
    }
}

#endif