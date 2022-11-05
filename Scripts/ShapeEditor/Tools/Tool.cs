#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a viewport tool that's used to manipulate shapes.</summary>
    public abstract class Tool : IEditorEventReceiver
    {
        /// <summary>The shape editor window.</summary>
        public ShapeEditorWindow editor { get; set; }

        /// <summary>
        /// The parent tool that called this tool (if any), to which the editor will return once the
        /// tool is finished. This is set when a single-use tool is instantiated with a keyboard binding.
        /// </summary>
        public Tool parent;

        /// <summary>Whether this tool is in single-use mode.</summary>
        public bool isSingleUse => parent != null;

        /// <summary>
        /// Gets whether the tool is busy and has to maintain the input focus, making it
        /// impossible to switch to another object.
        /// </summary>
        public virtual bool IsBusy()
        {
            return false;
        }

        /// <summary>Called when the tool is activated.</summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>Called when the tool is deactivated.</summary>
        public virtual void OnDeactivate()
        {
        }

        /// <summary>Called when the tool is rendered.</summary>
        public virtual void OnRender()
        {
        }

        /// <summary>Called when the tool receives a mouse down event.</summary>
        public virtual void OnMouseDown(int button)
        {
        }

        /// <summary>Called when the tool receives a mouse up event.</summary>
        public virtual void OnMouseUp(int button)
        {
        }

        /// <summary>Called when the tool receives a global mouse up event.</summary>
        public virtual void OnGlobalMouseUp(int button)
        {
        }

        /// <summary>Called when the tool receives a mouse drag event.</summary>
        public virtual void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
        }

        /// <summary>Called when the tool receives a global mouse drag event.</summary>
        public virtual void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
        }

        /// <summary>Called when the tool receives a mouse move event.</summary>
        public virtual void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
        }

        /// <summary>Called when the tool receives a mouse scroll event.</summary>
        public virtual bool OnMouseScroll(float delta)
        {
            return false;
        }

        /// <summary>Called when the tool receives a key down event.</summary>
        public virtual bool OnKeyDown(KeyCode keyCode)
        {
            return false;
        }

        /// <summary>Called when the tool receives a key up event.</summary>
        public virtual bool OnKeyUp(KeyCode keyCode)
        {
            return false;
        }

        /// <summary>Called when the tool receives input focus.</summary>
        public virtual void OnFocus()
        {
        }

        /// <summary>Called when the tool loses input focus.</summary>
        public virtual void OnFocusLost()
        {
        }
    }
}

#endif