#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ExternalRealtimeCSG
    {
        public static MonoBehaviour CreateBrushFromPolygonMesh(Transform parent, string brushName,
            PolygonMesh mesh, Material[] materials = null)
        {
            if (!IsAvailable() || mesh == null || mesh.Count < 4 || mesh.Count > short.MaxValue)
                return null;

            var vertices = new List<Vector3>();
            var vertexIndices = new Dictionary<Vector3, int>();
            var faces = new List<int[]>();
            var facePlanes = new List<Plane>();
            var edgeStarts = new List<int>();
            var edgeEnds = new List<int>();
            var edgeFaces = new List<int>();
            var directedEdges = new Dictionary<long, int>();
            double volume = 0;

            for (int faceIndex = 0; faceIndex < mesh.Count; faceIndex++)
            {
                var polygon = mesh[faceIndex];
                if (polygon == null || polygon.Count < 3)
                    return null;

                var loop = new List<int>(polygon.Count);
                for (int i = 0; i < polygon.Count; i++)
                {
                    var position = polygon[polygon.Count - 1 - i].position;
                    if (!IsFinite(position.x) || !IsFinite(position.y) || !IsFinite(position.z))
                        return null;

                    int vertexIndex;
                    if (!vertexIndices.TryGetValue(position, out vertexIndex))
                    {
                        if (vertices.Count >= short.MaxValue)
                            return null;
                        vertexIndex = vertices.Count;
                        vertices.Add(position);
                        vertexIndices.Add(position, vertexIndex);
                    }
                    if (loop.Count == 0 || loop[loop.Count - 1] != vertexIndex)
                        loop.Add(vertexIndex);
                }
                if (loop.Count > 1 && loop[0] == loop[loop.Count - 1])
                    loop.RemoveAt(loop.Count - 1);
                if (loop.Count < 3 || new HashSet<int>(loop).Count != loop.Count)
                    return null;
                var indices = loop.ToArray();

                var origin = vertices[indices[0]];
                double nx = 0, ny = 0, nz = 0;
                for (int i = 1; i + 1 < indices.Length; i++)
                {
                    var a = vertices[indices[i]] - origin;
                    var b = vertices[indices[i + 1]] - origin;
                    nx += (double)a.y * b.z - (double)a.z * b.y;
                    ny += (double)a.z * b.x - (double)a.x * b.z;
                    nz += (double)a.x * b.y - (double)a.y * b.x;
                }
                var length = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (length == 0 || double.IsInfinity(length) || double.IsNaN(length))
                    return null;
                var normal = new Vector3((float)(nx / length), (float)(ny / length), (float)(nz / length));
                facePlanes.Add(new Plane(normal, origin));
                volume += origin.x * nx + origin.y * ny + origin.z * nz;

                var edgeIndices = new int[indices.Length];
                for (int i = 0; i < indices.Length; i++)
                {
                    int start = indices[(i + indices.Length - 1) % indices.Length];
                    int end = indices[i];
                    long key = EdgeKey(start, end);
                    if (directedEdges.ContainsKey(key))
                        return null;
                    int edgeIndex = edgeEnds.Count;
                    directedEdges.Add(key, edgeIndex);
                    edgeIndices[i] = edgeIndex;
                    edgeStarts.Add(start);
                    edgeEnds.Add(end);
                    edgeFaces.Add(faceIndex);
                }
                faces.Add(edgeIndices);
            }

            if (volume <= 0)
                return null;

            var twins = new int[edgeEnds.Count];
            for (int i = 0; i < twins.Length; i++)
                if (!directedEdges.TryGetValue(EdgeKey(edgeEnds[i], edgeStarts[i]), out twins[i]))
                    return null;

            var controlMeshType = GetType("RealtimeCSG.Legacy.ControlMesh");
            var halfEdgeType = GetType("RealtimeCSG.Legacy.HalfEdge");
            var polygonType = GetType("RealtimeCSG.Legacy.Polygon");
            var shapeType = GetType("RealtimeCSG.Legacy.Shape");
            var surfaceType = GetType("RealtimeCSG.Legacy.Surface");
            var texGenType = GetType("RealtimeCSG.Legacy.TexGen");
            var texGenFlagsType = GetType("RealtimeCSG.Legacy.TexGenFlags");
            var settingsType = GetType("RealtimeCSG.CSGSettings");
            if (controlMeshType == null || halfEdgeType == null || polygonType == null || shapeType == null ||
                surfaceType == null || texGenType == null || texGenFlagsType == null || settingsType == null)
                return null;
            var defaultMaterial = settingsType.GetField("DefaultMaterial").GetValue(null) as Material;
            var defaultFlags = settingsType.GetField("DefaultTexGenFlags").GetValue(null);

            var edges = Array.CreateInstance(halfEdgeType, twins.Length);
            for (int i = 0; i < twins.Length; i++)
                edges.SetValue(Activator.CreateInstance(halfEdgeType,
                    new object[] { (short)edgeFaces[i], twins[i], (short)edgeEnds[i], true }), i);

            var polygons = Array.CreateInstance(polygonType, faces.Count);
            var surfaces = Array.CreateInstance(surfaceType, faces.Count);
            var texGens = Array.CreateInstance(texGenType, faces.Count);
            var texGenFlags = Array.CreateInstance(texGenFlagsType, faces.Count);
            for (int i = 0; i < faces.Count; i++)
            {
                polygons.SetValue(Activator.CreateInstance(polygonType, new object[] { faces[i], i }), i);
                var surface = Activator.CreateInstance(surfaceType);
                surfaceType.GetField("Plane").SetValue(surface, NewCSGPlane(facePlanes[i]));
                surfaceType.GetField("TexGenIndex").SetValue(surface, i);
                surfaces.SetValue(surface, i);
                var material = materials != null && i < materials.Length ? materials[i] : null;
                texGens.SetValue(Activator.CreateInstance(texGenType,
                    new object[] { material != null ? material : defaultMaterial, null }), i);
                texGenFlags.SetValue(defaultFlags, i);
            }

            var controlMesh = Activator.CreateInstance(controlMeshType);
            controlMeshType.GetField("Vertices").SetValue(controlMesh, vertices.ToArray());
            controlMeshType.GetField("Edges").SetValue(controlMesh, edges);
            controlMeshType.GetField("Polygons").SetValue(controlMesh, polygons);
            controlMeshType.GetProperty("Valid").SetValue(controlMesh, true, null);
            var shape = Activator.CreateInstance(shapeType);
            shapeType.GetField("Surfaces").SetValue(shape, surfaces);
            shapeType.GetField("TexGens").SetValue(shape, texGens);
            shapeType.GetField("TexGenFlags").SetValue(shape, texGenFlags);

            var brush = createBrushMethod.Invoke(null, new object[] { parent, brushName, controlMesh, shape });
            if (brush != null && mesh.booleanOperator == PolygonBooleanOperator.Difference)
                csgBrushOperationType.SetValue(brush, csgOperationTypeSubtractive);
            return brush as MonoBehaviour;
        }

        internal static int[] GetPolygonMaterialIndices(PolygonMesh mesh)
        {
            var indices = new int[mesh.Count];
            for (int i = 0; i < mesh.Count; i++)
                indices[i] = mesh[i].material;
            return indices;
        }

        private static long EdgeKey(int start, int end) => ((long)start << 32) | (uint)end;

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}

#endif
