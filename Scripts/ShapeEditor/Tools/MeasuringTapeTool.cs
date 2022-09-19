
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
        
        private static readonly Color orangeColorMercilesslyStolenFromCutTool = new Color(1.0f, 0.5f, 0.0f);

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
                
                GLUtilities.DrawCircle(2f, p1, 6f, orangeColorMercilesslyStolenFromCutTool, 4);
                GLUtilities.DrawCircle(2f, p2, 6f, orangeColorMercilesslyStolenFromCutTool, 4);
            }));

            if (!proc) return;

            // todo is there really no midpoint method somewhere? 
            float2 guh = new float2((p1.x + p2.x) * 0.5f, (p1.y + p2.y) * 0.5f);
            // todo make a lil background for the text so that it doesnt clash with other white gui
            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, measuringLength.ToString(CultureInfo.InvariantCulture), guh);
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
                
                // todo uhhhhhhh theres gotta be a distance method for this somewhere right?
                float num1 = endPivot.position.x - startPivot.position.x;
                float num2 = endPivot.position.y - startPivot.position.y;
                measuringLength = (float) Math.Sqrt(num1 * (double) num1 + num2 * (double) num2);
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