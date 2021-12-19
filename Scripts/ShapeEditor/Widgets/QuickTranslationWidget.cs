#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a keyboard shortcut translation widget.</summary>
    public class QuickTranslationWidget : Widget
    {
        private bool _wantsActive;
        private bool isDone;

        /// <summary>Called before this widget begins translating.</summary>
        public System.Action onBeginTranslating;

        /// <summary>
        /// Called whenever this translation widget is moved by the mouse and provides the screen
        /// delta and grid delta position changes.
        /// </summary>
        public System.Action<float2, float2> onMouseDrag;

        public override bool wantsActive => _wantsActive;

        /// <summary>Activates this keyboard shortcut widget.</summary>
        public void Activate()
        {
            editor.activeWidget = this;
            _wantsActive = true;
            isDone = false;

            onBeginTranslating?.Invoke();
        }

        public override void OnRender()
        {
            if (!isActive || isDone) return;

            editor.SetMouseCursor(MouseCursor.MoveArrow);
        }

        public override void OnMouseDown(int button)
        {
            if (!isActive) return;

            if (button == 0)
            {
                isDone = true;
            }
        }

        public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            if (!isActive || isDone) return;

            onMouseDrag?.Invoke(screenDelta, gridDelta);
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (button == 0)
            {
                _wantsActive = false;
            }
        }
    }
}

#endif