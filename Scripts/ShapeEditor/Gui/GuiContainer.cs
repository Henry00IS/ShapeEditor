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

        private T _activeChild;

        public T activeChild
        {
            get => _activeChild;
            set
            {
                _activeChild = value;

                if (_activeChild != null)
                {
                    editor.TrySwitchActiveEventReceiver(_activeChild);
                }
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
            // inform the active child that we got focus.
            activeChild?.OnParentFocus();
        }

        public virtual void OnFocusLost()
        {
            // inform the active child that we lost focus.
            activeChild?.OnParentFocusLost();
        }

        public virtual void OnParentFocus()
        {
        }

        public virtual void OnParentFocusLost()
        {
        }

        public virtual void OnGlobalMouseUp(int button)
        {
            activeChild?.OnGlobalMouseUp(button);
        }

        public virtual bool OnKeyDown(KeyCode keyCode)
        {
            if (activeChild != null)
            {
                return activeChild.OnKeyDown(keyCode);
            }
            return false;
        }

        public virtual bool OnKeyUp(KeyCode keyCode)
        {
            if (activeChild != null)
            {
                return activeChild.OnKeyUp(keyCode);
            }
            return false;
        }

        public virtual void OnMouseDown(int button)
        {
            // try to give focus to a child container.
            activeChild = FindAtPosition(mousePosition);

            // forward the event to a child container.
            activeChild?.OnMouseDown(button);
        }

        public virtual void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            activeChild?.OnMouseDrag(button, screenDelta, gridDelta);
        }

        public virtual void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            // forward this event to the topmost child container under the mouse position.
            var container = FindAtPosition(mousePosition);
            container?.OnMouseMove(screenDelta, gridDelta);
        }

        public virtual bool OnMouseScroll(float delta)
        {
            if (activeChild != null)
            {
                return activeChild.OnMouseScroll(delta);
            }
            return false;
        }

        public virtual void OnMouseUp(int button)
        {
            activeChild?.OnMouseUp(button);
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