#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a control inside of a window.</summary>
    public abstract class GuiControl : GuiContainer<GuiControl>
    {
        /// <summary>Creates a new control at the specified position of the specified size.</summary>
        /// <param name="position">The relative control position in screen coordinates.</param>
        /// <param name="size">The control size in screen coordinates.</param>
        public GuiControl(float2 position, float2 size)
        {
            this.position = position;
            this.size = size;
        }
    }
}

#endif