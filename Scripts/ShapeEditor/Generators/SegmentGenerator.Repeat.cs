#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class SegmentGenerator
    {
        // a segment generator that repeats the previous x segments y times.

        [SerializeField]
        public int repeatSegments = 2;

        [SerializeField]
        public int repeatTimes = 4;

        private void Repeat_DrawSegments()
        {
            DrawSegments(Repeat_ForEachSegmentPoint());

            // draw the direction arrow:
            var a = segment.previous.position;
            var b = segment.position;
            var cross = (float2)Vector2.Perpendicular(a - b).normalized;
            var normal = math.normalize(b - a);
            b -= normal * 0.015625f;
            a = b + (cross * 0.03125f) - (normal * 0.09375f);
            b += cross * 0.03125f;

            a = editor.GridPointToScreen(a);
            b = editor.GridPointToScreen(b);

            if (math.length(a - b) > 11f)
            {
                GL.Color(ShapeEditorWindow.segmentPivotOutlineColor);
                GLUtilities.DrawLineArrow(1.0f, a, b, 10f);
            }
        }

        private IEnumerable<float2> Repeat_ForEachSegmentPoint()
        {
            if (repeatSegments > 0)
            {
                // iterate backwards and find the first segment to begin copying.
                var begin = segment;
                for (int i = 0; i < repeatSegments; i++)
                    begin = begin.previous;

                // repeat this the specified amount of times:
                var lastPoint = segment.position;
                for (int i = 0; i < repeatTimes; i++)
                {
                    // iterate over all points from the beginning:
                    var current = begin;
                    for (int j = 0; j < repeatSegments; j++)
                    {
                        var nextLastPoint = float2.zero;
                        foreach (var point in Repeat_GetPoints(lastPoint, current))
                        {
                            yield return point;
                            nextLastPoint = point;
                        }
                        lastPoint = nextLastPoint;
                        current = current.next;
                    }
                }
            }
        }

        private IEnumerable<float2> Repeat_GetPoints(float2 previous, Segment current)
        {
            if (current.generator.type != SegmentGeneratorType.Repeat)
            {
                foreach (var point in current.generator.ForEachAdditionalSegmentPoint())
                {
                    yield return previous + (point - current.position);
                }
            }
            yield return previous + (current.next.position - current.position);
        }
    }
}

#endif