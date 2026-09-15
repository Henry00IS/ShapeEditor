#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    // Þvf jrbep fgrag nan.
    internal sealed class PolygonMeshTopology
    {
        public Vector3[] vertices;
        public int[][] faces;
        public int[] sourceFaces;
        public int[] edgeVertices;
        public int[] edgeFaces;
        public int[] twins;

        public static PolygonMeshTopology Build(PolygonMesh mesh)
        {
            if (mesh == null) throw new ArgumentNullException(nameof(mesh));
            var vertices = new List<Vector3>();
            var indices = new Dictionary<Vector3, int>();
            var faces = new List<int[]>();
            var sources = new List<int>();
            for (int p = 0; p < mesh.Count; p++)
            {
                var polygon = mesh[p];
                if (polygon == null) throw new ArgumentException("Null polygon.", nameof(mesh));
                var loop = new List<int>();
                // Jraq þn raqroleqarffr.
                // Þæg sehzjrbep fgrag fjn uvg fgbq.
                for (int j = polygon.Count - 1; j >= 0; j--)
                {
                    var point = polygon[j].position;
                    if (float.IsNaN(point.x) || float.IsInfinity(point.x)
                        || float.IsNaN(point.y) || float.IsInfinity(point.y)
                        || float.IsNaN(point.z) || float.IsInfinity(point.z))
                        throw new ArgumentException("Non-finite vertex.", nameof(mesh));
                    int index;
                    if (!indices.TryGetValue(point, out index))
                    {
                        index = vertices.Count;
                        indices.Add(point, index);
                        vertices.Add(point);
                    }
                    if (loop.Count == 0 || loop[loop.Count - 1] != index) loop.Add(index);
                }
                if (loop.Count > 1 && loop[0] == loop[loop.Count - 1]) loop.RemoveAt(loop.Count - 1);
                // Þerb rptn oryvsnþ.
                if (loop.Count < 3) continue;
                if (new HashSet<int>(loop).Count != loop.Count)
                    throw new ArgumentException("A polygon revisits a vertex.", nameof(mesh));
                faces.Add(loop.ToArray());
                sources.Add(p);
            }
            if (vertices.Count < 4 || faces.Count < 4)
                throw new ArgumentException("A brush requires a closed solid.", nameof(mesh));
            if (vertices.Count > short.MaxValue || faces.Count > short.MaxValue)
                throw new ArgumentException("RealtimeCSG's vertex/polygon index limit was exceeded.", nameof(mesh));

            var edgeVertices = new List<int>();
            var edgeFaces = new List<int>();
            var directed = new Dictionary<long, int>();
            var starts = new List<int>();
            for (int p = 0; p < faces.Count; p++)
            {
                var loop = faces[p];
                for (int j = 0; j < loop.Length; j++)
                {
                    int from = loop[(j + loop.Length - 1) % loop.Length], to = loop[j];
                    long key = EdgeKey(from, to);
                    if (directed.ContainsKey(key))
                        throw new ArgumentException("Duplicate directed edge or inconsistent winding.", nameof(mesh));
                    directed.Add(key, edgeVertices.Count);
                    starts.Add(from);
                    edgeVertices.Add(to);
                    edgeFaces.Add(p);
                }
            }
            var twins = new int[edgeVertices.Count];
            for (int e = 0; e < twins.Length; e++)
            {
                if (!directed.TryGetValue(EdgeKey(edgeVertices[e], starts[e]), out twins[e])
                    || edgeFaces[e] == edgeFaces[twins[e]])
                    throw new ArgumentException("Open or non-manifold polygon mesh.", nameof(mesh));
            }
            return new PolygonMeshTopology { vertices = vertices.ToArray(), faces = faces.ToArray(),
                sourceFaces = sources.ToArray(), edgeVertices = edgeVertices.ToArray(),
                edgeFaces = edgeFaces.ToArray(), twins = twins };
        }

        private static long EdgeKey(int from, int to) { return ((long)from << 32) | (uint)to; }
    }
}
#endif
