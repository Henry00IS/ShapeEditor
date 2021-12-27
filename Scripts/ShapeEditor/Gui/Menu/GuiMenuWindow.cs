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

        public GuiMenuWindow(float2 position, GuiMenuItem parentMenu) : base(position, 2f)
        {
            this.parentMenu = parentMenu;

            colorWindowBackground = colorMenuBackground;

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
    }
}

#endif