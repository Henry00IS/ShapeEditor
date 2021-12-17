#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a button control inside of a window.</summary>
    public class GuiButton : GuiControl
    {
        public Texture2D icon;

        private static readonly Color colorButtonBackground = new Color(0.192f, 0.192f, 0.192f, 0.5f);
        private static readonly Color colorButtonBorder = new Color(0.1f, 0.1f, 0.1f);

        public GuiButton(Texture2D icon, float2 position, float2 size) : base(position, size)
        {
            this.icon = icon;
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            // draw the button outline.
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawTransparentRectangleWithOutline(drawPosition.x, drawPosition.y, size.x, size.y, colorButtonBackground, colorButtonBorder);
            });

            // draw the button icon.
            GLUtilities.DrawGuiTextured(icon, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawPosition.x + (size.x / 2f) - (icon.width / 2f), drawPosition.y + (size.y / 2f) - (icon.height / 2f), icon.width, icon.height);
            });
        }
    }
}

#endif