#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a button control inside of a window.</summary>
    public class GuiButton : GuiControl
    {
        public Texture2D icon;
        public System.Action onClick;

        private static readonly Color colorButtonBackground = new Color(0.192f, 0.192f, 0.192f, 0.5f);
        private static readonly Color colorButtonBackgroundPressed = new Color(0.592f, 0.592f, 0.592f, 0.5f);
        private static readonly Color colorButtonBackgroundHover = new Color(0.292f, 0.292f, 0.292f, 0.5f);
        private static readonly Color colorButtonBorder = new Color(0.1f, 0.1f, 0.1f);
        private static readonly Color colorButtonActiveBorder = new Color(0.5f, 0.25f, 0.0f);

        private bool isPressed = false;

        public GuiButton(Texture2D icon, float2 position, float2 size, System.Action onClick) : base(position, size)
        {
            this.icon = icon;
            this.onClick = onClick;
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            // draw the button outline.
            GLUtilities.DrawGui(() =>
            {
                var backgroundColor = colorButtonBackground;
                if (isPressed)
                {
                    backgroundColor = colorButtonBackgroundPressed;
                }
                else if (isMouseOver)
                {
                    backgroundColor = colorButtonBackgroundHover;
                }
                GLUtilities.DrawTransparentRectangleWithOutline(drawPosition.x, drawPosition.y, size.x, size.y, backgroundColor, isActive ? colorButtonActiveBorder : colorButtonBorder);
            });

            // draw the button icon.
            GLUtilities.DrawGuiTextured(icon, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawPosition.x + (size.x / 2f) - (icon.width / 2f), drawPosition.y + (size.y / 2f) - (icon.height / 2f), icon.width, icon.height);
            });
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                isPressed = true;
            }
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                isPressed = false;

                if (isMouseOver && onClick != null)
                    onClick();
            }
        }
    }
}

#endif