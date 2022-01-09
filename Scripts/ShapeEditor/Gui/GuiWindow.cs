#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a floating window inside of the 2D Shape Editor.</summary>
    public class GuiWindow : GuiContainer<GuiControl>
    {
        /// <summary>
        /// Gets whether the window was closed and is about to be removed.
        /// <para>Use <see cref="Open"/> and <see cref="Close"/> instead of setting this flag.</para>
        /// </summary>
        public bool closed { get; set; }

        protected Color colorWindowBackground = new Color(0.192f, 0.192f, 0.192f, 0.5f);
        protected Color colorWindowBorder = new Color(0.1f, 0.1f, 0.1f);

        /// <summary>Creates a new window at the specified position of the specified size.</summary>
        /// <param name="position">The window position in screen coordinates.</param>
        /// <param name="size">The window size in screen coordinates.</param>
        public GuiWindow(float2 position, float2 size)
        {
            this.position = position;
            this.size = size;
        }

        /// <summary>
        /// Marks the window as ready to be removed, removing it as soon as possible.
        /// </summary>
        public void Close()
        {
            closed = true;
        }

        /// <summary>Called when the window is rendered.</summary>
        public override void OnRender()
        {
            // render the window.
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawTransparentRectangleWithOutline(position.x, position.y, size.x, size.y, colorWindowBackground, colorWindowBorder);
            });

            // render the child containers.
            base.OnRender();
        }
    }
}

#endif