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

        private void Viewport_OnPreRender()
        {
            if (mesh == null)
                RebuildMesh();
        }

        private void Viewport_OnPostRender()
        {
            GLUtilities.DrawGui(() =>
            {
                editor.project.ClearSelection();

                if (hit != null)
                {
                    var pos = new float2(hit.point.x, -hit.point.y);
                    if (hit.normal.z.EqualsWithEpsilon5(0.0f))
                    {
                        pos = editor.GridPointToScreen(pos);
                        GLUtilities.DrawCircle(1f, pos, 8f, Color.yellow);

                        var segment = editor.FindSegmentLineAtScreenPosition(pos, 1f);
                        if (segment != null)
                        {
                            segment.selected = true;
                            segment.next.selected = true;
                        }
                    }
                    else
                    {
                        var shape = editor.FindShapeAtGridPosition(pos);
                        if (shape != null)
                        {
                            shape.SelectAll();
                        }
                    }
                }
            });
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
    }
}

#endif