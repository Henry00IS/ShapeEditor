#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a button control inside of a window.</summary>
    public class GuiButton : GuiControl
    {
        /// <summary>The icon to be displayed inside of the button.</summary>
        public Texture2D icon;
        /// <summary>The action to be called when the button is clicked.</summary>
        public System.Action onClick;
        /// <summary>Whether this button appears as being checked (background color).</summary>
        public bool isChecked;

        private static readonly Color colorButtonBackground = new Color(0.192f, 0.192f, 0.192f, 0.5f);
        private static readonly Color colorButtonBackgroundPressed = new Color(0.592f, 0.592f, 0.592f, 0.5f);
        private static readonly Color colorButtonBackgroundHover = new Color(0.292f, 0.292f, 0.292f, 0.5f);
        private static readonly Color colorButtonBackgroundChecked = new Color(0.337f, 0.502f, 0.761f, 0.75f);
        private static readonly Color colorButtonBackgroundCheckedPressed = new Color(0.337f, 0.502f, 0.761f, 0.9f);
        private static readonly Color colorButtonBackgroundCheckedHover = new Color(0.337f, 0.502f, 0.761f, 0.8f);
        private static readonly Color colorButtonBorder = new Color(0.1f, 0.1f, 0.1f);
        private static readonly Color colorButtonActiveBorder = new Color(0.5f, 0.25f, 0.0f);

        private bool isPressed;

        public GuiButton(Texture2D icon, float2 position, float2 size, System.Action onClick) : base(position, size)
        {
            this.icon = icon;
            this.onClick = onClick;
        }

        public GuiButton(Texture2D icon, float2 size, System.Action onClick) : base(float2.zero, size)
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
                var backgroundColor = isChecked ? colorButtonBackgroundChecked : colorButtonBackground;
                if (isPressed)
                {
                    backgroundColor = isChecked ? colorButtonBackgroundCheckedPressed : colorButtonBackgroundPressed;
                }
                else if (isMouseHoverEffectApplicable)
                {
                    backgroundColor = isChecked ? colorButtonBackgroundCheckedHover : colorButtonBackgroundHover;
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