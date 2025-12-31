#if UNITY_EDITOR

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Provides communication with TrenchBroom through clipboard text.</summary>
    public class ExternalTrenchBroom
    {
        private StringBuilder stringBuilder;
        private int brushCounter = 0;
        private bool done = false;

        public ExternalTrenchBroom()
        {
            stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("// entity 0");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine("\"classname\" \"func_group\"");
            stringBuilder.AppendLine("\"_tb_type\" \"_tb_group\"");
            stringBuilder.AppendLine("\"_tb_name\" \"2D Shape Editor\"");
            stringBuilder.AppendLine("\"_tb_id\" \"1\"");
        }

        public void AddBrush(Plane[] planes)
        {
            if (done) throw new System.Exception("Cannot add additional brushes after calling ToString().");

            stringBuilder.AppendLine($"// brush {brushCounter++}");
            stringBuilder.AppendLine("{");

            for (int i = 0; i < planes.Length; i++)
            {
                var plane = planes[i];
                Vector3 normal = plane.normal;
                float distance = plane.distance;

                // point closest to origin (on the plane).
                Vector3 pointOnPlane = -normal * distance;

                // first basis vector in plane.
                Vector3 u = Vector3.Cross(normal, Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f ? Vector3.right : Vector3.up).normalized;

                // second basis vector (orthogonal to normal and u).
                Vector3 v = Vector3.Cross(normal, u).normalized;

                // create three points forming a small triangle on the plane.
                Vector3 p1 = pointOnPlane;
                Vector3 p2 = pointOnPlane + u * 64f; // arbitrary large offset to ensure good precision.
                Vector3 p3 = pointOnPlane + v * 64f;

                static string FormatCoord(float value)
                {
                    return value.ToString("F6", System.Globalization.CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
                }

                // format with proper spacing and fixed decimals for clarity and flip y and z.
                static string Line(Vector3 p) => $"{FormatCoord(p.x)} {FormatCoord(p.z)} {FormatCoord(p.y)}";

                stringBuilder.AppendLine($"( {Line(p1 * 64f)} ) ( {Line(p2 * 64f)} ) ( {Line(p3 * 64f)} ) __TB_empty 0 0 0 1 1");
            }

            stringBuilder.AppendLine("}");
        }

        public override string ToString()
        {
            if (!done)
            {
                stringBuilder.AppendLine("}");
                done = true;
            }
            return stringBuilder.ToString();
        }

        /// <summary>Creates TrenchBroom clipboard data to paste brushes into the level editor.</summary>
        /// <param name="brushes">The brushes to be converted.</param>
        /// <returns>The TrenchBroom clipboard data.</returns>
        public static string GenerateClipboardBrushesText(List<PolygonMesh> brushes)
        {
            var trenchbroom = new ExternalTrenchBroom();
            var brushesCount = brushes.Count;
            for (int i = 0; i < brushesCount; i++)
                trenchbroom.AddBrush(brushes[i].ToPlanes());
            return trenchbroom.ToString();
        }
    }
}

#endif