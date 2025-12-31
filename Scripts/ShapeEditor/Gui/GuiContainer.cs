#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// The base type of all GUI elements, that can hold a collection of children and forward events.
    /// </summary>
    public abstract class GuiContainer<T> : IGuiContainerEventReceiver<T> where T : IGuiContainerEventReceiver<T>
    {
        public ShapeEditorWindow editor { get; set; }

        public IGuiContainerEventReceiver parent { get; set; }

        public List<T> children { get; set; } = new List<T>();

        public float2 position { get; set; }

        public float2 size { get; set; }

        public bool isActive => editor.IsActive(this);

        public float2 drawPosition
        {
            get
            {
                var offset = position;
                var current = parent;
                while (current != null)
                {
                    offset += current.position;
                    current = current.parent;
                }
                return offset;
            }
        }

        public float2 mousePosition
        {
            get
            {
                return editor.mousePosition - drawPosition;
            }
        }

        public Rect rect => new Rect(position, size);

        public Rect sizeRect => new Rect(float2.zero, size);

        public Rect drawRect => new Rect(drawPosition, size);

        public bool isMouseOver => sizeRect.Contains(mousePosition);

        public T activeChild
        {
            get
            {
                if (editor.GetActiveEventReceiver() is IGuiContainerEventReceiver guiContainer)
                {
                    if (guiContainer.IsChildOf(this))
                    {
                        return children.Find(i => guiContainer.Equals(i));
                    }
                }
                return default;
            }
        }

        private bool _isMouseObstructed = false;

        public bool isMouseObstructed
        {
            get => _isMouseObstructed;
            set
            {
                _isMouseObstructed = value;

                // propogate this flag to all child instances.
                var childrenCount = children.Count;
                for (int i = 0; i < childrenCount; i++)
                    children[i].isMouseObstructed = value;
            }
        }

        /// <summary>Gets whether the mouse is hovering over the control and not busy.</summary>
        public bool isMouseOverNotBusy => isMouseOver && !editor.isMouseBusy;

        /// <summary>
        /// Returns true when the mouse is not obstructed, hovers over the control and is not busy.
        /// This check should be used before displaying hover effects.
        /// </summary>
        public bool isMouseHoverEffectApplicable => !isMouseObstructed && isMouseOverNotBusy;

        public T FindAtPosition(float2 position)
        {
            var childrenCount = children.Count;
            for (int i = childrenCount; i-- > 0;)
                if (children[i].rect.Contains(position))
                    return children[i];
            return default;
        }

        public T Add(T child)
        {
            // set up the parent references in the child.
            child.parent = this;

            // add the child to the container.
            children.Add(child);

            // activate the child.
            child.OnActivate();

            return child;
        }

        public void AddRange(IEnumerable<T> children)
        {
            foreach (var child in children)
                Add(child);
        }

        public bool IsActiveOrHasActiveChild()
        {
            if (isActive) return true;

            var childrenCount = children.Count;
            for (int i = 0; i < childrenCount; i++)
                if (children[i].IsActiveOrHasActiveChild())
                    return true;

            return false;
        }

        public bool IsActiveOrHasActiveParent()
        {
            if (isActive) return true;

            var current = parent;
            while (current != null)
            {
                if (current.isActive)
                    return true;

                current = current.parent;
            }
            return false;
        }

        public bool IsChildOf(IGuiContainerEventReceiver parent)
        {
            var current = parent;
            while (current != null)
            {
                if (current == parent)
                    return true;

                current = current.parent;
            }
            return false;
        }

        public virtual bool IsBusy()
        {
            return false;
        }

        public virtual void OnActivate()
        {
            // ensure the editor is always assigned.
            if (editor == null)
            {
                // find the editor reference in one of the parents.
                var current = parent;
                while (current != null)
                {
                    if (current.editor != null)
                    {
                        editor = current.editor;
                        break;
                    }
                    current = current.parent;
                }
            }
        }

        public virtual void OnDeactivate()
        {
        }

        public virtual void OnFocus()
        {
        }

        public virtual void OnFocusLost()
        {
        }

        public virtual void OnGlobalMouseUp(int button)
        {
        }

        public virtual bool OnKeyDown(KeyCode keyCode)
        {
            return false;
        }

        public virtual bool OnKeyUp(KeyCode keyCode)
        {
            return false;
        }

        public virtual void OnMouseDown(int button)
        {
            // try to give focus to a child container.
            var child = FindAtPosition(mousePosition);

            if (editor.TrySwitchActiveEventReceiver(child))
            {
                // forward the event to a child container.
                child?.OnMouseDown(button);
            }
        }

        public virtual void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
        }

        public virtual void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
        }

        public virtual void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            // forward this event to the topmost child container under the mouse position.
            var child = FindAtPosition(mousePosition);

            child?.OnMouseMove(screenDelta, gridDelta);
        }

        public virtual bool OnMouseScroll(float delta)
        {
            // forward this event to the topmost child container under the mouse position.
            var child = FindAtPosition(mousePosition);

            if (child != null)
            {
                // forward the event to a child container.
                return child.OnMouseScroll(delta);
            }
            return false;
        }

        public virtual void OnMouseUp(int button)
        {
        }

        public virtual void OnRender()
        {
            // render every child container.
            var childrenCount = children.Count;
            for (int i = 0; i < childrenCount; i++)
                children[i].OnRender();
        }
    }
}

#endif