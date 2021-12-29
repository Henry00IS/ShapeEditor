#if UNITY_EDITOR

using System;
using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class ToolbarWindow : GuiWindow
    {
        private GuiButton selectBoxButton;
        private GuiButton translateButton;
        private GuiButton rotateButton;
        private GuiButton scaleButton;
        private GuiButton cutButton;

        public ToolbarWindow(float2 position) : base(position, float2.zero) { }

        private GuiVerticalLayout verticalLayout;

        public override void OnActivate()
        {
            verticalLayout = new GuiVerticalLayout(this);

            verticalLayout.Add(selectBoxButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, 28, editor.UserSwitchToBoxSelectTool));
            verticalLayout.Add(translateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorTranslate, 28, editor.UserSwitchToTranslateTool));
            verticalLayout.Add(rotateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorRotate, 28, editor.UserSwitchToRotateTool));
            verticalLayout.Add(scaleButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorScale, 28, editor.UserSwitchToScaleTool));
            verticalLayout.Add(cutButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorCut, 28, editor.UserSwitchToCutTool));

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