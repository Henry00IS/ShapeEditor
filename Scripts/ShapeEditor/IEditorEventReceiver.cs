#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>An object that can have input focus and receive editor events.</summary>
    public interface IEditorEventReceiver
    {
        /// <summary>The shape editor window.</summary>
        ShapeEditorWindow editor { get; set; }

        /// <summary>
        /// Gets whether the object is busy and has to maintain the input focus, making it
        /// impossible to switch to another object.
        /// </summary>
        bool IsBusy();

        /// <summary>Called when the object is activated.</summary>
        void OnActivate();

        /// <summary>Called when the object is deactivated.</summary>
        void OnDeactivate();

        /// <summary>Called when the object is rendered.</summary>
        void OnRender();

        /// <summary>Called when the object receives a mouse down event.</summary>
        void OnMouseDown(int button);

        /// <summary>Called when the object receives a mouse up event.</summary>
        void OnMouseUp(int button);

        /// <summary>Called when the object receives a global mouse up event.</summary>
        void OnGlobalMouseUp(int button);

        /// <summary>Called when the object receives a mouse drag event.</summary>
        void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta);

        /// <summary>Called when the object receives a global mouse drag event.</summary>
        void OnGlobalMouseDrag(int button, float2 screenDelta, float2 gridDelta);

        /// <summary>Called when the object receives a mouse move event.</summary>
        void OnMouseMove(float2 screenDelta, float2 gridDelta);

        /// <summary>Called when the object receives a mouse scroll event.</summary>
        bool OnMouseScroll(float delta);

        /// <summary>Called when the object receives a key down event.</summary>
        bool OnKeyDown(KeyCode keyCode);

        /// <summary>Called when the object receives a key up event.</summary>
        bool OnKeyUp(KeyCode keyCode);

        /// <summary>Called when the object receives input focus.</summary>
        void OnFocus();

        /// <summary>Called when the object loses input focus.</summary>
        void OnFocusLost();
    }
}

#endif