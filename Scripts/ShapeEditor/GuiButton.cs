#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a button control inside of a window.</summary>
    public class GuiButton : GuiControl
    {
        public Texture2D icon;

        private static readonly Color colorButtonBackground = new Color(0.192f, 0.192f, 0.192f, 0.5f);
        private static readonly Color colorButtonBorder = new Color(0.1f, 0.1f, 0.1f);

        public GuiButton(Texture2D icon, float2 position, float2 size) : base(position, size)
        {
            this.icon = icon;
        }

        /// <summary>Called when the control is rendered.</summary>
        public override void OnRender()
        {
            // draw the button outline.
            ShapeEditorResources.temporaryLineMaterial.SetPass(0);
            GL.Begin(GL.QUADS);
            GLUtilities.DrawTransparentRectangleWithOutline(drawPosition.x, drawPosition.y, size.x, size.y, colorButtonBackground, colorButtonBorder);
            //GLUtilities.DrawRectangleOutline(drawPosition.x, drawPosition.y, size.x, size.y, Color.white);
            GL.End();

            var drawTextureMaterial = ShapeEditorResources.temporaryDrawTextureMaterial;
            drawTextureMaterial.mainTexture = icon;
            drawTextureMaterial.SetPass(0);

            // draw the button icon.
            GL.Begin(GL.QUADS);
            GLUtilities.DrawFlippedUvRectangle(drawPosition.x + (size.x / 2f) - (icon.width / 2f), drawPosition.y + (size.y / 2f) - (icon.height / 2f), icon.width, icon.height);
            GL.End();
        }
    }
}

#endif