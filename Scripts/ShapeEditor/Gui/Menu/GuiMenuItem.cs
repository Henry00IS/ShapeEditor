#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a menu item inside of a menu.</summary>
    public abstract class GuiMenuItem : GuiControl
    {
        private const float height = 20f;
        private static readonly Color colorMenuItemHoverBackground = new Color(0.243f, 0.243f, 0.251f);

        /// <summary>Whether the menu item is enabled or disabled if supported.</summary>
        public bool enabled = true;

        /// <summary>The icon of the menu item if supported.</summary>
        public Texture icon;

        /// <summary>The caption of the menu item.</summary>
        public string text = "";

        /// <summary>Called when the menu item is clicked.</summary>
        public Action onClick;

        /// <summary>Vertical layout helper to place the menu items in a column.</summary>
        private GuiVerticalLayout verticalLayout;

        /// <summary>Initializes a new instance with the specified caption for the menu item.</summary>
        /// <param name="text">The caption for the menu item.</param>
        /// <param name="onClick">Called when the menu item is clicked.</param>
        protected GuiMenuItem(string text, Action onClick = null) : base(1f, height)
        {
            this.text = text;
            this.onClick = onClick;
            verticalLayout = new GuiVerticalLayout(this);
        }

        /// <summary>Initializes a new instance with the specified caption and icon for the menu item.</summary>
        /// <param name="text">The caption for the menu item.</param>
        /// <param name="icon">The icon of the menu item.</param>
        /// <param name="onClick">Called when the menu item is clicked.</param>
        protected GuiMenuItem(string text, Texture icon, Action onClick = null) : base(1f, height)
        {
            this.text = text;
            this.icon = icon;
            this.onClick = onClick;
            verticalLayout = new GuiVerticalLayout(this);
        }

        /// <summary>Adds a vertical menu item to the menu strip.</summary>
        /// <param name="text">The caption of the menu item.</param>
        /// <param name="onClick">Called when the menu item is clicked.</param>
        /// <returns>The vertical menu item that has been created.</returns>
        public GuiMenuVerticalItem Add(string text, Action onClick)
        {
            var item = new GuiMenuVerticalItem(text, onClick);
            verticalLayout.Add(item);
            return item;
        }

        /// <summary>Adds a vertical menu item to the menu strip.</summary>
        /// <param name="text">The caption of the menu item.</param>
        /// <param name="icon">The icon of the menu item.</param>
        /// <param name="onClick">Called when the menu item is clicked.</param>
        /// <returns>The vertical menu item that has been created.</returns>
        public GuiMenuVerticalItem Add(string text, Texture icon, Action onClick)
        {
            var item = new GuiMenuVerticalItem(text, icon, onClick);
            verticalLayout.Add(item);
            return item;
        }

        /// <summary>Adds a vertical menu separator to the menu strip.</summary>
        /// <returns>The vertical menu separator that has been created.</returns>
        public GuiMenuSeparator Separator()
        {
            var item = new GuiMenuSeparator();
            verticalLayout.Add(item);
            return item;
        }

        /// <summary>Gets the width of a menu item.</summary>
        /// <returns>The width of the menu item.</returns>
        public abstract int GetWidth();

        /// <summary>Gets the height of a menu item.</summary>
        /// <returns>The height of the menu item.</returns>
        public abstract int GetHeight();

        /// <summary>Gets the largest width of the child menu items in a vertical menu.</summary>
        /// <returns>The largest width of the child menu items.</returns>
        public int GetLargestWidthOfChildren()
        {
            var largest = 0;
            var menuItemsCount = children.Count;
            for (int i = 0; i < menuItemsCount; i++)
            {
                var menuItem = (GuiMenuItem)children[i];
                var width = menuItem.GetWidth();
                if (width > largest)
                    largest = width;
            }
            return largest;
        }

        /// <summary>Gets the vertical menu height by adding all child menu item heights together.</summary>
        /// <returns>The height of the menu to fit all children.</returns>
        public int GetMenuHeight()
        {
            int height = 0;
            var menuItemsCount = children.Count;
            for (int i = 0; i < menuItemsCount; i++)
                height += Mathf.RoundToInt(children[i].size.y);
            return height;
        }

        public override void OnRender()
        {
            if (isMouseOver)
            {
                editor.SetTooltipText(onClick, InstructionsDisplayMode.Menu);

                GLUtilities.DrawGui(() =>
                {
                    GL.Color(colorMenuItemHoverBackground);
                    GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, size.y);
                });
            }
        }
    }
}

#endif