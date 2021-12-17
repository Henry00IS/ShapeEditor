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

        public ToolbarGuiWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            AddControl(selectBoxButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, new float2(0, 0), new float2(28, 28), () =>
            {
                parent.SwitchToBoxSelectTool();
            }));

            AddControl(translateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorTranslate, new float2(0, 28), new float2(28, 28), () =>
            {
                parent.SwitchToTranslateTool();
            }));

            AddControl(rotateButton = new GuiButton(ShapeEditorResources.Instance.shapeEditorRotate, new float2(0, 56), new float2(28, 28), () =>
            {
                parent.SwitchToRotateTool();
            }));
        }

        public override void OnRender()
        {
            Type type = parent.activeTool.GetType();
            selectBoxButton.isChecked = type == typeof(BoxSelectTool);
            translateButton.isChecked = type == typeof(TranslateTool);
            rotateButton.isChecked = type == typeof(RotateTool);

            base.OnRender();
        }
    }
}

#endif