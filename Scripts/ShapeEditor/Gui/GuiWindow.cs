#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a floating window inside of the 2D Shape Editor.</summary>
    public class GuiWindow : IEditorEventReceiver
    {
        /// <summary>The shape editor window.</summary>
        public ShapeEditorWindow editor { get; set; }

        /// <summary>The window position in screen coordinates.</summary>
        public float2 position;
        /// <summary>The window size in screen coordinates.</summary>
        public float2 size;

        /// <summary>The collection of child controls inside of the window.</summary>
        protected List<GuiControl> controls = new List<GuiControl>();

        /// <summary>Do not access directly, use <see cref="activeControl"/>!</summary>
        private GuiControl _activeControl;

        /// <summary>The control that currently has input focus or null.</summary>
        internal GuiControl activeControl
        {
            get => _activeControl;
            set
            {
                if (_activeControl == value) return;

                if (_activeControl != null)
                {
                    var lastActiveControl = _activeControl;
                    _activeControl = value;
                    lastActiveControl.OnFocusLost();
                }

                _activeControl = value;
                _activeControl?.OnFocus();
            }
        }

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
        /// Gets whether the window is busy and has to maintain the input focus, making it
        /// impossible to switch to another object.
        /// </summary>
        public virtual bool IsBusy()
        {
            return false;
        }

        /// <summary>Adds the specified control to the window.</summary>
        /// <param name="control">The control to be added to the window.</param>
        public void AddControl(GuiControl control)
        {
            control.parent = this;
            controls.Add(control);
        }

        /// <summary>Called when the window is activated.</summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>Called when the window is deactivated.</summary>
        public virtual void OnDeactivate()
        {
        }

        /// <summary>Called when the window is rendered.</summary>
        public virtual void OnRender()
        {
            // render the window.
            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawTransparentRectangleWithOutline(position.x, position.y, size.x, size.y, colorWindowBackground, colorWindowBorder);
            });

            // render every child control.
            var controlsCount = controls.Count;
            for (int i = 0; i < controlsCount; i++)
                controls[i].OnRender();
        }

        /// <summary>Called when the window receives input focus.</summary>
        public virtual void OnFocus()
        {
            if (activeControl != null)
            {
                activeControl.OnParentFocus();
            }
        }

        /// <summary>Called when the window loses input focus.</summary>
        public virtual void OnFocusLost()
        {
            if (activeControl != null)
            {
                // also check whether the control should remain active when the parent has focus again.
                if (!activeControl.OnParentFocusLost())
                {
                    activeControl = null;
                }
            }
        }

        /// <summary>Called when the window receives a mouse down event.</summary>
        public virtual void OnMouseDown(int button)
        {
            // possibly forward the event to a control.
            activeControl = FindControlAtPosition(mousePosition);
            if (activeControl != null)
            {
                activeControl.OnMouseDown(button);
            }
        }

        /// <summary>Called when the window receives a mouse up event.</summary>
        public virtual void OnMouseUp(int button)
        {
            if (activeControl != null)
            {
                activeControl.OnMouseUp(button);
            }
        }

        /// <summary>Called when the window receives a global mouse up event.</summary>
        public virtual void OnGlobalMouseUp(int button)
        {
            if (activeControl != null)
            {
                activeControl.OnGlobalMouseUp(button);
            }
        }

        /// <summary>Called when the window receives a mouse drag event.</summary>
        public virtual void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (activeControl != null)
            {
                activeControl.OnMouseDrag(button, screenDelta, gridDelta);
            }
        }

        /// <summary>Called when the window receives a mouse move event.</summary>
        public virtual void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            // forward this event to the topmost control under the mouse position.
            var control = FindControlAtPosition(mousePosition);
            if (control != null)
                control.OnMouseMove(screenDelta, gridDelta);
        }

        /// <summary>Called when the window receives a mouse scroll event.</summary>
        public virtual bool OnMouseScroll(float delta)
        {
            if (activeControl != null)
            {
                return activeControl.OnMouseScroll(delta);
            }
            return false;
        }

        /// <summary>Called when the window receives a key down event.</summary>
        public virtual bool OnKeyDown(KeyCode keyCode)
        {
            if (activeControl != null)
            {
                return activeControl.OnKeyDown(keyCode);
            }
            return false;
        }

        /// <summary>Called when the window receives a key up event.</summary>
        public virtual bool OnKeyUp(KeyCode keyCode)
        {
            if (activeControl != null)
            {
                return activeControl.OnKeyUp(keyCode);
            }
            return false;
        }

        /// <summary>Gets whether the window currently has input focus.</summary>
        public bool isActive => editor.IsActive(this);

        /// <summary>Whether there is an obstruction between the window and the mouse.</summary>
        public bool isMouseObstructed;

        /// <summary>Gets whether the mouse is hovering over the window.</summary>
        public bool isMouseOver => new Rect(float2.zero, size).Contains(mousePosition);

        /// <summary>Gets whether the mouse is hovering over the window and not busy.</summary>
        public bool isMouseOverNotBusy => isMouseOver && !editor.isMouseBusy;

        /// <summary>Gets the rectangle of the window.</summary>
        public Rect rect => new Rect(position, size);

        /// <summary>Gets the relative mouse position inside of the window.</summary>
        public float2 mousePosition => editor.mousePosition - position;

        /// <summary>Attempts to find the top control at the given relative position.</summary>
        /// <param name="position">The relative position to find the control at.</param>
        /// <returns>The control instance if found else null.</returns>
        public GuiControl FindControlAtPosition(float2 position)
        {
            var controlsCount = controls.Count;
            for (int i = 0; i < controlsCount; i++)
                if (controls[i].rect.Contains(position))
                    return controls[i];
            return null;
        }
    }
}

#endif