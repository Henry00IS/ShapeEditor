#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class ExtrudeTool : TranslateTool
    {
        public override void OnActivate()
        {
            // find all of the selected edges.
            var selectedSegments = new List<Segment>(editor.selectedSegmentsCount / 2);

            // for every shape in the project:
            var shapesCount = editor.project.shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = editor.project.shapes[i];

                // for every segment in the project:
                var segmentsCount = shape.segments.Count;
                for (int j = 0; j < segmentsCount; j++)
                {
                    var segment = shape.segments[j];
                    var next = segment.next;

                    // if the entire edge is selected and not a modifier:
                    if (segment.selected && next.selected && segment.generator.type == SegmentGeneratorType.Linear)
                        selectedSegments.Add(segment);
                }
            }

            // clear the active selection.
            editor.project.ClearSelection();

            // disable the translate undo operation.
            registerTranslateUndoOperation = false;
            editor.RegisterUndo("Extrude Selection");

            // extrude the selected edges.
            foreach (var segment in selectedSegments)
                ExtrudeSegment(segment);

            // the base functionality of the translate tool handles the rest.
            base.OnActivate();
        }

        private void ExtrudeSegment(Segment segment)
        {
            // duplicate the segments.
            var shape = segment.shape;

            var position1 = segment.position;
            var position2 = segment.next.position;

            var extrudedSegment = new Segment(shape, position1);
            extrudedSegment.selected = true;
            shape.InsertSegmentBefore(segment.next, extrudedSegment);

            extrudedSegment = new Segment(shape, position2);
            extrudedSegment.selected = true;
            shape.InsertSegmentBefore(segment.next.next, extrudedSegment);
        }

        protected override void ToolOnCancel()
        {
            // undo translation.
            foreach (var segment in editor.ForEachSelectedObject())
                segment.position = segment.gpVector1;

            // exit the tool.
            isSingleUseDone = true;
            editor.SwitchTool(parent);
        }
    }
}

#endif