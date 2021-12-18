#if UNITY_EDITOR

using Unity.Mathematics;
using ScaleGizmoState = AeternumGames.ShapeEditor.GLUtilities.ScaleGizmoState;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a viewport translation widget.</summary>
    public class ScaleWidget : Widget
    {
        private bool _wantsActive;
        private ScaleGizmoState activeScaleGizmoState;
        private ScaleGizmoState currentScaleGizmoState;
        private float2 initialGridPosition;
        private float initialDistance;

        /// <summary>Called before this widget begins scaling.</summary>
        public System.Action onBeginScaling;

        /// <summary>
        /// Called whenever this scale widget is scaled and provides the pivot in grid coordinates
        /// and the scale.
        /// </summary>
        public System.Action<float2, float2> onMouseDrag;

        public override bool wantsActive => _wantsActive;

        public override void OnRender()
        {
            if (!visible) return;

            GLUtilities.DrawGui(() =>
            {
                var drawPosition = position;

                // keep the gizmo in the same place while scaling.
                if (isActive)
                    drawPosition = editor.GridPointToScreen(initialGridPosition);

                GLUtilities.DrawScaleGizmo(drawPosition, editor.mousePosition, ref currentScaleGizmoState);
            });

            if (isActive)
            {
                activeScaleGizmoState.UpdateMouseCursor(editor);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (!visible) return;

            if (button == 0)
            {
                activeScaleGizmoState = currentScaleGizmoState;
                _wantsActive = activeScaleGizmoState.isActive;

                // on mouse down is called twice.
                if (isActive)
                {
                    initialGridPosition = editor.ScreenPointToGrid(position);
                    initialDistance = math.distance(initialGridPosition, editor.mouseGridPosition);
                    onBeginScaling?.Invoke();
                }
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (!visible || !isActive) return;

            if (button == 0)
            {
                float2 scale = math.distance(initialGridPosition, editor.mouseGridPosition);
                if (initialDistance == 0f)
                {
                    scale = new float2(1.0f, 1.0f);
                }
                else
                {
                    scale /= initialDistance;
                }

                if (activeScaleGizmoState.isMouseOverY)
                {
                    scale.x = 1.0f;
                }
                else if (activeScaleGizmoState.isMouseOverX)
                {
                    scale.y = 1.0f;
                }

                onMouseDrag?.Invoke(initialGridPosition, scale);
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