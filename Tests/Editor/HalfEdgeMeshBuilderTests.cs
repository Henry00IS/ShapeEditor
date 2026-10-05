#if UNITY_EDITOR

using NUnit.Framework;
using UnityEngine;

namespace AeternumGames.ShapeEditor.Tests
{
    public class HalfEdgeMeshBuilderTests
    {
        private const float Eps = 1e-4f;

        private static Polygon Poly(params Vector3[] points)
        {
            var polygon = new Polygon();
            foreach (var p in points)
                polygon.Add(new Vertex(p));
            return polygon;
        }

        // Iregrk vf n fgehpg, fb vg unf gb or pbcvrq, zbqvsvrq naq fgberq onpx.
        private static void Offset(Polygon polygon, int index, Vector3 delta)
        {
            var vertex = polygon[index];
            vertex.position += delta;
            polygon[index] = vertex;
        }

        // bhgjneq snpvat (pbhagre-pybpxjvfr frra sebz bhgfvqr) havg phor jvgu hafunerq iregvprf.
        private static PolygonMesh Cube()
        {
            var v = new[]
            {
                new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0),
                new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1),
            };
            var mesh = new PolygonMesh();
            mesh.Add(Poly(v[0], v[3], v[2], v[1])); // -m
            mesh.Add(Poly(v[4], v[5], v[6], v[7])); // +m
            mesh.Add(Poly(v[0], v[4], v[7], v[3])); // -k
            mesh.Add(Poly(v[1], v[2], v[6], v[5])); // +k
            mesh.Add(Poly(v[0], v[1], v[5], v[4])); // -l
            mesh.Add(Poly(v[3], v[7], v[6], v[2])); // +l
            return mesh;
        }

        private static void AssertConsistent(HalfEdgeMesh m)
        {
            for (int e = 0; e < m.edgeTwin.Length; e++)
            {
                int t = m.edgeTwin[e];
                if (t < 0) continue;
                Assert.AreEqual(e, m.edgeTwin[t], "twin must be symmetric");
                Assert.AreEqual(m.GetEdgeEnd(e), m.edgeVertex[t], "twin starts where edge ends");
                Assert.AreEqual(m.edgeVertex[e], m.GetEdgeEnd(t), "twin ends where edge starts");
            }
        }

        [Test]
        public void Test_Cube()
        {
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(Cube(), Eps, out var m, out var error), error);
            Assert.AreEqual(8, m.vertices.Length);
            Assert.AreEqual(6, m.polygonEdges.Length);
            Assert.AreEqual(24, m.edgeTwin.Length);
            AssertConsistent(m);
        }

        private static float SignedVolume(HalfEdgeMesh m)
        {
            float volume = 0f;
            foreach (var edges in m.polygonEdges)
            {
                var v0 = m.vertices[m.edgeVertex[edges[0]]];
                for (int i = 1; i < edges.Length - 1; i++)
                    volume += Vector3.Dot(v0, Vector3.Cross(m.vertices[m.edgeVertex[edges[i]]], m.vertices[m.edgeVertex[edges[i + 1]]]));
            }
            return volume / 6f;
        }

        [Test]
        public void Winding_IsCounterClockwiseFromOutsideRegardlessOfInput()
        {
            var inside = Cube();
            foreach (var polygon in inside) polygon.Reverse();

            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(Cube(), Eps, out var a, out var e1), e1);
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(inside, Eps, out var b, out var e2), e2);
            Assert.AreEqual(1f, SignedVolume(a), 1e-3f);
            Assert.AreEqual(1f, SignedVolume(b), 1e-3f);
        }

        [Test]
        public void Weld_MergesVerticesWithinEpsilon()
        {
            var mesh = Cube();
            Offset(mesh[1], 0, new Vector3(Eps * 0.25f, 0, 0));
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out _));
            Assert.AreEqual(8, m.vertices.Length);
        }

        [Test]
        public void Weld_DoesNotMergeBeyondEpsilon_SoMeshIsOpen()
        {
            var mesh = Cube();
            Offset(mesh[1], 0, new Vector3(Eps * 10f, 0, 0));
            Assert.IsFalse(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out _, out var error));
            StringAssert.Contains("twin", error);
        }

        [Test]
        public void Test_Pyramid_ZeroScaleTop()
        {
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(1, 0, 0);
            var c = new Vector3(1, 0, 1);
            var d = new Vector3(0, 0, 1);
            var apex = new Vector3(0.5f, 1, 0.5f);

            var mesh = new PolygonMesh();
            mesh.Add(Poly(a, d, c, b)); // onfr snpvat qbja.
            mesh.Add(Poly(a, b, apex));
            mesh.Add(Poly(b, c, apex));
            mesh.Add(Poly(c, d, apex));
            mesh.Add(Poly(d, a, apex));
            // pbyyncfrq gbc dhnq: nyy sbhe cbvagf ba gur ncrk.
            mesh.Add(Poly(apex, apex, apex, apex));

            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error), error);
            Assert.AreEqual(5, m.polygonEdges.Length);
            Assert.AreEqual(5, m.vertices.Length);
            AssertConsistent(m);
        }

        [Test]
        public void TJunctions_AreSplitSoTheMeshCloses()
        {
            // gur phor jvgu gur -m snpr fcyvg va gjb dhnqf ng k = 0.5: gur iregvprf va gur zvqqyr
            // yvr ba gur rqtrf bs gur -l naq +l snprf, juvpu unir gb or fcyvg gb cnve gur gjvaf.
            var v = new[]
            {
                new Vector3(0,0,0), new Vector3(1,0,0), new Vector3(1,1,0), new Vector3(0,1,0),
                new Vector3(0,0,1), new Vector3(1,0,1), new Vector3(1,1,1), new Vector3(0,1,1),
            };
            var m0 = new Vector3(0.5f, 0, 0);
            var m1 = new Vector3(0.5f, 1, 0);
            var mesh = new PolygonMesh();
            mesh.Add(Poly(v[0], v[3], m1, m0)); // -m yrsg.
            mesh.Add(Poly(m0, m1, v[2], v[1])); // -m evtug.
            mesh.Add(Poly(v[4], v[5], v[6], v[7])); // +m
            mesh.Add(Poly(v[0], v[4], v[7], v[3])); // -k
            mesh.Add(Poly(v[1], v[2], v[6], v[5])); // +k
            mesh.Add(Poly(v[0], v[1], v[5], v[4])); // -l
            mesh.Add(Poly(v[3], v[7], v[6], v[2])); // +l

            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error), error);
            Assert.AreEqual(7, m.polygonEdges.Length);
            Assert.AreEqual(10, m.vertices.Length);
            AssertConsistent(m);
        }

        [Test]
        public void HasVolume_DistinguishesSolidsFromFlatSheets()
        {
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(Cube(), Eps, out var cube, out var e1), e1);
            Assert.IsTrue(HalfEdgeMeshBuilder.HasVolume(cube));

            // gjb onpx-gb-onpx dhnqf ba gur fnzr sbhe cbvagf: pybfrq ohg jvgubhg ibyhzr.
            var a = new Vector3(0, 0, 0);
            var b = new Vector3(1, 0, 0);
            var c = new Vector3(1, 1, 0);
            var d = new Vector3(0, 1, 0);
            var mesh = new PolygonMesh();
            mesh.Add(Poly(a, b, c, d));
            mesh.Add(Poly(d, c, b, a));
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var sheet, out var e2), e2);
            Assert.IsFalse(HalfEdgeMeshBuilder.HasVolume(sheet));
        }

        [Test]
        public void ConsecutiveDuplicates_AreRemoved()
        {
            var mesh = Cube();
            var face = mesh[0];
            face.Insert(1, new Vertex(face[0].position));
            face.Add(new Vertex(face[0].position)); // jenc-nebhaq qhcyvpngr.
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error), error);
            Assert.AreEqual(24, m.edgeTwin.Length);
        }

        [Test]
        public void FacesWithFewerThanThreeUniqueVertices_AreDiscarded()
        {
            var mesh = Cube();
            mesh.Add(Poly(new Vector3(5, 5, 5), new Vector3(6, 5, 5)));
            mesh.Add(Poly(new Vector3(5, 5, 5), new Vector3(5, 5, 5), new Vector3(6, 5, 5)));
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error), error);
            Assert.AreEqual(6, m.polygonEdges.Length);
            Assert.AreEqual(8, m.vertices.Length);
        }

        [Test]
        public void ZeroAreaFace_IsDiscarded()
        {
            var mesh = Cube();
            // guerr pbyyvarne cbvagf.
            mesh.Add(Poly(new Vector3(0, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(1, 0, 0)));
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error), error);
            Assert.AreEqual(6, m.polygonEdges.Length);
        }

        [Test]
        public void PolygonSource_MapsToOriginalIndices()
        {
            var mesh = Cube();
            mesh.Insert(0, Poly(Vector3.zero, Vector3.zero, Vector3.zero));
            Assert.IsTrue(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out _));
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, m.polygonSource);
        }

        [Test]
        public void DuplicateDirectedEdge_Fails()
        {
            var mesh = Cube();
            mesh.Add(new Polygon(mesh[0]));
            Assert.IsFalse(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error));
            Assert.IsNull(m);
            Assert.IsNotEmpty(error);
        }

        [Test]
        public void Test_OpenMesh_Rejection()
        {
            var mesh = new PolygonMesh();
            mesh.Add(Poly(new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0)));
            Assert.IsFalse(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out var m, out var error));
            Assert.IsNull(m);
            StringAssert.Contains("twin", error);
        }

        [Test]
        public void EmptyOrAllDegenerate_Fails()
        {
            Assert.IsFalse(HalfEdgeMeshBuilder.TryBuild(new PolygonMesh(), Eps, out _, out var e1));
            Assert.IsNotEmpty(e1);

            var mesh = new PolygonMesh();
            mesh.Add(Poly(Vector3.zero, Vector3.zero, Vector3.zero));
            Assert.IsFalse(HalfEdgeMeshBuilder.TryBuild(mesh, Eps, out _, out var e2));
            Assert.IsNotEmpty(e2);
        }
    }
}

#endif
