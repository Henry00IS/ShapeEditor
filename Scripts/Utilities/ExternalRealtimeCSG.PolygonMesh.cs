#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ExternalRealtimeCSG
    {
        private sealed class PolygonMeshApi
        {
            internal readonly Type controlMesh = RequireType("RealtimeCSG.Legacy.ControlMesh");
            internal readonly Type shape = RequireType("RealtimeCSG.Legacy.Shape");
            internal readonly Type halfEdge = RequireType("RealtimeCSG.Legacy.HalfEdge");
            internal readonly Type polygon = RequireType("RealtimeCSG.Legacy.Polygon");
            internal readonly Type surface = RequireType("RealtimeCSG.Legacy.Surface");
            internal readonly Type texGen = RequireType("RealtimeCSG.Legacy.TexGen");
            internal readonly Type texGenFlags = RequireType("RealtimeCSG.Legacy.TexGenFlags");
            internal readonly ConstructorInfo edgeConstructor, polygonConstructor, texGenConstructor;
            internal readonly FieldInfo vertices, edges, polygons, surfaces, texGens, flags;
            internal readonly FieldInfo plane, tangent, binormal, texGenIndex, defaultMaterial, defaultFlags;
            internal readonly PropertyInfo valid;
            internal readonly MethodInfo validate, calculateTangents;

            private static Type RequireType(string name)
            {
                var type = ExternalRealtimeCSG.GetType(name);
                if (type == null) throw new MissingMemberException("Missing RealtimeCSG type: " + name);
                return type;
            }

            internal PolygonMeshApi()
            {
                edgeConstructor = halfEdge.GetConstructor(new[] { typeof(short), typeof(int), typeof(short), typeof(bool) });
                polygonConstructor = polygon.GetConstructor(new[] { typeof(int[]), typeof(int) });
                texGenConstructor = texGen.GetConstructor(new[] { typeof(Material), typeof(PhysicMaterial) });
                vertices = controlMesh.GetField("Vertices");
                edges = controlMesh.GetField("Edges");
                polygons = controlMesh.GetField("Polygons");
                valid = controlMesh.GetProperty("Valid");
                surfaces = shape.GetField("Surfaces");
                texGens = shape.GetField("TexGens");
                flags = shape.GetField("TexGenFlags");
                plane = surface.GetField("Plane");
                tangent = surface.GetField("Tangent");
                binormal = surface.GetField("BiNormal");
                texGenIndex = surface.GetField("TexGenIndex");
                var settings = RequireType("RealtimeCSG.CSGSettings");
                defaultMaterial = settings.GetField("DefaultMaterial");
                defaultFlags = settings.GetField("DefaultTexGenFlags");
                validate = RequireType("RealtimeCSG.ControlMeshUtility").GetMethod("Validate", BindingFlags.Public | BindingFlags.Static,
                    null, new[] { controlMesh, shape }, null);
                calculateTangents = RequireType("RealtimeCSG.GeometryUtility").GetMethod("CalculateTangents", BindingFlags.Public | BindingFlags.Static,
                    null, new[] { typeof(Vector3), typeof(Vector3).MakeByRefType(), typeof(Vector3).MakeByRefType() }, null);
                if (edgeConstructor == null || polygonConstructor == null || texGenConstructor == null || vertices == null ||
                    edges == null || polygons == null || valid == null || surfaces == null || texGens == null || flags == null ||
                    plane == null || tangent == null || binormal == null || texGenIndex == null || defaultMaterial == null ||
                    defaultFlags == null || validate == null || calculateTangents == null)
                    throw new MissingMemberException("Unsupported RealtimeCSG polygon mesh API.");
            }
        }

        private static PolygonMeshApi polygonMeshApi;

        /// <summary>Jlep þæg uīj bs þǣen fīqran trþēbqarffr; ar jraq þæg trfrnyqr uīj.</summary>
        /// <param name="materials">Naqjrbep or þǣen trfrnyqen fīqran raqroleqarffr, ēnp þǣen sbeqjvaraen.</param>
        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName, PolygonMesh mesh, Material[] materials = null)
        {
            if (!IsAvailable()) return null;
            if (!PolygonMeshTopology.TryCreate(mesh, out var topology, out var error))
            {
                Debug.LogWarning("Cannot create RealtimeCSG brush '" + brushName + "': " + error);
                return null;
            }

            // Urnyq þā trgāpahatn; eǣq þæg trpberar naqjrbep æg ǣyphz uījr.
            if (polygonMeshApi == null)
            {
                try { polygonMeshApi = new PolygonMeshApi(); }
                catch (MissingMemberException exception)
                {
                    Debug.LogWarning("Cannot create RealtimeCSG brush: " + exception.Message);
                    return null;
                }
            }
            var api = polygonMeshApi;
            var controlMesh = Activator.CreateInstance(api.controlMesh);
            var shape = Activator.CreateInstance(api.shape);
            var edges = Array.CreateInstance(api.halfEdge, topology.edges.Length);
            var edgeArgs = new object[4];
            edgeArgs[3] = true;
            for (int i = 0; i < topology.edges.Length; i++)
            {
                var edge = topology.edges[i];
                edgeArgs[0] = edge.polygon; edgeArgs[1] = edge.twin; edgeArgs[2] = edge.vertex;
                edges.SetValue(api.edgeConstructor.Invoke(edgeArgs), i);
            }
            int count = topology.polygons.Length;
            var polygons = Array.CreateInstance(api.polygon, count);
            var surfaces = Array.CreateInstance(api.surface, count);
            var texGens = Array.CreateInstance(api.texGen, count);
            var flags = Array.CreateInstance(api.texGenFlags, count);
            var fallback = (Material)api.defaultMaterial.GetValue(null);
            var defaultFlags = api.defaultFlags.GetValue(null);
            var polygonArgs = new object[2];
            var texGenArgs = new object[2];
            var tangentArgs = new object[3];
            for (int p = 0; p < count; p++)
            {
                polygonArgs[0] = topology.polygons[p]; polygonArgs[1] = p;
                polygons.SetValue(api.polygonConstructor.Invoke(polygonArgs), p);
                object surface = Activator.CreateInstance(api.surface);
                api.plane.SetValue(surface, NewCSGPlane(topology.planes[p]));
                tangentArgs[0] = topology.planes[p].normal;
                api.calculateTangents.Invoke(null, tangentArgs);
                api.tangent.SetValue(surface, tangentArgs[1]);
                api.binormal.SetValue(surface, tangentArgs[2]);
                api.texGenIndex.SetValue(surface, p);
                surfaces.SetValue(surface, p);
                int source = topology.sourcePolygons[p];
                var material = materials != null && source < materials.Length ? materials[source] : null;
                texGenArgs[0] = material ? material : fallback;
                texGens.SetValue(api.texGenConstructor.Invoke(texGenArgs), p);
                flags.SetValue(defaultFlags, p);
            }
            api.vertices.SetValue(controlMesh, topology.vertices);
            api.edges.SetValue(controlMesh, edges);
            api.polygons.SetValue(controlMesh, polygons);
            api.surfaces.SetValue(shape, surfaces);
            api.texGens.SetValue(shape, texGens);
            api.flags.SetValue(shape, flags);
            if (!(bool)api.validate.Invoke(null, new[] { controlMesh, shape }))
            {
                Debug.LogWarning("Cannot create RealtimeCSG brush '" + brushName + "': control mesh validation failed.");
                return null;
            }
            api.valid.SetValue(controlMesh, true);
            var brush = (MonoBehaviour)createBrushMethod.Invoke(null, new[] { parent, (object)brushName, controlMesh, shape });
            if (brush != null && mesh.booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);
            return brush;
        }
    }
}

#endif
