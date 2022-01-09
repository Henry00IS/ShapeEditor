#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    public class TextboxTestWindow : GuiWindow
    {
        private GuiTextbox textbox;

        public TextboxTestWindow(float2 position, float2 size) : base(position, size) { }

        public override void OnActivate()
        {
            base.OnActivate();

            Add(new GuiWindowTitle("Textbox Test Window"));

            Add(textbox = new GuiFloatTextbox(new float2(10f, 25f), new float2(200, 20))
            {
            });

            Add(textbox = new GuiTextbox(new float2(10f, 50f), new float2(200, 20), "Hello World"));
        }
    }
}

#endif