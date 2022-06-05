#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a label control inside of a window.</summary>
    public class GuiLabel : GuiControl
    {
        /// <summary>The text to be displayed inside of the label.</summary>
        public string text;

        public GuiLabel(string text, float2 position, float2 size) : base(position, size)
        {
            this.text = text;
        }

        public GuiLabel(string text, float2 size) : base(float2.zero, size)
        {
            this.text = text;
        }

        public GuiLabel(string text) : base(float2.zero, new float2(20f, 14f))
        {
            this.text = text;
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            // draw the text.
            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, text, drawPosition);
        }
    }
}

#endif