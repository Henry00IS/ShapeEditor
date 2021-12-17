#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a floating window inside of the 2D Shape Editor.</summary>
    public class GuiWindow
    {
        /// <summary>The window position in screen coordinates.</summary>
        public float2 position;
        /// <summary>The window size in screen coordinates.</summary>
        public float2 size;
        /// <summary>The collection of child controls inside of the window.</summary>
        private List<GuiControl> controls = new List<GuiControl>();

        private static readonly Color colorWindowBackground = new Color(0.192f, 0.192f, 0.192f, 0.5f);
        private static readonly Color colorWindowBorder = new Color(0.1f, 0.1f, 0.1f);

        /// <summary>Creates a new window at the specified position of the specified size.</summary>
        /// <param name="position">The window position in screen coordinates.</param>
        /// <param name="size">The window size in screen coordinates.</param>
        public GuiWindow(float2 position, float2 size)
        {
            this.position = position;
            this.size = size;
        }

        /// <summary>Adds the specified control to the window.</summary>
        /// <param name="control">The control to be added to the window.</param>
        public void AddControl(GuiControl control)
        {
            control.parent = this;
            controls.Add(control);
        }

        /// <summary>Called when the window is rendered.</summary>
        public void OnRender()
        {
            // render the window.
            var guiMaterial = ShapeEditorResources.temporaryGuiMaterial;
            guiMaterial.mainTexture = null;
            guiMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.LoadIdentity();
            GLUtilities.DrawTransparentRectangleWithOutline(position.x, position.y, size.x, size.y, colorWindowBackground, colorWindowBorder);
            GL.End();
            GL.PopMatrix();

            // render every child control.
            var controlsCount = controls.Count;
            for (int i = 0; i < controlsCount; i++)
                controls[i].OnRender();
        }
    }
}

#endif