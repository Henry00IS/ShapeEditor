#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class ToolbarGuiWindow : GuiWindow
    {
        public ToolbarGuiWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, new float2(0, 0), new float2(28, 28)));
            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorTranslate, new float2(0, 28), new float2(28, 28)));
            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorRotate, new float2(0, 56), new float2(28, 28)));
        }
    }
}

#endif