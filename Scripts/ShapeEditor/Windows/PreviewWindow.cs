#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEngine;
using GLUtilities3D = AeternumGames.ShapeEditor.GLUtilities.GLUtilities3D;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The 3D preview window.</summary>
    public class PreviewWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(360, 270);
        private GuiViewport viewport;

        public PreviewWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetCenterPosition();

            Add(new GuiWindowTitle("3D Preview Test"));

            Add(viewport = new GuiViewport(new float2(1, 21), new float2(windowSize.x - 2, windowSize.y - 22)));
            viewport.onPreRender += Viewport_OnPreRender;
            viewport.onPostRender += Viewport_OnPostRender;
            viewport.onRender3D += Viewport_OnRender3D;
            viewport.onPostRender2D += Viewport_OnPostRender2D;
            viewport.onUnusedKeyDown += Viewport_OnUnusedKeyDown;
        }

        private float2 GetCenterPosition()
        {
            return new float2(
                Mathf.RoundToInt((editor.position.width / 2f) - (windowSize.x / 2f)),
                Mathf.RoundToInt((editor.position.height / 2f) - (windowSize.y / 2f))
            );
        }

        private Mesh mesh;
        private MeshRaycast meshRaycast;
        private MeshRaycastHit hit;
        private byte materialIndex;
        private byte materialIndexUnderMouse;

        private void Viewport_OnPreRender()
        {
            if (mesh == null)
                RebuildMesh();
        }

        private void Viewport_OnPostRender()
        {
            if (hit != null)
            {
                var pos = new float2(hit.point.x, -hit.point.y);
                if (hit.normal.z.EqualsWithEpsilon5(0.0f))
                {
                    //GLUtilities.DrawCircle(1f, editor.GridPointToScreen(pos), 8f, Color.yellow);

                    var segment = editor.project.FindSegmentLineAtPosition(pos, 1f);
                    if (segment != null && editor.isLeftMousePressed)
                    {
                        segment.material = materialIndex;
                    }
                    materialIndexUnderMouse = 0;
                    if (segment != null)
                    {
                        materialIndexUnderMouse = segment.material;
                    }
                }
                else
                {
                    /* full shape detection.
                    var shape = editor.FindShapeAtGridPosition(pos);
                    if (shape != null)
                    {
                        shape.SelectAll();
                    }*/
                }
            }
        }

        private void Viewport_OnRender3D()
        {
            GLUtilities3D.DrawGuiTextured(ShapeEditorResources.Instance.shapeEditorDefaultMaterial.mainTexture, -viewport.camera.transform.position, () =>
            {
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });

            GLUtilities3D.DrawGuiLines(() =>
            {
                GL.Color(Color.green);
                GLUtilities3D.DrawLine(new float3(1f, 1f, 1f), new float3(1f, 2f, 1f));
            });

            // no need to do raycasting when the mouse isn't over the window.
            if (isMouseOver)
            {
                GLUtilities3D.DrawGuiLines(() =>
                {
                    var ray = viewport.camera.ScreenPointToRay(viewport.mousePosition);
                    var target = ray.origin + ray.direction * 2f;

                    if (meshRaycast.Raycast(ray.origin, ray.direction, out hit))
                    {
                        GL.Color(Color.blue);
                        GLUtilities3D.DrawLine(hit.point, hit.point + hit.normal * 0.25f);

                        GL.Color(Color.red);
                        GLUtilities3D.DrawLine(hit.vertex1, hit.vertex2);
                        GLUtilities3D.DrawLine(hit.vertex2, hit.vertex3);
                        GLUtilities3D.DrawLine(hit.vertex3, hit.vertex1);
                    }
                });
            }
        }

        private void Viewport_OnPostRender2D()
        {
            GLUtilities.DrawGui(() =>
            {
                var pos = viewport.camera.WorldToScreenPoint(new Vector3(1f, 1f, 1f));
                if (pos.z >= 0f)
                {
                    GLUtilities.DrawCircle(1f, new float2(pos.x, pos.y), 8f, Color.red);
                }
            });

            GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, "Drawing Material (key 1-8): " + materialIndex, new float2(10, 10));
            if (hit != null)
                GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, "Material under mouse: " + materialIndexUnderMouse, new float2(10, 30));
        }

        private void RebuildMesh()
        {
            // ensure the project data is ready.
            editor.project.Validate();
            var convexPolygons2D = editor.project.GenerateConvexPolygons();
            convexPolygons2D.CalculateBounds2D();
            mesh = MeshGenerator.CreateExtrudedPolygonMesh(convexPolygons2D, 0.5f);
            meshRaycast = new MeshRaycast(mesh);
        }

        public override void OnFocus()
        {
            RebuildMesh();
        }

        private bool Viewport_OnUnusedKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Alpha1: materialIndex = 0; return true;
                case KeyCode.Alpha2: materialIndex = 1; return true;
                case KeyCode.Alpha3: materialIndex = 2; return true;
                case KeyCode.Alpha4: materialIndex = 3; return true;
                case KeyCode.Alpha5: materialIndex = 4; return true;
                case KeyCode.Alpha6: materialIndex = 5; return true;
                case KeyCode.Alpha7: materialIndex = 6; return true;
                case KeyCode.Alpha8: materialIndex = 7; return true;
            }
            return false;
        }

        /*
        private class MeshTriangleLookupTable
        {
            /// <summary>The project containing all shapes and segments.</summary>
            private Project project;

            private ShapeEditorWindow editor;

            /// <summary>An array containing all triangles in the mesh.</summary>
            private int[] triangles;

            /// <summary>An array containing all vertices in the mesh.</summary>
            private Vector3[] vertices;

            /// <summary>Gets an array containing all triangles in the mesh.</summary>
            public int[] Triangles => triangles;

            /// <summary>Gets an array containing all vertices in the mesh.</summary>
            public Vector3[] Vertices => vertices;

            private Dictionary<int, List<Segment>> table2;

            public MeshTriangleLookupTable(ShapeEditorWindow editor, Mesh mesh, Project project)
            {
                this.project = project;
                this.editor = editor;
                triangles = mesh.triangles;
                vertices = mesh.vertices;

                CalculateLookupTable();
            }

            private void CalculateLookupTable()
            {
                // first we iterate over all triangles in the mesh. we associate a segment with
                // every triangle. We look for segment edges at the triangle center.
                Dictionary<int, Segment> triangleTable1 = new Dictionary<int, Segment>(triangles.Length);

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    var v1 = vertices[triangles[i]];
                    var v2 = vertices[triangles[i + 1]];
                    var v3 = vertices[triangles[i + 2]];
                    var plane = new Plane(v1, v2, v3);

                    if (plane.normal.z.EqualsWithEpsilon5(0.0f))
                    {
                        var center = (v1 + v2 + v3) / 3f;
                        var pos = new float2(center.x, -center.y);

                        pos = editor.GridPointToScreen(pos);
                        GLUtilities.DrawCircle(1f, pos, 8f, Color.yellow);

                        var segment = editor.FindSegmentLineAtScreenPosition(pos, 1f);
                        if (segment != null)
                        {
                            triangleTable1.Add(i, segment);
                        }
                    }
                }

                // now we iterate over every segment that has a triangle associated. we look
                // backwards and forwards to find segment lines that lie on the same plane. we also
                // associate those with the same triangle. this is important because polybool is
                // joining segments together.
                table2 = new Dictionary<int, List<Segment>>(triangleTable1.Count);

                foreach (var triangle in triangleTable1)
                {
                    var t2segments = new List<Segment>();
                    table2[triangle.Key] = t2segments;

                    t2segments.Add(triangle.Value);
                }
            }

            public bool TryGetTriangleSegments(int triangleIndex, out List<Segment> segment)
            {
                if (!table2.ContainsKey(triangleIndex))
                {
                    segment = default;
                    return false;
                }
                segment = table2[triangleIndex];
                return true;
            }
        }*/
    }
}

#endif