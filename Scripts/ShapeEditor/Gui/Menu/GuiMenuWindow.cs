#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents an opened menu that shows selectable menu items.</summary>
    public class GuiMenuWindow : GuiWindow
    {
        private static readonly Color colorMenuBackground = new Color(0.106f, 0.106f, 0.110f);

        /// <summary>The parent menu that has a collection of menu items.</summary>
        public readonly GuiMenuItem parentMenu;

        public GuiMenuWindow(GuiMenuItem item) : base(0f, 2f)
        {
            parentMenu = item;

            colorWindowBackground = colorMenuBackground;

            // calculate the menu position.
            position = new float2(item.drawRect.x, item.drawRect.yMax);

            // calculate the required menu size.
            size = new float2(item.GetLargestWidthOfChildren(), item.GetMenuHeight() + 2f);

            // add the menu items.
            foreach (var child in item.children)
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

        /// <summary>Called when a menu item inside of the window was clicked.</summary>
        public void OnMenuItemClicked()
        {
            Close();
        }
    }
}

#endif