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
            base.OnActivate();

            verticalLayout = new GuiVerticalLayout(this);

            verticalLayout.Add(selectBoxButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, 28, editor.UserSwitchToBoxSelectTool) { tooltip = "Box Select Tool" });
            verticalLayout.Add(translateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorTranslate, 28, editor.UserSwitchToTranslateTool) { tooltip = "Translate Tool" });
            verticalLayout.Add(rotateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorRotate, 28, editor.UserSwitchToRotateTool) { tooltip = "Rotate Tool" });
            verticalLayout.Add(scaleButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorScale, 28, editor.UserSwitchToScaleTool) { tooltip = "Scale Tool" });
            verticalLayout.Add(cutButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorCut, 28, editor.UserSwitchToCutTool) { tooltip = "Cut Tool" });

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