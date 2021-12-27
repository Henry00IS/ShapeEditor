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

        /// <summary>Gets whether the mouse is hovering over the control and not busy.</summary>
        public bool isMouseOverNotBusy => isMouseOver && !editor.isMouseBusy;

        /// <summary>
        /// Returns true when the mouse is not obstructed, hovers over the control and is not busy.
        /// This check should be used before displaying hover effects.
        /// </summary>
        public bool isMouseHoverEffectApplicable => !isMouseObstructed && isMouseOverNotBusy;
    }
}

#endif