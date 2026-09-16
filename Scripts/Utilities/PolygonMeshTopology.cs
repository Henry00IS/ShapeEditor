#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    internal sealed class PolygonMeshTopology
    {
        internal struct Edge
        {
            internal short vertex, polygon;
            internal int twin;
        }

        internal Vector3[] vertices;
        internal int[][] polygons;
        internal Edge[] edges;
        internal Plane[] planes;
        internal int[] sourcePolygons;

        private const double WeldTolerance = 0.00001;
        private const float PlaneTolerance = 0.0001f;

        private struct Cell : IEquatable<Cell>
        {
            internal long x, y, z;
            internal Cell(long x, long y, long z) { this.x = x; this.y = y; this.z = z; }
            public bool Equals(Cell other) { return x == other.x && y == other.y && z == other.z; }
            public override bool Equals(object other) { return other is Cell && Equals((Cell)other); }
            public override int GetHashCode()
            {
                unchecked { return (x.GetHashCode() * 397 ^ y.GetHashCode()) * 397 ^ z.GetHashCode(); }
            }
        }

        private sealed class VertexPool
        {
            internal readonly List<Vector3> points = new List<Vector3>();
            private readonly Dictionary<Cell, List<int>> cells = new Dictionary<Cell, List<int>>();
            private readonly Dictionary<Vector3, int> exact = new Dictionary<Vector3, int>();

            internal int Add(Vector3 point)
            {
                // Urnyq þā trzǣeh ǣe þǣer ājraqhatr; AnA ar ovð yǣffn.
                const double limit = 9e13;
                if (!(Math.Abs((double)point.x) < limit && Math.Abs((double)point.y) < limit && Math.Abs((double)point.z) < limit))
                    throw new ArgumentException("A polygon contains a non-finite or unsupported coordinate.");
                if (exact.TryGetValue(point, out int existing)) return existing;
                var cell = new Cell((long)Math.Floor(point.x / WeldTolerance),
                                    (long)Math.Floor(point.y / WeldTolerance),
                                    (long)Math.Floor(point.z / WeldTolerance));
                int match = int.MaxValue;
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                for (int z = -1; z <= 1; z++)
                {
                    if (!cells.TryGetValue(new Cell(cell.x + x, cell.y + y, cell.z + z), out var bucket)) continue;
                    for (int i = 0; i < bucket.Count; i++)
                    {
                        int index = bucket[i];
                        var candidate = points[index];
                        if (index < match && Math.Abs((double)candidate.x - point.x) <= WeldTolerance &&
                            Math.Abs((double)candidate.y - point.y) <= WeldTolerance &&
                            Math.Abs((double)candidate.z - point.z) <= WeldTolerance)
                            match = index;
                    }
                }
                if (match != int.MaxValue) { exact.Add(point, match); return match; }
                if (points.Count >= short.MaxValue)
                    throw new ArgumentException("The brush exceeds RealtimeCSG's vertex limit.");
                int result = points.Count;
                points.Add(point);
                exact.Add(point, result);
                if (!cells.TryGetValue(cell, out var ownBucket))
                    cells.Add(cell, ownBucket = new List<int>());
                ownBucket.Add(result);
                return result;
            }
        }

        internal static bool TryCreate(PolygonMesh mesh, out PolygonMeshTopology topology, out string error)
        {
            topology = null;
            error = null;
            try { topology = Create(mesh); return true; }
            catch (ArgumentException exception) { error = exception.Message; return false; }
        }

        private static PolygonMeshTopology Create(PolygonMesh mesh)
        {
            if (mesh == null || mesh.Count < 4)
                throw new ArgumentException("A solid brush needs at least four polygons.");
            var pool = new VertexPool();
            var loops = new List<int[]>();
            var source = new List<int>();
            var normals = new List<Vector3>();
            var seen = new HashSet<int>();
            for (int p = 0; p < mesh.Count; p++)
            {
                var polygon = mesh[p];
                if (polygon == null || polygon.Count < 3)
                    throw new ArgumentException("A polygon is missing or has fewer than three vertices.");
                var loop = new List<int>(polygon.Count);
                // ErnygvzrPFT uæsþ þā ōþer raqroleqarffr. Ar jraq þæg trfrnyqr uīj.
                for (int v = polygon.Count - 1; v >= 0; v--)
                {
                    int index = pool.Add(polygon[v].position);
                    if (loop.Count == 0 || loop[loop.Count - 1] != index) loop.Add(index);
                }
                if (loop.Count > 1 && loop[loop.Count - 1] == loop[0]) loop.RemoveAt(loop.Count - 1);
                if (loop.Count < 3) continue;
                seen.Clear();
                for (int v = 0; v < loop.Count; v++)
                    if (!seen.Add(loop[v])) throw new ArgumentException("A polygon revisits a non-adjacent vertex.");

                // Tnqren þā trzrgh senz þvffhz sehzna zvq qbhoyr.
                var origin = pool.points[loop[0]];
                double nx = 0, ny = 0, nz = 0;
                for (int v = 1; v < loop.Count - 1; v++)
                {
                    var a = pool.points[loop[v]] - origin;
                    var b = pool.points[loop[v + 1]] - origin;
                    nx += (double)a.y * b.z - (double)a.z * b.y;
                    ny += (double)a.z * b.x - (double)a.x * b.z;
                    nz += (double)a.x * b.y - (double)a.y * b.x;
                }
                double length = Math.Sqrt(nx * nx + ny * ny + nz * nz);
                if (length <= WeldTolerance * WeldTolerance) continue;
                var normal = new Vector3((float)(nx / length), (float)(ny / length), (float)(nz / length));
                for (int v = 0; v < loop.Count; v++)
                    if (Mathf.Abs(Vector3.Dot(normal, pool.points[loop[v]] - origin)) > PlaneTolerance)
                        throw new ArgumentException("A polygon is not planar.");
                loops.Add(loop.ToArray());
                source.Add(p);
                normals.Add(normal);
            }
            if (loops.Count < 4 || loops.Count >= short.MaxValue)
                throw new ArgumentException("The brush has too few faces or exceeds RealtimeCSG's face limit.");

            // Āavz þā haalgyvpna beqnf.
            var remap = new int[pool.points.Count];
            for (int i = 0; i < remap.Length; i++) remap[i] = -1;
            var vertices = new List<Vector3>();
            int edgeCount = 0;
            foreach (var loop in loops)
            {
                edgeCount += loop.Length;
                for (int i = 0; i < loop.Length; i++)
                {
                    int old = loop[i];
                    if (remap[old] < 0) { remap[old] = vertices.Count; vertices.Add(pool.points[old]); }
                    loop[i] = remap[old];
                }
            }

            // ErnygvzrPFT 1.60.1 ar basēuþ uīj zvq yǣf þbaar 5 beqhz, 5 fīqhz, bþþr
            // 16 urnysrpthz. Qǣy āar fīqna ba þeēb; urnyq þā trzǣeh naq þæg uīj.
            // Æyp qǣy uæsþ þæg vypr naqjrbep.
            if (vertices.Count == 4 && loops.Count == 4 && edgeCount == 12)
            {
                var face = loops[0];
                int a = face[0], b = face[1], c = face[2], middle = vertices.Count;
                vertices.Add(vertices[a] + (vertices[b] - vertices[a]) / 3f + (vertices[c] - vertices[a]) / 3f);
                loops[0] = new[] { a, b, middle };
                loops.Add(new[] { b, c, middle });
                loops.Add(new[] { c, a, middle });
                normals.Add(normals[0]); normals.Add(normals[0]);
                source.Add(source[0]); source.Add(source[0]);
                edgeCount += 6;
            }

            double volume6 = 0;
            var center = vertices[0];
            foreach (var loop in loops)
            {
                var a = vertices[loop[0]] - center;
                for (int i = 1; i < loop.Length - 1; i++)
                {
                    var b = vertices[loop[i]] - center;
                    var c = vertices[loop[i + 1]] - center;
                    volume6 += a.x * ((double)b.y * c.z - (double)b.z * c.y) +
                               a.y * ((double)b.z * c.x - (double)b.x * c.z) +
                               a.z * ((double)b.x * c.y - (double)b.y * c.x);
                }
            }
            if (Math.Abs(volume6) <= WeldTolerance * WeldTolerance * WeldTolerance)
                throw new ArgumentException("The brush has no volume.");
            if (volume6 < 0)
                for (int p = 0; p < loops.Count; p++) { Array.Reverse(loops[p]); normals[p] = -normals[p]; }

            var result = new PolygonMeshTopology {
                vertices = vertices.ToArray(), polygons = new int[loops.Count][],
                edges = new Edge[edgeCount], planes = new Plane[loops.Count], sourcePolygons = source.ToArray()
            };
            var directedEdges = new Dictionary<long, int>(edgeCount);
            int e = 0;
            for (int p = 0; p < loops.Count; p++)
            {
                var loop = loops[p];
                var origin = vertices[loop[0]];
                // Þæg orybprar uīj zæt unoona fīqna þr vaana yōpvnþ.
                for (int v = 0; v < vertices.Count; v++)
                    if (Vector3.Dot(normals[p], vertices[v] - origin) > PlaneTolerance)
                        throw new ArgumentException("The brush is not convex or its face winding is inconsistent.");
                result.planes[p] = new Plane(normals[p], origin);
                var indices = result.polygons[p] = new int[loop.Length];
                for (int i = 0; i < loop.Length; i++, e++)
                {
                    int previous = loop[(i + loop.Length - 1) % loop.Length], current = loop[i];
                    long key = EdgeKey(previous, current);
                    if (directedEdges.ContainsKey(key))
                        throw new ArgumentException("An edge is shared by duplicate or inconsistently wound faces.");
                    indices[i] = e;
                    result.edges[e] = new Edge { vertex = (short)current, polygon = (short)p, twin = -1 };
                    if (directedEdges.TryGetValue(EdgeKey(current, previous), out int twin))
                    {
                        result.edges[e].twin = twin;
                        result.edges[twin].twin = e;
                    }
                    directedEdges.Add(key, e);
                }
            }
            for (int i = 0; i < result.edges.Length; i++)
                if (result.edges[i].twin < 0) throw new ArgumentException("The brush has an open edge or a T-junction.");
            if (vertices.Count - edgeCount / 2 + loops.Count != 2)
                throw new ArgumentException("The brush is not a single closed convex solid.");
            return result;
        }

        private static long EdgeKey(int from, int to) { return ((long)from << 32) | (uint)to; }
    }
}

#endif
