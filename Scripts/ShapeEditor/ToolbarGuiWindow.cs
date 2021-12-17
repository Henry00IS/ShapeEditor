#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class ToolbarGuiWindow : GuiWindow
    {
        public ToolbarGuiWindow(float2 position, float2 size) : base(position, size)
        {
            AddControl(new GuiButton(ShapeEditorResources.Instance.shapeEditorSelectBox, new float2(0, 0), new float2(28, 28)));
        }
    }
}

#endif