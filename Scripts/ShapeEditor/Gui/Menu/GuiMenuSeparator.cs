#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A horizontal menu item used in menu bars.</summary>
    public class GuiMenuSeparator : GuiMenuItem
    {
        private static Color separatorColor = new Color(0.200f, 0.200f, 0.216f);

        public GuiMenuSeparator() : base("")
        {
            size = new float2(GetWidth(), GetHeight());
        }

        /// <summary>Gets the height of the menu item in a vertical menu.</summary>
        /// <returns>The height of the menu item.</returns>
        public override int GetHeight()
        {
            return 3;
        }

        /// <summary>Gets the width of the menu item in a vertical menu.</summary>
        /// <returns>The width of the menu item.</returns>
        public override int GetWidth()
        {
            return 0; // is set automatically by regular items.
        }

        public override void OnRender()
        {
            GLUtilities.DrawGui(() =>
            {
                GL.Color(separatorColor);
                GLUtilities.DrawLine(1.0f, drawPosition + new float2(0f, 1f), drawPosition + new float2(size.x, 1f));
            });
        }

        // ignore mouse down on separators, so we don't close the menu.
        public override void OnMouseDown(int button)
        {
        }
    }
}

#endif