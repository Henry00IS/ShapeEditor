#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a viewport rotation widget.</summary>
    public class RotationWidget : Widget
    {
        private const float radius = 64f;
        private bool _wantsActive;

        private float initialRotation;
        private float2 initialGridPosition;

        /// <summary>Called before this widget begins rotating.</summary>
        public System.Action onBeginRotating;

        /// <summary>
        /// Called whenever this widget is rotated and provides the pivot in grid coordinates and
        /// the amount of degrees from -180 to 180.
        /// </summary>
        public System.Action<float2, float> onRotation;

        public override bool wantsActive => _wantsActive;

        public override void OnRender()
        {
            if (!visible || isOtherActive) return;

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
                    initialRotation = Vector2.SignedAngle(editor.mousePosition - position, Vector2.up);
                    initialGridPosition = editor.ScreenPointToGrid(position);
                }

                // on mouse down is called twice.
                if (isActive)
                {
                    onBeginRotating?.Invoke();
                }
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (!visible || !isActive) return;

            if (button == 0)
            {
                var currentRotation = Vector2.SignedAngle(editor.mousePosition - position, Vector2.up);
                var delta = Mathf.DeltaAngle(initialRotation, currentRotation);

                onRotation?.Invoke(initialGridPosition, -delta);
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