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
        public int sineDetail = 64;

        [SerializeField]
        public float sineFrequency = -3.5f;

        [SerializeField]
        public float sineGridSnapSize = 0f;

        [SerializeField]
        public Pivot sinePivot1 = new Pivot();

        private void Sine_Constructor()
        {
            var distance = math.distance(segment.position, segment.next.position);
            var normal = math.normalize(segment.next.position - segment.position);
            sinePivot1.position = segment.position + (normal * distance * 0.125f);
        }

        private void Sine_DrawPivots()
        {
            float2 p1 = editor.GridPointToScreen(sinePivot1.position);

            GLUtilities.DrawSolidRectangleWithOutline(p1.x - ShapeEditorWindow.halfPivotScale, p1.y - ShapeEditorWindow.halfPivotScale, ShapeEditorWindow.pivotScale, ShapeEditorWindow.pivotScale, sinePivot1.selected ? ShapeEditorWindow.segmentPivotSelectedColor : Color.white, sinePivot1.selected ? ShapeEditorWindow.segmentPivotOutlineColor : Color.black);

            if (sinePivot1.selected)
            {
                editor.selectedSegmentsCount++;
                editor.selectedSegmentsAveragePosition += p1;
            }
        }

        private void Sine_DrawSegments()
        {
            DrawSegments(Sine_ForEachSegmentPoint());

            var p1 = editor.GridPointToScreen(segment.position);
            var p3 = editor.GridPointToScreen(sinePivot1.position);

            GL.Color(Color.red);
            GLUtilities.DrawLine(1.0f, p1, p3);
        }

        private IEnumerable<ISelectable> Sine_ForEachSelectableObject()
        {
            yield return sinePivot1;
        }

        private IEnumerable<float2> Sine_ForEachSegmentPoint()
        {
            var p1 = segment.position;
            var p2 = segment.next.position;
            var p3 = sinePivot1.position;

            var height = math.distance(p1, p3);
            var normal = math.normalize(p2 - p1);
            var cross = Vector2.Perpendicular(normal);

            for (int i = 1; i <= sineDetail - 1; i++)
            {
                float2 pos;
                var t = i / (float)sineDetail;
                pos = math.lerp(p1, p2, t);

                var curve = math.sin(2f * Mathf.PI * t * sineFrequency) * height;
                pos.x += curve * cross.x;
                pos.y += curve * cross.y;

                yield return pos.Snap(sineGridSnapSize);
            }
        }
    }
}

#endif