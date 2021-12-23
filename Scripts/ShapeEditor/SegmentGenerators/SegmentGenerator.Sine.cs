#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that generates a sine wave between two segments.

        [SerializeField]
        public int sineDetail = 3;

        [SerializeField]
        public Pivot sinePivot1 = new Pivot();

        public void Sine_Constructor(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            var distance = math.distance(segment.position, next.position);
            var normal = math.normalize(next.position - segment.position);
            sinePivot1.position = segment.position + (normal * distance * 0.125f);
        }

        public void Sine_DrawPivots(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            float2 p1 = editor.GridPointToScreen(sinePivot1.position);

            GLUtilities.DrawSolidRectangleWithOutline(p1.x - ShapeEditorWindow.halfPivotScale, p1.y - ShapeEditorWindow.halfPivotScale, ShapeEditorWindow.pivotScale, ShapeEditorWindow.pivotScale, sinePivot1.selected ? ShapeEditorWindow.segmentPivotSelectedColor : Color.white, sinePivot1.selected ? ShapeEditorWindow.segmentPivotOutlineColor : Color.black);

            if (sinePivot1.selected)
            {
                editor.selectedSegmentsCount++;
                editor.selectedSegmentsAveragePosition += p1;
            }
        }

        public void Sine_DrawSegments(ShapeEditorWindow editor, Segment segment, Segment next)
        {
            var p1 = editor.GridPointToScreen(segment.position);
            var p2 = editor.GridPointToScreen(next.position);
            var p3 = editor.GridPointToScreen(sinePivot1.position);

            var height = math.distance(p1, p3);

            GL.Color(ShapeEditorWindow.segmentColor);

            var detail = 64;
            var frequency = -3.5f;
            var normal = math.normalize(p2 - p1);
            var cross = Vector2.Perpendicular(normal);

            var prevPos = p1;
            for (int i = 1; i <= detail; i++)
            {
                float2 pos;
                if (i == detail)
                {
                    pos = p2; // make sure it always aligns perfectly.
                }
                else
                {
                    var t = i / (float)detail;
                    pos = math.lerp(p1, p2, t);

                    var curve = math.sin(2f * Mathf.PI * t * frequency) * height;
                    pos.x += curve * cross.x;
                    pos.y += curve * cross.y;
                }

                GLUtilities.DrawLine(1.0f, prevPos, pos);
                prevPos = pos;
            }

            GL.Color(Color.red);
            GLUtilities.DrawLine(1.0f, p1, p3);
        }

        public IEnumerable<ISelectable> Sine_ForEachSelectableObject()
        {
            yield return sinePivot1;
        }
    }
}

#endif