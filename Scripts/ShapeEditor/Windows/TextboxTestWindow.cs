#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class TextboxTestWindow : GuiWindow
    {
        private GuiTextbox textbox;

        public TextboxTestWindow(ShapeEditorWindow parent, float2 position, float2 size) : base(parent, position, size)
        {
            AddControl(textbox = new GuiTextbox(new float2(10f, 10f), new float2(200, 20))
            {
                placeholder = "Username",
            });

            AddControl(textbox = new GuiTextbox(new float2(10f, 30f), new float2(200, 20))
            {
                placeholder = "Password",
                isPassword = true,
            });

            AddControl(textbox = new GuiTextbox(new float2(10f, 50f), new float2(200, 20))
            {
                text = "Something You cannot edit",
                placeholder = "Read Only",
                isReadonly = true,
            });
        }
    }
}

#endif