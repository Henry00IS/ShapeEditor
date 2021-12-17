#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a control inside of a window.</summary>
    public class GuiControl
    {
        /// <summary>The parent window that this control resides in.</summary>
        public GuiWindow parent;
        /// <summary>The relative position to the parent.</summary>
        public float2 position;
        /// <summary>The size of the control.</summary>
        public float2 size;

        /// <summary>The top left draw position inside of the client area of the window.</summary>
        public float2 drawPosition => parent.position + position + new float2(1f, 1f);

        /// <summary>Creates a new control at the specified position of the specified size.</summary>
        /// <param name="position">The relative control position in screen coordinates.</param>
        /// <param name="size">The control size in screen coordinates.</param>
        public GuiControl(float2 position, float2 size)
        {
            this.position = position;
            this.size = size;
        }

        /// <summary>Called when the control is rendered.</summary>
        public virtual void OnRender()
        {
        }
    }
}

#endif