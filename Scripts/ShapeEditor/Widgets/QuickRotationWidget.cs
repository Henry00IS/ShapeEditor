#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Represents a keyboard shortcut rotation widget.</summary>
    public class QuickRotationWidget : Widget
    {
        private bool _wantsActive;
        private bool isDone;

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

        /// <summary>Activates this keyboard shortcut widget.</summary>
        public void Activate()
        {
            //editor.activeWidget = this;
            _wantsActive = true;
            isDone = false;

            initialRotation = Vector2.SignedAngle(editor.mousePosition - position, Vector2.up);
            initialGridPosition = editor.ScreenPointToGrid(position);
            onBeginRotating?.Invoke();
        }

        public override void OnRender()
        {
            if (!isActive || isDone) return;

            GLUtilities.DrawGui(() =>
            {
                GL.Color(Color.gray);
                GLUtilities.DrawDottedLine(1.0f, editor.mousePosition, editor.GridPointToScreen(initialGridPosition));
            });

            editor.SetMouseCursor(MouseCursor.RotateArrow);
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

            var currentRotation = Vector2.SignedAngle(editor.mousePosition - position, Vector2.up);
            var delta = Mathf.DeltaAngle(initialRotation, currentRotation);

            onRotation?.Invoke(initialGridPosition, -delta);
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