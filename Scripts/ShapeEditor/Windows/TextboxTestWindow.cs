#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class TextboxTestWindow : GuiWindow
    {
        private GuiTextbox textbox;

        public TextboxTestWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            AddControl(textbox = new GuiFloatTextbox(new float2(10f, 10f), new float2(200, 20))
            {
            });

            AddControl(textbox = new GuiTextbox(new float2(10f, 50f), new float2(200, 20), "Hello World"));
        }
    }
}

#endif