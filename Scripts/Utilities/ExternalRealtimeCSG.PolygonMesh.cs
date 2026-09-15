#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ExternalRealtimeCSG
    {
        private sealed class MeshBindings
        {
            public readonly Type control = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.ControlMesh");
            public readonly Type shape = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.Shape");
            public readonly Type edge = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.HalfEdge");
            public readonly Type polygon = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.Polygon");
            public readonly Type surface = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.Surface");
            public readonly Type texGen = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.TexGen");
            public readonly Type flags = ExternalRealtimeCSG.GetType("RealtimeCSG.Legacy.TexGenFlags");
            public readonly FieldInfo vertices, edges, polygons, surfaces, texGens, texGenFlags;
            public readonly FieldInfo plane, texIndex, scale, renderMaterial;
            public readonly ConstructorInfo edgeConstructor, polygonConstructor;
            public readonly MethodInfo calcPlane, initializeShape, validate, create;
            public readonly object defaultFlags;

            public MeshBindings()
            {
                const BindingFlags methods = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                vertices = control.GetField("Vertices"); edges = control.GetField("Edges");
                polygons = control.GetField("Polygons"); surfaces = shape.GetField("Surfaces");
                texGens = shape.GetField("TexGens"); texGenFlags = shape.GetField("TexGenFlags");
                plane = surface.GetField("Plane"); texIndex = surface.GetField("TexGenIndex");
                scale = texGen.GetField("Scale"); renderMaterial = texGen.GetField("RenderMaterial");
                edgeConstructor = edge.GetConstructor(new[] { typeof(short), typeof(int), typeof(short), typeof(bool) });
                polygonConstructor = polygon.GetConstructor(new[] { typeof(int[]), typeof(int) });
                calcPlane = ExternalRealtimeCSG.GetType("RealtimeCSG.GeometryUtility").GetMethod("CalcPolygonPlane", methods,
                    null, new[] { control, typeof(short) }, null);
                initializeShape = ExternalRealtimeCSG.GetType("RealtimeCSG.ShapeUtility").GetMethod("EnsureInitialized", methods,
                    null, new[] { shape }, null);
                validate = ExternalRealtimeCSG.GetType("RealtimeCSG.ControlMeshUtility").GetMethod("Validate", methods,
                    null, new[] { control, shape }, null);
                create = brushFactory.GetMethod("CreateBrush", methods, null,
                    new[] { typeof(Transform), typeof(string), control, shape }, null);
                defaultFlags = ExternalRealtimeCSG.GetType("RealtimeCSG.CSGSettings").GetField("DefaultTexGenFlags").GetValue(null);
            }
        }

        private static MeshBindings meshBindings;

        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName,
                                                               PolygonMesh mesh, Material[] materials = null)
        {
            if (!IsAvailable()) return null;
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            if (materials != null && materials.Length != mesh.Count)
                throw new ArgumentException("Supply one material per source polygon.", nameof(materials));
            var topology = PolygonMeshTopology.Build(mesh);
            var b = meshBindings ?? (meshBindings = new MeshBindings());
            var control = Activator.CreateInstance(b.control);
            var shape = Activator.CreateInstance(b.shape);
            var edges = Array.CreateInstance(b.edge, topology.twins.Length);
            var polygons = Array.CreateInstance(b.polygon, topology.faces.Length);
            for (int e = 0; e < edges.Length; e++)
                edges.SetValue(b.edgeConstructor.Invoke(new object[] { (short)topology.edgeFaces[e],
                    topology.twins[e], (short)topology.edgeVertices[e], true }), e);
            int offset = 0;
            for (int p = 0; p < polygons.Length; p++)
            {
                var faceEdges = new int[topology.faces[p].Length];
                for (int e = 0; e < faceEdges.Length; e++) faceEdges[e] = offset++;
                polygons.SetValue(b.polygonConstructor.Invoke(new object[] { faceEdges, p }), p);
            }
            b.vertices.SetValue(control, topology.vertices);
            b.edges.SetValue(control, edges); b.polygons.SetValue(control, polygons);
            var surfaces = Array.CreateInstance(b.surface, polygons.Length);
            var texGens = Array.CreateInstance(b.texGen, polygons.Length);
            var flags = Array.CreateInstance(b.flags, polygons.Length);
            for (int p = 0; p < polygons.Length; p++)
            {
                var surface = Activator.CreateInstance(b.surface);
                b.texIndex.SetValue(surface, p);
                b.plane.SetValue(surface, b.calcPlane.Invoke(null, new object[] { control, (short)p }));
                surfaces.SetValue(surface, p);
                var tex = Activator.CreateInstance(b.texGen);
                b.scale.SetValue(tex, Vector2.one);
                var material = materials == null ? null : materials[topology.sourceFaces[p]];
                b.renderMaterial.SetValue(tex, material ? material : WallMaterial);
                texGens.SetValue(tex, p); flags.SetValue(b.defaultFlags, p);
            }
            b.surfaces.SetValue(shape, surfaces); b.texGens.SetValue(shape, texGens);
            b.texGenFlags.SetValue(shape, flags);
            b.initializeShape.Invoke(null, new[] { shape });
            if (!(bool)b.validate.Invoke(null, new[] { control, shape }))
                throw new ArgumentException("RealtimeCSG rejected the polygon mesh.", nameof(mesh));
            b.control.GetProperty("Valid").SetValue(control, true);
            var brush = (MonoBehaviour)b.create.Invoke(null, new object[] { parent, brushName, control, shape });
            if (brush && mesh.booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);
            return brush;
        }
    }
}
#endif
