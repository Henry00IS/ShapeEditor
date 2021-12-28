#if UNITY_EDITOR

using System;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A horizontal menu item used in menu bars.</summary>
    public class GuiMenuHorizontalItem : GuiMenuItem
    {
        public GuiMenuHorizontalItem(string text, Action onClick = null) : base(text, onClick)
        {
            size = new float2(GetWidth(), GetHeight());
        }

        /// <summary>Gets the height of the menu item in a horizontal menu.</summary>
        /// <returns>The height of the menu item.</returns>
        public override int GetHeight()
        {
            return 20;
        }

        /// <summary>Gets the width of the menu item in a horizontal menu.</summary>
        /// <returns>The width of the menu item.</returns>
        public override int GetWidth()
        {
            var font = ShapeEditorResources.fontSegoeUI14;
            return font.StringWidth(text) + 14; // 7 padding on both sides.
        }

        public override void OnRender()
        {
            // if the menu is open we darken the background of this item.
            if (parent is GuiMenuStrip menuStrip && menuStrip.IsMenuOpen(this))
            {
                var rect = drawRect;
                GLUtilities.DrawGui(() =>
                {
                    GL.Color(GuiMenuWindow.colorMenuBackground);
                    GLUtilities.DrawRectangle(rect.x, rect.y, rect.width, rect.height);
                });
            }
            else
            {
                base.OnRender();
            }

            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, text, drawPosition + new float2(7f, 3f));
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                onClick?.Invoke();
            }
        }
    }
}

#endif