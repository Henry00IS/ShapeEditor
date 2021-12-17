#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

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

        /// <summary>Called when the control receives a mouse down event.</summary>
        public virtual void OnMouseDown(int button)
        {
        }

        /// <summary>Called when the control receives a mouse up event.</summary>
        public virtual void OnMouseUp(int button)
        {
        }

        /// <summary>Called when the control receives a global mouse up event.</summary>
        public virtual void OnGlobalMouseUp(int button)
        {
        }

        /// <summary>Called when the control receives a mouse drag event.</summary>
        public virtual void OnMouseDrag(int button)
        {
        }

        /// <summary>Called when the control receives a mouse move event.</summary>
        public virtual void OnMouseMove(float2 screenDelta)
        {
        }

        /// <summary>Gets whether the control currently has input focus.</summary>
        public bool isActive => parent.isActive && this == parent.activeControl;

        /// <summary>Gets whether the mouse is hovering over the control.</summary>
        public bool isMouseOver => new Rect(float2.zero, size).Contains(mousePosition);

        /// <summary>Gets the rectangle of the control.</summary>
        public Rect rect => new Rect(position, size);

        /// <summary>Gets the relative mouse position inside of the control.</summary>
        public float2 mousePosition => parent.mousePosition - position;
    }
}

#endif