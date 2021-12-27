#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A vertical menu item used in open menus.</summary>
    public class GuiMenuVerticalItem : GuiMenuItem
    {
        public GuiMenuVerticalItem(string text) : base(text)
        {
            size = new float2(GetWidth(), GetHeight());
        }

        public GuiMenuVerticalItem(string text, Texture icon) : base(text, icon)
        {
            size = new float2(GetWidth(), GetHeight());
        }

        /// <summary>Gets the width of the menu item in a vertical menu.</summary>
        /// <returns>The width of the menu item.</returns>
        public override int GetWidth()
        {
            var font = ShapeEditorResources.fontSegoeUI14;
            return font.StringWidth(text) + 14 + 20 + 7; // 7 padding on both sides with 20 image and just 7 more because it looks nice.
        }

        /// <summary>Gets the height of the menu item in a vertical menu.</summary>
        /// <returns>The height of the menu item.</returns>
        public override int GetHeight()
        {
            return 22;
        }

        public override void OnRender()
        {
            base.OnRender();

            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, text, drawPosition + new float2(27f, 4f));

            if (icon)
            {
                GLUtilities.DrawGuiTextured(icon, () =>
                {
                    GLUtilities.DrawFlippedUvRectangle(drawPosition.x + 4f, drawPosition.y + 2f, icon.width, icon.height);
                });
            }
        }
    }
}

#endif