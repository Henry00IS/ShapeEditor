#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class RotateTool : BoxSelectTool
    {
        private bool isSingleUseDone = false;
        private float initialRotation;
        private float2 initialGridPosition;

        private RotationWidget rotationWidget = new RotationWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            if (isSingleUse)
            {
                initialRotation = Vector2.SignedAngle(editor.mousePosition - editor.selectedSegmentsAveragePosition, Vector2.up);
                initialGridPosition = editor.ScreenPointToGrid(editor.selectedSegmentsAveragePosition);
                ToolOnBeginRotating();
            }
            else
            {
                editor.AddWidget(rotationWidget);
                rotationWidget.onBeginRotating = () => ToolOnBeginRotating();
                rotationWidget.onRotation = (pivot, degrees) => ToolOnRotation(pivot, degrees);
            }
        }

        public override void OnRender()
        {
            base.OnRender();

            if (isSingleUse)
            {
                GLUtilities.DrawGui(() =>
                {
                    GL.Color(Color.gray);
                    GLUtilities.DrawDottedLine(1.0f, editor.mousePosition, editor.GridPointToScreen(initialGridPosition));
                });

                editor.SetMouseCursor(MouseCursor.RotateArrow);
            }
            else
            {
                if (editor.selectedSegmentsCount > 0)
                {
                    rotationWidget.position = editor.selectedSegmentsAveragePosition;
                    rotationWidget.visible = true;
                }
                else
                {
                    rotationWidget.visible = false;
                }
            }
        }

        public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            if (isSingleUse && !isSingleUseDone)
            {
                var position = editor.GridPointToScreen(initialGridPosition);
                var currentRotation = Vector2.SignedAngle(editor.mousePosition - position, Vector2.up);
                var delta = Mathf.DeltaAngle(initialRotation, currentRotation);

                ToolOnRotation(initialGridPosition, -delta);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    isSingleUseDone = true;
                }
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    // we do not want the marquee in this mode.
                    return;
                }
            }

            base.OnMouseDrag(button, screenDelta, gridDelta);
        }

        public override void OnGlobalMouseUp(int button)
        {
            if (isSingleUse)
            {
                if (button == 0)
                {
                    editor.SwitchTool(parent);
                }
            }
            else
            {
                base.OnGlobalMouseUp(button);
            }
        }

        public override bool IsBusy()
        {
            if (isSingleUse)
            {
                return !isSingleUseDone;
            }
            return false;
        }

        public override bool OnKeyDown(KeyCode keyCode)
        {
            if (isSingleUse)
            {
                switch (keyCode)
                {
                    case KeyCode.Escape:
                        ToolOnCancel();
                        return true;
                }
            }
            return base.OnKeyDown(keyCode);
        }

        private void ToolOnBeginRotating()
        {
            editor.RegisterUndo("Rotate Selection");

            // store the initial position of all selected segments.
            foreach (var segment in editor.ForEachSelectedObject())
                segment.gpVector1 = segment.position;
        }

        private void ToolOnRotation(float2 pivot, float degrees)
        {
            if (editor.isSnapping)
            {
                degrees = degrees.Snap(editor.angleSnap);
            }

            // rotate the selected segments using their initial position.
            foreach (var segment in editor.ForEachSelectedObject())
                segment.position = MathEx.RotatePointAroundPivot(segment.gpVector1, pivot, degrees);
        }

        /// <summary>Cancels the single-use tool operation and undoes all changes.</summary>
        private void ToolOnCancel()
        {
            // undo rotation.
            foreach (var segment in editor.ForEachSelectedObject())
                segment.position = segment.gpVector1;

            // discard undo operation.
            editor.DiscardUndo();

            // exit the tool.
            isSingleUseDone = true;
            editor.SwitchTool(parent);
        }
    }
}

#endif