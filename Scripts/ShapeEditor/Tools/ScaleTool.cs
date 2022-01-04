#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class ScaleTool : BoxSelectTool
    {
        private bool isSingleUseDone = false;
        private float2 initialGridPosition;
        private float initialDistance;

        private ScaleWidget scaleWidget = new ScaleWidget();

        public override void OnActivate()
        {
            base.OnActivate();

            if (isSingleUse)
            {
                initialGridPosition = editor.ScreenPointToGrid(editor.selectedSegmentsAveragePosition);
                initialDistance = math.distance(initialGridPosition, editor.mouseGridPosition);
                ToolOnBeginScaling();
            }
            else
            {
                editor.AddWidget(scaleWidget);
                scaleWidget.onBeginScaling = () => ToolOnBeginScaling();
                scaleWidget.onMouseDrag = (pivot, scale) => ToolOnMouseDrag(pivot, scale);
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

                editor.SetMouseCursor(MouseCursor.ScaleArrow);
            }
            else
            {
                if (editor.selectedSegmentsCount > 0)
                {
                    scaleWidget.position = editor.selectedSegmentsAveragePosition;
                    scaleWidget.visible = true;
                }
                else
                {
                    scaleWidget.visible = false;
                }
            }
        }

        public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            if (isSingleUse && !isSingleUseDone)
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

                ToolOnMouseDrag(initialGridPosition, scale);
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

        private void ToolOnBeginScaling()
        {
            editor.RegisterUndo("Scale Selection");

            // store the initial position of all selected segments.
            foreach (var segment in editor.ForEachSelectedObject())
                segment.gpVector1 = segment.position;
        }

        private void ToolOnMouseDrag(float2 pivot, float2 scale)
        {
            // optionally snap the scale to grid increments.
            if (editor.isSnapping)
                scale = scale.Snap(editor.gridSnap);

            // scale the selected segments using their initial position.
            foreach (var segment in editor.ForEachSelectedObject())
                segment.position = MathEx.ScaleAroundPivot(segment.gpVector1, pivot, scale);
        }
    }
}

#endif