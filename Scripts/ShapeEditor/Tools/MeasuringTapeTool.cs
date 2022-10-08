
#if UNITY_EDITOR

using System;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class MeasuringTapeTool : Tool
    {
        private readonly Pivot startPivot = new Pivot();
        private readonly Pivot endPivot = new Pivot();
        private float measuringLength;
        private bool proc;
        
        private static readonly Color pivotColor = new Color(1.0f, 0.5f, 0.0f);

        public override void OnActivate()
        {
            SetPivotPosition(startPivot);
            SetPivotPosition(endPivot);
            proc = false;
        }

        public override void OnRender()
        {
            float2 p1 = editor.GridPointToScreen(startPivot.position);
            float2 p2 = editor.GridPointToScreen(endPivot.position);

            GLUtilities.DrawGui((() => 
            {
                GL.Color(Color.white);
                GLUtilities.DrawLine(1f, p1, p2);
                GL.Color(Color.red);
                GLUtilities.DrawDottedLine(1f, p1, p2, 16f);
                
                GLUtilities.DrawCircle(2f, p1, 6f, pivotColor, 4);
                GLUtilities.DrawCircle(2f, p2, 6f, pivotColor, 4);
            }));

            if (!proc) return;

            string distance = measuringLength.ToString("0.00000", CultureInfo.InvariantCulture).TrimEnd('0', '.') + "m";
            if (distance == "m") return;

            float2 mid = (p1 + p2) / 2f;
            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, distance, mid);
        }

        public override void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            if (!proc)
            {
                SetPivotPosition(startPivot);
                SetPivotPosition(endPivot);
            }
        }

        public override void OnMouseDown(int button)
        {
            if (button == 0)
            {
                SetPivotPosition(startPivot);
                SetPivotPosition(endPivot);
                proc = true;
                measuringLength = 0f;
            }
        }

        public override void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (button == 0 && proc)
            {
                SetPivotPosition(endPivot);
                measuringLength = math.distance(startPivot.position, endPivot.position);
            }
        }

        public override void OnMouseUp(int button)
        {
            if (button == 0)
            {
                SetPivotPosition(endPivot);

                // disable process if user clicked without dragging
                if (measuringLength == 0f)
                    proc = false;
            }
        }

        private void SetPivotPosition(Pivot pivot)
        {
            pivot.position = editor.isSnapping ?
                editor.mouseGridPosition.Snap(editor.gridSnap) : editor.mouseGridPosition;
        }
    }
}

#endif