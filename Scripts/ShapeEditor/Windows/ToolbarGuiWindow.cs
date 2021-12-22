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

        public ToolbarGuiWindow(float2 position, float2 size) : base(position, size) { }

        public override void OnActivate()
        {
            AddControl(selectBoxButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, new float2(1, 1), new float2(28, 28), () =>
            {
                editor.SwitchToBoxSelectTool();
            }));

            AddControl(translateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorTranslate, new float2(1, 29), new float2(28, 28), () =>
            {
                editor.SwitchToTranslateTool();
            }));

            AddControl(rotateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorRotate, new float2(1, 57), new float2(28, 28), () =>
            {
                editor.SwitchToRotateTool();
            }));

            AddControl(scaleButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorScale, new float2(1, 85), new float2(28, 28), () =>
            {
                editor.SwitchToScaleTool();
            }));

            AddControl(cutButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorCut, new float2(1, 113), new float2(28, 28), () =>
            {
                editor.SwitchToCutTool();
            }));
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