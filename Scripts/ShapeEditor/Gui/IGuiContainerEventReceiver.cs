#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public interface IGuiContainerEventReceiver : IEditorEventReceiver
    {
        /// <summary>The parent container that this container resides in or null.</summary>
        IGuiContainerEventReceiver parent { get; set; }

        /// <summary>The relative position to the parent container.</summary>
        float2 position { get; set; }

        /// <summary>The size of the container.</summary>
        float2 size { get; set; }

        /// <summary>Gets whether the container has input focus.</summary>
        public bool isActive { get; }

        /// <summary>The draw position in screen coordinates for the container.</summary>
        public float2 drawPosition { get; }

        /// <summary>Gets the relative mouse position inside of the container.</summary>
        public float2 mousePosition { get; }

        /// <summary>Gets the relative rectangle of the container inside of the parent.</summary>
        public Rect rect { get; }

        /// <summary>Gets the rectangle of the container (no position only size).</summary>
        public Rect sizeRect { get; }

        /// <summary>Gets the rectangle of the container in screen coordinates.</summary>
        public Rect drawRect { get; }

        /// <summary>Gets whether the mouse is hovering over the container.</summary>
        public bool isMouseOver { get; }

        /// <summary>
        /// While rendering the <see cref="GuiWindow"/> instances in the shape editor, the shape
        /// editor will determine whether there is an obstruction between the window and the mouse.
        /// This property can be used to determine whether hover effects are applicable. The flag is
        /// propogated to all child containers but only checks whether the mouse can see the window,
        /// not whether it's blocked by other containers.
        /// </summary>
        public bool isMouseObstructed { get; set; }

        /// <summary>Gets whether this container or a parent container has input focus.</summary>
        public bool IsActiveOrHasActiveParent();

        /// <summary>Gets whether this container is a child of the specified parent container.</summary>
        public bool IsChildOf(IGuiContainerEventReceiver parent);
    }

    public interface IGuiContainerEventReceiver<T> : IGuiContainerEventReceiver where T : IGuiContainerEventReceiver<T>
    {
        /// <summary>The collection of children inside of the container.</summary>
        List<T> children { get; set; }

        /// <summary>The child <typeparamref name="T"/> that currently has input focus or null.</summary>
        public T activeChild { get; }

        /// <summary>Gets whether this container or recursively a child container has input focus.</summary>
        public bool IsActiveOrHasActiveChild();

        /// <summary>Attempts to find a child <typeparamref name="T"/> at the given relative position.</summary>
        /// <param name="position">The relative position to find the <typeparamref name="T"/> at.</param>
        /// <returns>The <typeparamref name="T"/> instance if found else null.</returns>
        public T FindAtPosition(float2 position);

        /// <summary>
        /// Adds the specified <typeparamref name="T"/> to the container as a child.
        /// </summary>
        /// <param name="child">The child <typeparamref name="T"/> to be added.</param>
        /// <returns>A reference to the child that was added to the container.</returns>
        public T Add(T child);

        /// <summary>
        /// Adds the specified enumerable of <typeparamref name="T"/> to the container as children.
        /// </summary>
        /// <param name="children">The child enumerable <typeparamref name="T"/> to be added.</param>
        public void AddRange(IEnumerable<T> children);
    }
}

#endif