#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a keyboard shortcut scale widget.</summary>
    public class QuickScaleWidget : Widget
    {
        private bool _wantsActive;
        private bool isDone;

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

        /// <summary>Activates this keyboard shortcut widget.</summary>
        public void Activate()
        {
            editor.activeWidget = this;
            _wantsActive = true;
            isDone = false;

            initialGridPosition = editor.ScreenPointToGrid(position);
            initialDistance = math.distance(initialGridPosition, editor.mouseGridPosition);
            onBeginScaling?.Invoke();
        }

        public override void OnRender()
        {
            if (!isActive || isDone) return;

            GLUtilities.DrawGui(() =>
            {
                GL.Color(Color.gray);
                GLUtilities.DrawDottedLine(1.0f, editor.mousePosition, editor.GridPointToScreen(initialGridPosition));
            });

            editor.SetMouseCursor(MouseCursor.ScaleArrow);
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

            float2 scale = math.distance(initialGridPosition, editor.mouseGridPosition);
            if (initialDistance == 0f)
            {
                scale = new float2(1.0f, 1.0f);
            }
            else
            {
                scale /= initialDistance;
            }

            onMouseDrag?.Invoke(initialGridPosition, scale);
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