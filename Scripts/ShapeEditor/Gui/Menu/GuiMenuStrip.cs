#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a menu strip inside of a window.</summary>
    public class GuiMenuStrip : GuiControl
    {
        private const float height = 20f;
        private static readonly Color colorMenuBackground = new Color(0.17f, 0.17f, 0.17f);

        /// <summary>Horizontal layout helper to place the menu items in a row.</summary>
        private GuiHorizontalLayout horizontalLayout;
        /// <summary>The menu window used when a menu item is open.</summary>
        private GuiMenuWindow activeMenuWindow;

        public GuiMenuStrip() : base(1f, height)
        {
            horizontalLayout = new GuiHorizontalLayout(this, 0, 0);
        }

        /// <summary>Adds a horizontal menu item to the menu strip.</summary>
        /// <param name="text">The caption of the menu item.</param>
        /// <returns>The horizontal menu item that has been created.</returns>
        public GuiMenuHorizontalItem Add(string text)
        {
            var item = new GuiMenuHorizontalItem(text);
            horizontalLayout.Add(item);
            item.onClick += () => OnMenuItemClicked(item);
            return item;
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            // the menu strip is always as wide as the parent container.
            size = new float2(parent.size.x - 2f, height);

            // draw the menu bar background.
            GLUtilities.DrawGui(() =>
            {
                GL.Color(colorMenuBackground);
                GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, size.y);
            });

            // draw the horizontal menu items.
            base.OnRender();

            // if the mouse is hovering over the menu bar that has an open menu:
            if (isMouseOver && !isMouseObstructed && IsMenuOpen())
            {
                // switch to a different menu if we are hovering over a different item.
                var item = FindItemAtPosition(editor.mousePosition);
                if (item != null)
                    OpenMenu(item);
            }
        }

        /// <summary>Finds a menu item at the specified position.</summary>
        /// <param name="position">The screen position to find a menu item at.</param>
        /// <returns>The menu item if found else null.</returns>
        private GuiMenuItem FindItemAtPosition(float2 position)
        {
            if (FindAtPosition(position) is GuiMenuItem menuItem)
                return menuItem;
            return null;
        }

        /// <summary>Opens the specified menu item unless it's already open.</summary>
        /// <param name="item">The menu item to be opened.</param>
        private void OpenMenu(GuiMenuItem item)
        {
            if (IsMenuOpen(item)) return;

            CloseMenu();

            activeMenuWindow = new GuiMenuWindow(item);
            activeMenuWindow.Open();
        }

        /// <summary>Closes the active menu.</summary>
        private void CloseMenu()
        {
            if (activeMenuWindow != null)
            {
                activeMenuWindow.Close();
                activeMenuWindow = null;
            }
        }

        /// <summary>Checks whether any menu item is open.</summary>
        /// <returns>True when a menu is open else false.</returns>
        private bool IsMenuOpen()
        {
            return activeMenuWindow != null && !activeMenuWindow.closed;
        }

        /// <summary>Checks whether the specified menu item is open.</summary>
        /// <returns>True when a menu for the specified item is open else false.</returns>
        private bool IsMenuOpen(GuiMenuItem item)
        {
            if (!IsMenuOpen()) return false;
            return activeMenuWindow.parentMenu == item;
        }

        /// <summary>Called whenever a menu item is clicked.</summary>
        /// <param name="item">The menu item that was clicked.</param>
        private void OnMenuItemClicked(GuiMenuItem item)
        {
            OpenMenu(item);
        }
    }
}

#endif