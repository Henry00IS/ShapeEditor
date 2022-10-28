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
        /// <summary>The text to be displayed inside of the button when the icon is not set.</summary>
        public string text;
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

        public GuiButton(Texture2D icon, string text, float2 position, float2 size, System.Action onClick) : base(position, size)
        {
            this.icon = icon;
            this.text = text;
            this.onClick = onClick;
        }

        public GuiButton(Texture2D icon, string text, float2 size, System.Action onClick) : this(icon, text, float2.zero, size, onClick) { }

        public GuiButton(Texture2D icon, float2 position, float2 size, System.Action onClick) : this(icon, null, position, size, onClick) { }

        public GuiButton(Texture2D icon, float2 size, System.Action onClick) : this(icon, null, float2.zero, size, onClick) { }

        public GuiButton(string text, float2 position, float2 size, System.Action onClick) : this(null, text, position, size, onClick) { }

        public GuiButton(string text, float2 size, System.Action onClick) : this(null, text, float2.zero, size, onClick) { }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            // set the tooltip text when hovering over this control.
            if (isMouseHoverEffectApplicable)
                editor.SetTooltipText(onClick);

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

            // draw the button content.
            if (icon != null && text != null)
            {
                RenderButtonTextAndIcon();
            }
            else
            {
                if (icon == null)
                {
                    RenderButtonText();
                }
                else
                {
                    RenderButtonIcon();
                }
            }
        }

        /// <summary>Called when the button icon is rendered.</summary>
        private void RenderButtonIcon()
        {
            // draw the button icon.
            GLUtilities.DrawGuiTextured(icon, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawPosition.x + (size.x / 2f) - (icon.width / 2f), drawPosition.y + (size.y / 2f) - (icon.height / 2f), icon.width, icon.height);
            });
        }

        /// <summary>Called when the button text is rendered.</summary>
        private void RenderButtonText()
        {
            var textStringWidth = ShapeEditorResources.fontSegoeUI14.StringWidth(text);

            // draw the button text.
            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, text, math.floor(new float2(drawPosition.x + (size.x / 2f) - (textStringWidth / 2f), drawPosition.y + (size.y / 2f) - ShapeEditorResources.fontSegoeUI14.halfHeight)));
        }

        /// <summary>Called when the button icon and text are rendered.</summary>
        private void RenderButtonTextAndIcon()
        {
            // draw the button icon.
            GLUtilities.DrawGuiTextured(icon, () =>
            {
                GLUtilities.DrawFlippedUvRectangle(drawPosition.x + 1f, drawPosition.y + (size.y / 2f) - (icon.height / 2f), icon.width, icon.height);
            });

            // draw the button text.
            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, text, math.floor(new float2(drawPosition.x + icon.width + 2f, drawPosition.y + (size.y / 2f) - ShapeEditorResources.fontSegoeUI14.halfHeight)));
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
            if (button == 0 && isPressed)
            {
                isPressed = false;

                if (isMouseOver)
                    onClick?.Invoke();
            }
        }

        public override bool IsBusy()
        {
            return isPressed;
        }
    }
}

#endif