#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Converts a legacy SabreCSG 2D Shape Editor V1 project into a V2 project.</summary>
    public static class ProjectV1Extensions
    {
        /// <summary>Converts the V1 project to V2.</summary>
        public static Project ToV2(this ProjectV1 oldProject)
        {
            var project = new Project();
            project.shapes.Clear();

            foreach (var oldShape in oldProject.shapes)
            {
                var shape = new Shape();
                shape.segments.Clear();
                project.shapes.Add(shape);

                foreach (var oldSegment in oldShape.segments)
                {
                    var segment = new Segment(shape, new float2(oldSegment.position.x / 8f, oldSegment.position.y / 8f));
                    shape.AddSegment(segment);

                    if (oldSegment.type == ProjectV1.SegmentType.Bezier)
                    {
                        segment.generator = new SegmentGenerator(segment, SegmentGeneratorType.Bezier);
                        segment.generator.bezierPivot1.position = new float2(oldSegment.bezierPivot1.position.x / 8f, oldSegment.bezierPivot1.position.y / 8f);
                        segment.generator.bezierPivot2.position = new float2(oldSegment.bezierPivot2.position.x / 8f, oldSegment.bezierPivot2.position.y / 8f);
                    }
                }
            }

            return project;
        }
    }
}

#endif