#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a viewport rotation widget.</summary>
    public class RotationWidget : Widget
    {
        private const float radius = 32f;
        private bool _wantsActive;

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
                GLUtilities.DrawCircle(1.0f, position, radius, Color.white);
            });

            if (isActive)
            {
                editor.SetMouseCursor(MouseCursor.RotateArrow);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (!visible) return;

            if (button == 0)
            {
                if (math.distance(position, editor.mousePosition) <= radius)
                {
                    _wantsActive = true;
                }
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (!visible || !isActive) return;

            if (button == 0)
            {
                onMouseDrag?.Invoke(float2.zero, float2.zero);
            }
        }

        public override void OnGlobalMouseUp(int button)
        {
            _wantsActive = false;
        }
    }
}

#endif