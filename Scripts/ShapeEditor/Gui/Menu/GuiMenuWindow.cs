#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents an opened menu that shows selectable menu items.</summary>
    public class GuiMenuWindow : GuiWindow
    {
        public static readonly Color colorMenuBackground = new Color(0.106f, 0.106f, 0.110f);

        /// <summary>The parent menu that has a collection of menu items.</summary>
        public readonly GuiMenuItem parentMenu;

        public GuiMenuWindow(GuiMenuItem item) : base(0f, 2f)
        {
            parentMenu = item;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            colorWindowBackground = colorMenuBackground;

            // calculate the menu position.
            position = new float2(parentMenu.drawRect.x, parentMenu.drawRect.yMax);

            // calculate the required menu size.
            size = new float2(parentMenu.GetLargestWidthOfChildren(), parentMenu.GetMenuHeight() + 2f);

            // add the menu items.
            foreach (var child in parentMenu.children)
            {
                child.size = new float2(size.x - 2f, child.size.y);
                Add(child);
            }
        }

        public override void OnRender()
        {
            if (!IsActiveOrHasActiveChild())
            {
                Close();
            }
            else
            {
                base.OnRender();
            }
        }

        public override void OnMouseUp(int button)
        {
            if (activeChild != null)
            {
                base.OnMouseUp(button);
            }
            else
            {
                // the menu items never got a mouse down event, none of them are active, so we
                // manually forward the mouse up event.
                if (FindAtPosition(mousePosition) is GuiMenuVerticalItem item)
                {
                    editor.TrySwitchActiveEventReceiver(item);
                    item.OnMouseUp(button);
                }
            }
        }

        /// <summary>Called when a menu item inside of the window was clicked.</summary>
        public void OnMenuItemClicked()
        {
            Close();
        }

        /// <summary>
        /// Called when the user let go of the mouse, possibly over a different menu item.
        /// </summary>
        public void OnMouseUpOutsideActiveMenuItem(int button)
        {
            // the menu item the mouse is hovering over, if there is one, never got a mouse down
            // event, so it's not active, thus we manually forward the mouse up event.
            if (FindAtPosition(mousePosition) is GuiMenuVerticalItem item)
            {
                editor.TrySwitchActiveEventReceiver(item);
                item.OnMouseUp(button);
            }
        }
    }
}

#endif