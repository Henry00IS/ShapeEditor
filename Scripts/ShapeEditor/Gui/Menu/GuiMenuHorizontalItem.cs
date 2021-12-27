#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A horizontal menu item used in menu bars.</summary>
    public class GuiMenuHorizontalItem : GuiMenuItem
    {
        public GuiMenuHorizontalItem(string text) : base(text)
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
            base.OnRender();

            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, text, drawPosition + new float2(7f, 3f));
        }
    }
}

#endif