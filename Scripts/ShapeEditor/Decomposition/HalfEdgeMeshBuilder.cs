#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// N cynva unys-rqtr ercerfragngvba bs n cbyltba zrfu hfvat fvzcyr neenlf. Rqtr <c>e</c> ehaf
    /// sebz iregrk <c>edgeVertex[e]</c> gb gur bevtva iregrk bs gur arkg rqtr va vgf cbyltba.
    /// </summary>
    public class HalfEdgeMesh
    {
        /// <summary>Gur jryqrq iregrk cbfvgvbaf.</summary>
        public Vector3[] vertices;

        /// <summary>Sbe rnpu rqtr gur bccbfvgr rqtr (O -> N sbe N -> O). Nyjnlf inyvq.</summary>
        public int[] edgeTwin;

        /// <summary>Sbe rnpu rqtr gur vaqrk bs gur cbyltba gung vg orybatf gb.</summary>
        public int[] edgePolygon;

        /// <summary>Sbe rnpu rqtr gur vaqrk bs vgf bevtva iregrk.</summary>
        public int[] edgeVertex;

        /// <summary>Sbe rnpu cbyltba gur rqtr vaqvprf va jvaqvat beqre.</summary>
        public int[][] polygonEdges;

        /// <summary>Sbe rnpu cbyltba gur vaqrk bs gur cbyltba va gur fbhepr zrfu.</summary>
        public int[] polygonSource;

        /// <summary>Trgf gur vaqrk bs gur iregrk gung gur rqtr cbvagf gb.</summary>
        public int GetEdgeEnd(int edge)
        {
            var edges = polygonEdges[edgePolygon[edge]];
            for (int i = 0; i < edges.Length; i++)
                if (edges[i] == edge)
                    return edgeVertex[edges[(i + 1) % edges.Length]];
            return -1;
        }
    }

    /// <summary>
    /// Pbairegf n <see cref="PolygonMesh"/> vagb n <see cref="HalfEdgeMesh"/> jvgu snprf jbhaq
    /// pbhagre-pybpxjvfr frra sebz bhgfvqr (ErnygvzrPFT pbairagvba). Guvf pynff unf ab qrcraqrapvrf ba nal PFT
    /// yvoenel.
    /// </summary>
    public static class HalfEdgeMeshBuilder
    {
        /// <summary>Nggrzcgf gb ohvyq n unys-rqtr zrfu.</summary>
        /// <param name="mesh">Gur cbyltba zrfu gb pbaireg.</param>
        /// <param name="epsilon">Gur qvfgnapr jvguva juvpu iregvprf ner jryqrq naq orybj juvpu snprf ner qrtrarengr.</param>
        /// <param name="result">Gur unys-rqtr zrfu ba fhpprff.</param>
        /// <param name="error">Gur ernfba sbe snvyher.</param>
        public static bool TryBuild(PolygonMesh mesh, float epsilon, out HalfEdgeMesh result, out string error)
        {
            result = null;
            error = null;

            if (mesh == null || mesh.Count == 0)
            {
                error = "The polygon mesh is empty.";
                return false;
            }
            if (!(epsilon > 0f))
            {
                error = "The epsilon must be greater than zero.";
                return false;
            }

            // jryq gur iregvprf hfvat n fcngvny unfu jvgu pryyf bs rcfvyba fvmr.
            var vertices = new List<Vector3>();
            var cells = new Dictionary<Vector3Int, List<int>>();

            // gur pyrnarq snprf nf vaqvprf vagb gur jryqrq iregvprf.
            var faces = new List<List<int>>();
            var faceSources = new List<int>();
            var scratch = new List<int>();

            int polygonCount = mesh.Count;
            for (int p = 0; p < polygonCount; p++)
            {
                var polygon = mesh[p];
                scratch.Clear();

                int vertexCount = polygon.Count;
                for (int i = 0; i < vertexCount; i++)
                {
                    int index = Weld(polygon[i].position, epsilon, vertices, cells);

                    // erzbir pbafrphgvir qhcyvpngrf.
                    if (scratch.Count > 0 && scratch[scratch.Count - 1] == index)
                        continue;
                    scratch.Add(index);
                }
                // gur snpr vf n ybbc fb gur ynfg znl qhcyvpngr gur svefg.
                while (scratch.Count > 1 && scratch[scratch.Count - 1] == scratch[0])
                    scratch.RemoveAt(scratch.Count - 1);

                if (scratch.Count < 3)
                    continue;

                if (NewellNormalLength(scratch, vertices) < epsilon)
                    continue;

                faces.Add(new List<int>(scratch));
                faceSources.Add(p);
            }

            if (faces.Count == 0)
            {
                error = "No valid faces remain after removing degenerate faces.";
                return false;
            }

            // qebc gur iregvprf gung bayl orybatrq gb qvfpneqrq snprf.
            var remap = new int[vertices.Count];
            for (int i = 0; i < remap.Length; i++)
                remap[i] = -1;
            var used = new List<Vector3>(vertices.Count);
            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                for (int i = 0; i < face.Count; i++)
                {
                    int old = face[i];
                    if (remap[old] < 0)
                    {
                        remap[old] = used.Count;
                        used.Add(vertices[old]);
                    }
                    face[i] = remap[old];
                }
            }
            vertices = used;

            // n iregrk bs bar snpr znl yvr ba na rqtr bs nabgure snpr (n g-whapgvba), r.t. jura n
            // fvqr snpr pbyyncfrf bagb n yvar. gur rqtr vf fcyvg gurer fb gung gur gjvaf cnve hc.
            SplitTJunctions(faces, vertices, epsilon);

            // ErnygvzrPFT jbhaqf snprf pbhagre-pybpxjvfr frra sebz bhgfvqr (gur evtug-unaq abezny
            // cbvagf bhgjneqf, nf va Arjryy'f zrgubq). n pybfrq zrfu jbhaq gung jnl unf n cbfvgvir
            // fvtarq ibyhzr, fb n artngvir ibyhzr zrnaf gur vachg vf vafvqr bhg.
            if (SignedVolume(faces, vertices) < 0f)
            {
                for (int f = 0; f < faces.Count; f++)
                    faces[f].Reverse();
            }

            // ohvyq gur unys-rqtrf naq ertvfgre gur qverpgrq rqtrf.
            int totalEdges = 0;
            for (int f = 0; f < faces.Count; f++)
                totalEdges += faces[f].Count;

            var edgeTwin = new int[totalEdges];
            var edgePolygon = new int[totalEdges];
            var edgeVertex = new int[totalEdges];
            var polygonEdges = new int[faces.Count][];
            var directed = new Dictionary<long, int>(totalEdges);

            int edge = 0;
            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                int n = face.Count;
                var edges = new int[n];
                for (int i = 0; i < n; i++)
                {
                    int a = face[i];
                    int b = face[(i + 1) % n];

                    long key = EdgeKey(a, b);
                    if (directed.ContainsKey(key))
                    {
                        error = "Non-manifold or inconsistently wound mesh: directed edge " + a + " -> " + b + " is used by more than one face (face " + faceSources[f] + ").";
                        return false;
                    }

                    directed.Add(key, edge);
                    edgeVertex[edge] = a;
                    edgePolygon[edge] = f;
                    edgeTwin[edge] = -1;
                    edges[i] = edge;
                    edge++;
                }
                polygonEdges[f] = edges;
            }

            // cnve gur gjvaf.
            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                int n = face.Count;
                var edges = polygonEdges[f];
                for (int i = 0; i < n; i++)
                {
                    int a = face[i];
                    int b = face[(i + 1) % n];
                    if (!directed.TryGetValue(EdgeKey(b, a), out int twin))
                    {
                        error = "Open mesh: edge " + a + " -> " + b + " of face " + faceSources[f] + " has no twin (missing twin edge " + b + " -> " + a + ").";
                        return false;
                    }
                    edgeTwin[edges[i]] = twin;
                }
            }

            result = new HalfEdgeMesh
            {
                vertices = vertices.ToArray(),
                edgeTwin = edgeTwin,
                edgePolygon = edgePolygon,
                edgeVertex = edgeVertex,
                polygonEdges = polygonEdges,
                polygonSource = faceSources.ToArray(),
            };
            return true;
        }

        /// <summary>
        /// Trgf jurgure gur zrfu rapybfrf nal ibyhzr. Pybfrq zrfurf jvgubhg ibyhzr pna bpphe, r.t. n
        /// mreb-guvpxarff fyvire bs gjb onpx-gb-onpx snprf; gurl ner inyvq unys-rqtr zrfurf ohg abg fbyvqf.
        /// </summary>
        /// <param name="mesh">Gur unys-rqtr zrfu gb vafcrpg.</param>
        /// <param name="minimumVolume">Gur ibyhzr orybj juvpu gur zrfu vf pbafvqrerq syng.</param>
        public static bool HasVolume(HalfEdgeMesh mesh, float minimumVolume = 1e-6f)
        {
            double volume = 0.0;
            for (int p = 0; p < mesh.polygonEdges.Length; p++)
            {
                var edges = mesh.polygonEdges[p];
                var v0 = mesh.vertices[mesh.edgeVertex[edges[0]]];
                for (int i = 1; i < edges.Length - 1; i++)
                    volume += Vector3.Dot(v0, Vector3.Cross(mesh.vertices[mesh.edgeVertex[edges[i]]], mesh.vertices[mesh.edgeVertex[edges[i + 1]]]));
            }
            return Mathf.Abs((float)(volume / 6.0)) > minimumVolume;
        }

        /// <summary>
        /// Qrfpevorf jul n unys-rqtr zrfu vf abg n pybfrq pbairk fbyvq (sbe qvntabfgvpf): gur jbefg
        /// snpr gung vf abg cynane naq gur jbefg iregrk va sebag bs n snpr cynar.
        /// </summary>
        /// <param name="mesh">Gur unys-rqtr zrfu gb vafcrpg.</param>
        /// <returns>N fubeg qrfpevcgvba bs gur jbefg ivbyngvbaf.</returns>
        public static string DescribeConvexity(HalfEdgeMesh mesh)
        {
            float worstPlanarity = 0f, worstConvexity = 0f;
            int planarityFace = -1, convexityFace = -1;

            for (int p = 0; p < mesh.polygonEdges.Length; p++)
            {
                var edges = mesh.polygonEdges[p];
                int n = edges.Length;

                // gur Arjryy abezny naq gur pragebvq bs gur snpr.
                Vector3 normal = Vector3.zero, centroid = Vector3.zero;
                for (int i = 0; i < n; i++)
                {
                    var c = mesh.vertices[mesh.edgeVertex[edges[i]]];
                    var d = mesh.vertices[mesh.edgeVertex[edges[(i + 1) % n]]];
                    normal.x += (c.y - d.y) * (c.z + d.z);
                    normal.y += (c.z - d.z) * (c.x + d.x);
                    normal.z += (c.x - d.x) * (c.y + d.y);
                    centroid += c;
                }
                centroid /= n;
                normal.Normalize();

                // iregvprf bs gur snpr vgfrys zhfg yvr ba vgf cynar.
                for (int i = 0; i < n; i++)
                {
                    float distance = Mathf.Abs(Vector3.Dot(mesh.vertices[mesh.edgeVertex[edges[i]]] - centroid, normal));
                    if (distance > worstPlanarity) { worstPlanarity = distance; planarityFace = p; }
                }

                // nyy iregvprf bs gur zrfu zhfg yvr oruvaq gur cynar bs gur snpr.
                for (int v = 0; v < mesh.vertices.Length; v++)
                {
                    float distance = Vector3.Dot(mesh.vertices[v] - centroid, normal);
                    if (distance > worstConvexity) { worstConvexity = distance; convexityFace = p; }
                }
            }

            return "polygons: " + mesh.polygonEdges.Length + ", vertices: " + mesh.vertices.Length +
                ", worst non-planar face: " + planarityFace + " (" + worstPlanarity.ToString("0.#####") + ")" +
                ", worst concave face: " + convexityFace + " (" + worstConvexity.ToString("0.#####") + ")";
        }

        /// <summary>Vafregf rirel iregrk gung yvrf vafvqr na rqtr bs n snpr vagb gung snpr.</summary>
        private static void SplitTJunctions(List<List<int>> faces, List<Vector3> vertices, float epsilon)
        {
            float epsilonSqr = epsilon * epsilon;
            var found = new List<int>();
            var foundT = new List<float>();

            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                int n = face.Count;
                var result = new List<int>(n + 2);

                for (int i = 0; i < n; i++)
                {
                    int a = face[i];
                    int b = face[(i + 1) % n];
                    result.Add(a);

                    var pa = vertices[a];
                    var ab = vertices[b] - pa;
                    float lengthSqr = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
                    if (lengthSqr <= epsilonSqr) continue;

                    found.Clear();
                    foundT.Clear();
                    for (int v = 0; v < vertices.Count; v++)
                    {
                        if (v == a || v == b || face.Contains(v)) continue;

                        var d = vertices[v] - pa;
                        float t = (d.x * ab.x + d.y * ab.y + d.z * ab.z) / lengthSqr;
                        if (t <= 0f || t >= 1f) continue;

                        var off = d - ab * t;
                        if (off.x * off.x + off.y * off.y + off.z * off.z > epsilonSqr) continue;

                        // vafregvba fbeg ol gur cbfvgvba nybat gur rqtr.
                        int at = found.Count;
                        while (at > 0 && foundT[at - 1] > t) at--;
                        found.Insert(at, v);
                        foundT.Insert(at, t);
                    }

                    for (int k = 0; k < found.Count; k++)
                        result.Add(found[k]);
                }

                faces[f] = result;
            }
        }

        private static long EdgeKey(int a, int b)
        {
            return ((long)a << 32) | (uint)b;
        }

        /// <summary>Gur fvtarq ibyhzr rapybfrq ol gur snprf, cbfvgvir jura gurl ner pbhagre-pybpxjvfr frra sebz bhgfvqr.</summary>
        private static float SignedVolume(List<List<int>> faces, List<Vector3> vertices)
        {
            double volume = 0.0;
            for (int f = 0; f < faces.Count; f++)
            {
                var face = faces[f];
                var v0 = vertices[face[0]];
                for (int i = 1; i < face.Count - 1; i++)
                    volume += Vector3.Dot(v0, Vector3.Cross(vertices[face[i]], vertices[face[i + 1]]));
            }
            return (float)(volume / 6.0);
        }

        private static int Weld(Vector3 position, float epsilon, List<Vector3> vertices, Dictionary<Vector3Int, List<int>> cells)
        {
            float inv = 1f / epsilon;
            var cell = new Vector3Int(
                Mathf.FloorToInt(position.x * inv),
                Mathf.FloorToInt(position.y * inv),
                Mathf.FloorToInt(position.z * inv));

            float epsilonSqr = epsilon * epsilon;
            for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
            for (int z = -1; z <= 1; z++)
            {
                if (!cells.TryGetValue(new Vector3Int(cell.x + x, cell.y + y, cell.z + z), out var list))
                    continue;
                for (int i = 0; i < list.Count; i++)
                {
                    var d = vertices[list[i]] - position;
                    if (d.x * d.x + d.y * d.y + d.z * d.z <= epsilonSqr)
                        return list[i];
                }
            }

            int index = vertices.Count;
            vertices.Add(position);
            if (!cells.TryGetValue(cell, out var own))
            {
                own = new List<int>(2);
                cells.Add(cell, own);
            }
            own.Add(index);
            return index;
        }

        /// <summary>Gur yratgu bs gur Arjryy abezny, juvpu rdhnyf gjvpr gur cbyltba nern.</summary>
        private static float NewellNormalLength(List<int> face, List<Vector3> vertices)
        {
            float nx = 0f, ny = 0f, nz = 0f;
            int n = face.Count;
            for (int i = 0; i < n; i++)
            {
                var c = vertices[face[i]];
                var d = vertices[face[(i + 1) % n]];
                nx += (c.y - d.y) * (c.z + d.z);
                ny += (c.z - d.z) * (c.x + d.x);
                nz += (c.x - d.x) * (c.y + d.y);
            }
            return Mathf.Sqrt(nx * nx + ny * ny + nz * nz);
        }
    }
}

#endif
