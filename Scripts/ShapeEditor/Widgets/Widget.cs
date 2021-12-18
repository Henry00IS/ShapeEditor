#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a viewport widget (e.g. transform handles).</summary>
    public abstract class Widget
    {
        /// <summary>The shape editor window.</summary>
        public ShapeEditorWindow editor;

        /// <summary>The widget position in screen coordinates.</summary>
        public float2 position;

        /// <summary>Whether the widget is visible.</summary>
        public bool visible = true;

        /// <summary>Gets whether the widget wants to be active with input focus.</summary>
        public abstract bool wantsActive { get; }

        /// <summary>Gets whether the widget currently has input focus.</summary>
        public bool isActive => this == editor.activeWidget;

        /// <summary>Called when the widget is activated.</summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>Called when the widget is deactivated.</summary>
        public virtual void OnDeactivate()
        {
        }

        /// <summary>Called when the widget is rendered.</summary>
        public virtual void OnRender()
        {
        }

        /// <summary>Called when the widget receives a mouse down event.</summary>
        public virtual void OnMouseDown(int button)
        {
        }

        /// <summary>Called when the widget receives a mouse up event.</summary>
        public virtual void OnMouseUp(int button)
        {
        }

        /// <summary>Called when the widget receives a global mouse up event.</summary>
        public virtual void OnGlobalMouseUp(int button)
        {
        }

        /// <summary>Called when the widget receives a mouse drag event.</summary>
        public virtual void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
        }

        /// <summary>Called when the widget receives a mouse move event.</summary>
        public virtual void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
        }

        /// <summary>Called when the widget receives a key down event.</summary>
        public virtual bool OnKeyDown(KeyCode keyCode)
        {
            return false;
        }

        /// <summary>Called when the widget receives a key up event.</summary>
        public virtual bool OnKeyUp(KeyCode keyCode)
        {
            return false;
        }
    }
}

#endif