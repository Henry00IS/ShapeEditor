#if UNITY_EDITOR

using Unity.Mathematics;
using TranslationGizmoState = AeternumGames.ShapeEditor.GLUtilities.TranslationGizmoState;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a viewport translation widget.</summary>
    public class TranslationWidget : Widget
    {
        private bool _wantsActive;
        private TranslationGizmoState activeTranslationGizmoState;
        private TranslationGizmoState currentTranslationGizmoState;

        /// <summary>
        /// Called whenever this translation widget is dragged by the mouse and provides the screen
        /// delta and grid delta position changes.
        /// </summary>
        public System.Action<float2, float2> onMouseDrag;

        public override bool wantsActive => _wantsActive;

        public override void OnRender()
        {
            if (!visible) return;

            GLUtilities.DrawGui(() =>
            {
                GLUtilities.DrawTranslationGizmo(position, editor.mousePosition, ref currentTranslationGizmoState);
            });

            if (isActive)
            {
                activeTranslationGizmoState.UpdateMouseCursor(editor);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (!visible) return;

            if (button == 0)
            {
                activeTranslationGizmoState = currentTranslationGizmoState;
                _wantsActive = activeTranslationGizmoState.isActive;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (!visible || !isActive) return;

            if (button == 0)
            {
                onMouseDrag?.Invoke(activeTranslationGizmoState.ModifyDeltaMovement(screenDelta), activeTranslationGizmoState.ModifyDeltaMovement(gridDelta));
            }
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