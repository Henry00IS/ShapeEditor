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
        private static readonly Color colorMenuHoverBackground = new Color(0.243f, 0.243f, 0.251f);

        /// <summary>The currently open menu window.</summary>
        private GuiMenuWindow guiMenuWindow;

        /// <summary>Horizontal layout helper to place the menu items in a row.</summary>
        private GuiHorizontalLayout horizontalLayout;

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
            horizontalLayout.AddControl(item);
            return item;
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            size = new float2(parent.size.x - 2f, height);

            // draw the menu bar background.
            GLUtilities.DrawGui(() =>
            {
                GL.Color(colorMenuBackground);
                GLUtilities.DrawRectangle(drawPosition.x, drawPosition.y, size.x, size.y);
            });

            // draw the horizontal menu items.
            base.OnRender();

            // if the mouse is hovering over the menu bar:
            if (isMouseOver)
            {
                // and a menu is currently open:
                if (IsMenuOpen())
                {
                    // and the mouse is hovering over a different horizontal menu item:
                    var openMenuItemIndex = GetOpenMenuIndex();
                    var index = FindMenuItemAt(parent.editor.mousePosition, out var rect);
                    if (index != -1 && index != openMenuItemIndex)
                    {
                        // close the old menu window.
                        guiMenuWindow.Close();

                        // open a new menu window.
                        guiMenuWindow = new GuiMenuWindow(new float2(rect.x, rect.yMax), (GuiMenuItem)children[index]);
                        parent.editor.OpenWindow(guiMenuWindow);
                        parent.editor.TrySwitchActiveEventReceiver(guiMenuWindow);
                    }
                }
            }
        }

        /// <summary>Finds a child menu item index at the specified point or else returns -1.</summary>
        /// <param name="point">The screen point to find a menu item at.</param>
        /// <param name="rect">The draw rectangle for the child menu if found.</param>
        /// <returns>The index if found else -1.</returns>
        private int FindMenuItemAt(float2 point, out Rect rect)
        {
            rect = default;
            var font = ShapeEditorResources.fontSegoeUI14;
            var xpos = 7f;
            var menuItemsCount = children.Count;
            for (int i = 0; i < menuItemsCount; i++)
            {
                var menuItem = (GuiMenuItem)children[i];
                var width = font.StringWidth(menuItem.text) + 14f;
                if ((rect = new Rect(new Vector2(drawPosition.x + xpos - 7f, drawPosition.y), new Vector2(width, height))).Contains(point))
                    return i;
                xpos += width;
            }
            return -1;
        }

        /// <summary>Returns whether the menu is currently open.</summary>
        /// <returns>True when the menu is open else false.</returns>
        private bool IsMenuOpen()
        {
            return guiMenuWindow != null && guiMenuWindow.isActive;
        }

        /// <summary>Returns the menu item index of the open menu or else -1.</summary>
        /// <returns>The menu item index or -1 if not found or not open.</returns>
        private int GetOpenMenuIndex()
        {
            if (IsMenuOpen())
                return children.IndexOf(guiMenuWindow.parentMenu);
            return -1;
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                var index = FindMenuItemAt(parent.editor.mousePosition, out var rect);

                // open the desired menu item.
                if (index != -1)
                {
                    guiMenuWindow = new GuiMenuWindow(new float2(rect.x, rect.yMax), (GuiMenuItem)children[index]);
                    parent.editor.OpenWindow(guiMenuWindow);
                    parent.editor.TrySwitchActiveEventReceiver(guiMenuWindow);
                }
                // close it when the menu bar is clicked elsewhere.
                else if (IsMenuOpen())
                {
                    guiMenuWindow.Close();
                }
            }
        }
    }
}

#endif