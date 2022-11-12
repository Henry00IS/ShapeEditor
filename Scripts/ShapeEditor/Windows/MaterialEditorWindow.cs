#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.SearchService;
using UnityEngine;
using GLUtilities3D = AeternumGames.ShapeEditor.GLUtilities.GLUtilities3D;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The 3D material editor window.</summary>
    public class MaterialEditorWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(360, 290);
        private GuiButton buttonSetBrushMaterial1;
        private GuiButton buttonSetBrushMaterial2;
        private GuiButton buttonSetBrushMaterial3;
        private GuiButton buttonSetBrushMaterial4;
        private GuiButton buttonSetBrushMaterial5;
        private GuiButton buttonSetBrushMaterial6;
        private GuiButton buttonSetBrushMaterial7;
        private GuiButton buttonSetBrushMaterial8;
        private GuiViewport viewport;

        public MaterialEditorWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();

            var resources = ShapeEditorResources.Instance;
            colorWindowBackground = new Color(0.192f, 0.192f, 0.192f);
            position = GetCenterPosition();

            Add(new GuiWindowTitle("Material Editor"));

            var horizontalLayout = new GuiHorizontalLayout(this, 1, 21);
            horizontalLayout.Add(new GuiButton(resources.shapeEditorNew, 20, UserResetMaterials));
            horizontalLayout.Space(5);
            horizontalLayout.Add(buttonSetBrushMaterial1 = new GuiButton("1", 20, UserSetBrushMaterial1));
            horizontalLayout.Add(buttonSetBrushMaterial2 = new GuiButton("2", 20, UserSetBrushMaterial2));
            horizontalLayout.Add(buttonSetBrushMaterial3 = new GuiButton("3", 20, UserSetBrushMaterial3));
            horizontalLayout.Add(buttonSetBrushMaterial4 = new GuiButton("4", 20, UserSetBrushMaterial4));
            horizontalLayout.Add(buttonSetBrushMaterial5 = new GuiButton("5", 20, UserSetBrushMaterial5));
            horizontalLayout.Add(buttonSetBrushMaterial6 = new GuiButton("6", 20, UserSetBrushMaterial6));
            horizontalLayout.Add(buttonSetBrushMaterial7 = new GuiButton("7", 20, UserSetBrushMaterial7));
            horizontalLayout.Add(buttonSetBrushMaterial8 = new GuiButton("8", 20, UserSetBrushMaterial8));

            Add(viewport = new GuiViewport(new float2(1, 41), new float2(windowSize.x - 2, windowSize.y - 42)));
            viewport.onPreRender += Viewport_OnPreRender;
            viewport.onPreRender2D += Viewport_OnPreRender2D;
            viewport.onRender3D += Viewport_OnRender3D;
            viewport.onPostRender2D += Viewport_OnPostRender2D;
            viewport.onPostRender += Viewport_OnPostRender;
            viewport.onUnusedKeyDown += Viewport_OnUnusedKeyDown;
        }

        private float2 GetCenterPosition()
        {
            return new float2(
                Mathf.RoundToInt((editor.position.width / 2f) - (windowSize.x / 2f)),
                Mathf.RoundToInt((editor.position.height / 2f) - (windowSize.y / 2f))
            );
        }

        public override void OnRender()
        {
            buttonSetBrushMaterial1.isChecked = materialIndex == 0;
            buttonSetBrushMaterial2.isChecked = materialIndex == 1;
            buttonSetBrushMaterial3.isChecked = materialIndex == 2;
            buttonSetBrushMaterial4.isChecked = materialIndex == 3;
            buttonSetBrushMaterial5.isChecked = materialIndex == 4;
            buttonSetBrushMaterial6.isChecked = materialIndex == 5;
            buttonSetBrushMaterial7.isChecked = materialIndex == 6;
            buttonSetBrushMaterial8.isChecked = materialIndex == 7;

            base.OnRender();
        }

        private Mesh mesh;
        private MeshRaycast meshRaycast;
        private MeshColors meshColors;
        private MeshTriangleLookupTable lookupTable;
        private byte materialIndex;
        private byte materialIndexUnderMouse;

        [Instructions(title: "Reset all surfaces to material index number one.", description: "Resets all materials assignments in the project to the default material slot which appears as white.")]
        private void UserResetMaterials()
        {
            // for every shape in the project:
            var project = editor.project;
            var shapesCount = project.shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = project.shapes[i];

                // reset the material index.
                shape.frontMaterial = 0;
                shape.backMaterial = 0;

                // for every edge in the shape:
                var segmentCount = shape.segments.Count;
                for (int j = 0; j < segmentCount; j++)
                {
                    // reset the material index.
                    var segment = shape.segments[j];
                    segment.material = 0;
                }
            }
        }

        [Instructions(title: "Draw with material index number one.", shortcut: "1 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).\n\nThis is the default material slot and appears as white.")]
        private void UserSetBrushMaterial1() => UserSetBrushMaterial(0);

        [Instructions(title: "Draw with material index number two.", shortcut: "2 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial2() => UserSetBrushMaterial(1);

        [Instructions(title: "Draw with material index number three.", shortcut: "3 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial3() => UserSetBrushMaterial(2);

        [Instructions(title: "Draw with material index number four.", shortcut: "4 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial4() => UserSetBrushMaterial(3);

        [Instructions(title: "Draw with material index number five.", shortcut: "5 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial5() => UserSetBrushMaterial(4);

        [Instructions(title: "Draw with material index number six.", shortcut: "6 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial6() => UserSetBrushMaterial(5);

        [Instructions(title: "Draw with material index number seven.", shortcut: "7 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial7() => UserSetBrushMaterial(6);

        [Instructions(title: "Draw with material index number eight.", shortcut: "8 key", description: "You can draw this material index with the left mouse button on the mesh in the 3D viewport. Once you have extruded your shape, you can assign materials in the scene to these slots (indicated here by colored areas).")]
        private void UserSetBrushMaterial8() => UserSetBrushMaterial(7);

        private void UserSetBrushMaterial(byte materialIndex)
        {
            this.materialIndex = materialIndex;
        }

        private void RebuildMesh()
        {
            // ensure the project data is ready.
            editor.project.Validate();
            var convexPolygons2D = editor.project.GenerateConvexPolygons();
            convexPolygons2D.CalculateBounds2D();
            mesh = MeshGenerator.CreateExtrudedPolygonMesh(convexPolygons2D, 0.5f);
            meshRaycast = new MeshRaycast(mesh);
            lookupTable = new MeshTriangleLookupTable(meshRaycast.Triangles, meshRaycast.Vertices, editor.project);
            meshColors = new MeshColors(mesh);
        }

        public override void OnFocus()
        {
            RebuildMesh();
        }

        /// <summary>
        /// Called at the beginning of the control's OnRender function. This draws on the normal screen.
        /// </summary>
        private void Viewport_OnPreRender()
        {
            if (mesh == null)
                RebuildMesh();

            UpdateMeshColors();
        }

        /// <summary>
        /// Called before drawing the 3D world on the render texture with a 2D pixel matrix.
        /// </summary>
        private void Viewport_OnPreRender2D()
        {
        }

        /// <summary>
        /// Called when the 3D world is to be drawn the render texture with a 3D projection matrix.
        /// </summary>
        private void Viewport_OnRender3D()
        {
            GLUtilities3D.DrawGuiTextured(ShapeEditorResources.Instance.shapeEditorDefaultMaterial.mainTexture, viewport.camera.transform.position, () =>
            {
                Graphics.DrawMeshNow(mesh, Matrix4x4.identity);
            });

            // no need to do raycasting when the mouse isn't over the window.
            if (isMouseOver)
            {
                MeshRaycastHit hit = null;
                materialIndexUnderMouse = 255;

                GLUtilities3D.DrawGuiLines(() =>
                {
                    var ray = viewport.camera.ScreenPointToRay(viewport.mousePosition);

                    if (meshRaycast.Raycast(ray.origin, ray.direction, out hit))
                    {
                        GL.Color(Color.blue);
                        GLUtilities3D.DrawLine(hit.point, hit.point + hit.normal * 0.25f);

                        if (hit.normal.z.EqualsWithEpsilon5(0.0f))
                        {
                            GL.Color(Color.green);

                            var pos = new float2(hit.point.x, -hit.point.y);
                            var segment = editor.project.FindSegmentLineAtPosition(pos, 1f);
                            if (lookupTable.TryGetTrianglesForSegment(segment, out var triangleIndices))
                            {
                                foreach (var triangleIndex in triangleIndices)
                                {
                                    var v1 = lookupTable.Vertices[lookupTable.Triangles[triangleIndex]];
                                    var v2 = lookupTable.Vertices[lookupTable.Triangles[triangleIndex + 1]];
                                    var v3 = lookupTable.Vertices[lookupTable.Triangles[triangleIndex + 2]];

                                    GLUtilities3D.DrawLine(v1, v2);
                                    GLUtilities3D.DrawLine(v2, v3);
                                    GLUtilities3D.DrawLine(v3, v1);
                                }
                            }
                        }
                        else
                        {
                            // todo: triangle based lookup table for shapes:

                            var pos = new float2(hit.point.x, -hit.point.y);
                            var shape = editor.FindShapeAtGridPosition(pos);
                            if (shape != null)
                            {
                                GL.Color(Color.red);
                                GLUtilities3D.DrawLine(hit.vertex1, hit.vertex2);
                                GLUtilities3D.DrawLine(hit.vertex2, hit.vertex3);
                                GLUtilities3D.DrawLine(hit.vertex3, hit.vertex1);
                            }
                        }
                    }
                });

                if (hit != null)
                {
                    var pos = new float2(hit.point.x, -hit.point.y);
                    if (hit.normal.z.EqualsWithEpsilon5(0.0f))
                    {
                        if (editor.isLeftMousePressed)
                        {
                            if (lookupTable.TryGetSegmentsForTriangleIndex(hit.triangleIndex, out var segments))
                            {
                                foreach (var segment in segments)
                                {
                                    segment.material = materialIndex;
                                }
                            }
                        }

                        var segmentUnderMouse = editor.project.FindSegmentLineAtPosition(pos, 1f);
                        materialIndexUnderMouse = 0;
                        if (segmentUnderMouse != null)
                            materialIndexUnderMouse = segmentUnderMouse.material;
                    }
                    else
                    {
                        materialIndexUnderMouse = 0;

                        // todo: triangle based lookup table for shapes:
                        var shape = editor.FindShapeAtGridPosition(pos);
                        if (editor.isLeftMousePressed)
                        {
                            if (shape != null)
                            {
                                if (hit.normal.z < 0.5f)
                                {
                                    shape.frontMaterial = materialIndex;
                                }
                                else if (hit.normal.z > 0.5f)
                                {
                                    shape.backMaterial = materialIndex;
                                }
                            }
                        }

                        if (shape != null)
                        {
                            if (hit.normal.z < 0.5f)
                            {
                                materialIndexUnderMouse = shape.frontMaterial;
                            }
                            else if (hit.normal.z > 0.5f)
                            {
                                materialIndexUnderMouse = shape.backMaterial;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called after drawing the 3D world on the render texture with a 2D pixel matrix.
        /// </summary>
        private void Viewport_OnPostRender2D()
        {
            if (materialIndexUnderMouse != 255)
                GLUtilities.DrawGuiText(ShapeEditorResources.fontSegoeUI14, "Material under mouse: " + materialIndexUnderMouse, new float2(10, 10));
        }

        /// <summary>
        /// Called at the end of the control's <see cref="OnRender"/> function. This draws on the
        /// normal screen.
        /// </summary>

        private void Viewport_OnPostRender()
        {
        }

        private bool Viewport_OnUnusedKeyDown(KeyCode keyCode)
        {
            switch (keyCode)
            {
                case KeyCode.Alpha1: UserSetBrushMaterial1(); return true;
                case KeyCode.Alpha2: UserSetBrushMaterial2(); return true;
                case KeyCode.Alpha3: UserSetBrushMaterial3(); return true;
                case KeyCode.Alpha4: UserSetBrushMaterial4(); return true;
                case KeyCode.Alpha5: UserSetBrushMaterial5(); return true;
                case KeyCode.Alpha6: UserSetBrushMaterial6(); return true;
                case KeyCode.Alpha7: UserSetBrushMaterial7(); return true;
                case KeyCode.Alpha8: UserSetBrushMaterial8(); return true;
            }
            return false;
        }

        private void UpdateMeshColors()
        {
            var triangles = meshRaycast.Triangles;

            // find all triangles that are part of the edge.
            for (int k = 0; k < triangles.Length; k += 3)
            {
                Color32 color = new Color32(255, 255, 255, 255);

                if (lookupTable.TryGetSegmentsForTriangleIndex(k, out var segments))
                {
                    switch (segments[0].material)
                    {
                        case 0: color = new Color32(255, 255, 255, 255); break;
                        case 1: color = new Color32(0, 0, 255, 255); break;
                        case 2: color = new Color32(0, 255, 0, 255); break;
                        case 3: color = new Color32(255, 0, 0, 255); break;
                        case 4: color = new Color32(0, 255, 255, 255); break;
                        case 5: color = new Color32(255, 255, 0, 255); break;
                        case 6: color = new Color32(255, 0, 255, 255); break;
                        case 7: color = new Color32(64, 172, 128, 255); break;
                    }
                }

                meshColors[triangles[k]] = color;
                meshColors[triangles[k + 1]] = color;
                meshColors[triangles[k + 2]] = color;
            }
            meshColors.UpdateMesh();
        }

        private class MeshColors
        {
            /// <summary>The mesh that will be updated.</summary>
            private Mesh mesh;

            /// <summary>An array containing all triangles in the mesh.</summary>
            private int[] triangles;

            /// <summary>An array containing all vertices in the mesh.</summary>
            private Vector3[] vertices;

            /// <summary>An array containing all colors in the mesh.</summary>
            private Color32[] colors;

            /// <summary>Whether the color array has been updated.</summary>
            private bool dirty;

            /// <summary>Gets an array containing all triangles in the mesh.</summary>
            public int[] Triangles => triangles;

            /// <summary>Gets an array containing all vertices in the mesh.</summary>
            public Vector3[] Vertices => vertices;

            /// <summary>Gets or sets the color for the specified vertex index.</summary>
            /// <param name="i">The vertex index</param>
            /// <returns>The color of the vertex at the specified index.</returns>
            public Color32 this[int i]
            {
                get => colors[i];
                set
                {
                    // only mark dirty if the assigned color is different.
                    if (!colors[i].Equals(value))
                        dirty = true;
                    colors[i] = value;
                }
            }

            /// <summary>Creates a new instance and assigns white vertex colors to the mesh.</summary>
            /// <param name="mesh">The mesh to have editable vertex colors.</param>
            public MeshColors(Mesh mesh)
            {
                this.mesh = mesh;
                triangles = mesh.triangles;
                vertices = mesh.vertices;
                colors = new Color32[vertices.Length];

                var white = new Color32(255, 255, 255, 255);
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = white;
                mesh.colors32 = colors;
            }

            /// <summary>Updates the mesh colors if they were changed.</summary>
            public void UpdateMesh()
            {
                if (dirty)
                {
                    dirty = false;
                    mesh.colors32 = colors;
                }
            }
        }

        private class MeshTriangleLookupTable
        {
            /// <summary>The project containing all shapes and segments.</summary>
            private Project project;

            /// <summary>An array containing all triangles in the mesh.</summary>
            private int[] triangles;

            /// <summary>An array containing all vertices in the mesh.</summary>
            private Vector3[] vertices;

            /// <summary>Gets an array containing all triangles in the mesh.</summary>
            public int[] Triangles => triangles;

            /// <summary>Gets an array containing all vertices in the mesh.</summary>
            public Vector3[] Vertices => vertices;

            /// <summary>The internal dictionary of triangle indices for a segment.</summary>
            private Dictionary<Segment, List<int>> segmentTriangles = new Dictionary<Segment, List<int>>();

            /// <summary>The internal dictionary of segments for a triangle index.</summary>
            private Dictionary<int, List<Segment>> triangleSegments = new Dictionary<int, List<Segment>>();

            public MeshTriangleLookupTable(Mesh mesh, Project project)
            {
                this.project = project;
                triangles = mesh.triangles;
                vertices = mesh.vertices;

                CalculateSegmentLookupTables();
            }

            public MeshTriangleLookupTable(int[] triangles, Vector3[] vertices, Project project)
            {
                this.project = project;
                this.triangles = triangles;
                this.vertices = vertices;

                CalculateSegmentLookupTables();
            }

            private void CalculateSegmentLookupTables()
            {
                // for every shape in the project:
                var shapesCount = project.shapes.Count;
                for (int i = 0; i < shapesCount; i++)
                {
                    var shape = project.shapes[i];

                    // for every edge in the shape:
                    var segmentCount = shape.segments.Count;
                    for (int j = 0; j < segmentCount; j++)
                    {
                        // get the current segment.
                        var segment = shape.segments[j];

                        // find all triangles that are part of the edge.
                        for (int k = 0; k < triangles.Length; k += 3)
                        {
                            var v1 = vertices[triangles[k]];
                            var v2 = vertices[triangles[k + 1]];
                            var v3 = vertices[triangles[k + 2]];
                            var plane = new Plane(v1, v2, v3);

                            if (plane.normal == Vector3.zero)
                                continue;

                            // the triangle must not be facing front or back.
                            if (plane.normal.z.EqualsWithEpsilon5(0.0f))
                            {
                                // flatten the triangle vertices into 2D space.

                                v1.z = 0.0f;
                                v2.z = 0.0f;
                                v3.z = 0.0f;
                                var v1to2 = math.distance(v1, v2);
                                var v1to3 = math.distance(v1, v3);
                                var v2to3 = math.distance(v2, v3);
                                float2 p1;
                                float2 p2;

                                // find the triangle edge with the most 2D X&Y movement:

                                if (v1to2 > v1to3) // v1to3 out
                                {
                                    if (v1to2 > v2to3) // v2to3 out
                                    {
                                        p1 = new float2(v1.x, -v1.y);
                                        p2 = new float2(v2.x, -v2.y);
                                    }
                                    else // v1to2 out
                                    {
                                        p1 = new float2(v2.x, -v2.y);
                                        p2 = new float2(v3.x, -v3.y);
                                    }
                                }
                                else // v1to2 out
                                {
                                    if (v1to3 > v2to3) // v2to3 out
                                    {
                                        p1 = new float2(v1.x, -v1.y);
                                        p2 = new float2(v3.x, -v3.y);
                                    }
                                    else // v1to3 out
                                    {
                                        p1 = new float2(v2.x, -v2.y);
                                        p2 = new float2(v3.x, -v3.y);
                                    }
                                }

                                // associates the current triangle index with the current segment.
                                System.Action associate = () =>
                                {
                                    if (segmentTriangles.TryGetValue(segment, out var triangles))
                                        triangles.Add(k);
                                    else
                                        segmentTriangles.Add(segment, new List<int>() { k });

                                    if (triangleSegments.TryGetValue(k, out var segments))
                                        segments.Add(segment);
                                    else
                                        triangleSegments.Add(k, new List<Segment>() { segment });
                                };

                                // given two points checks whether they both lie on the triangle edge we chose.
                                System.Func<float2, float2, bool> check = (a, b)
                                    => (MathEx.IsPointOnLine2(a, p1, p2, 0.0001403269f)
                                    && MathEx.IsPointOnLine2(b, p1, p2, 0.0001403269f))
                                    || (MathEx.IsPointOnLine2(p1, a, b, 0.0001403269f)
                                    && MathEx.IsPointOnLine2(p2, a, b, 0.0001403269f));

                                // iterate over all points of the edge (including the segment generator):
                                float2 last = segment.position;
                                foreach (var point in segment.generator.ForEachAdditionalSegmentPoint())
                                {
                                    // if this segment lies on the triangle edge:
                                    if (check(last, point))
                                    {
                                        associate();
                                        break;
                                    }
                                    last = point;
                                }
                                if (check(last, segment.next.position))
                                    associate();
                            }
                        }
                    }
                }
            }

            /// <summary>Looks up all triangle indices associated with the specified segment.</summary>
            /// <param name="segment">The segment to find triangle indices for.</param>
            /// <param name="triangles">The triangle indices that lie on the edge.</param>
            /// <returns>True when the segment was found else false.</returns>
            public bool TryGetTrianglesForSegment(Segment segment, out List<int> triangles)
            {
                if (segment == null) { triangles = null; return false; }
                return segmentTriangles.TryGetValue(segment, out triangles);
            }

            /// <summary>Looks up all segments associated with the specified triangle index.</summary>
            /// <param name="triangleIndex">The triangle index to find segments for.</param>
            /// <param name="segments">The segments that lie on the triangle edge.</param>
            /// <returns>True when the triangle index was found else false.</returns>
            public bool TryGetSegmentsForTriangleIndex(int triangleIndex, out List<Segment> segments)
            {
                return triangleSegments.TryGetValue(triangleIndex, out segments);
            }
        }
    }
}

#endif