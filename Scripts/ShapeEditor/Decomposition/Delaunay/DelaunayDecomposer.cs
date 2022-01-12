#if UNITY_EDITOR
/*
* Velcro Physics:
* Copyright (c) 2017 Ian Qvist
*/

using AeternumGames.ShapeEditor.Delaunay.Delaunay;
using AeternumGames.ShapeEditor.Delaunay.Delaunay.Sweep;
using System.Collections.Generic;
using UnityEngine;
using Vertices = AeternumGames.ShapeEditor.Polygon;

namespace AeternumGames.ShapeEditor.Delaunay
{
    /// <summary>
    /// 2D constrained Delaunay triangulation algorithm.
    /// Based on the paper "Sweep-line algorithm for constrained Delaunay triangulation" by V. Domiter and and B. Zalik
    ///
    /// Properties:
    /// - Creates triangles with a large interior angle.
    /// - Supports holes
    /// - Generate a lot of garbage due to incapsulation of the Poly2Tri library.
    /// - Running time is O(n^2), n = number of vertices.
    /// - Does not care about winding order.
    ///
    /// Source: http://code.google.com/p/poly2tri/
    /// </summary>
    internal static class DelaunayDecomposer
    {
        /// <summary>Decompose the polygon into several smaller non-concave polygon.</summary>
        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            Debug.Assert(vertices.Count >= 3);

            Polygon.Polygon poly = new Polygon.Polygon();

            foreach (var vertex in vertices)
            {
                poly.Points.Add(new TriangulationPoint(vertex.x, vertex.y));
            }

            if (vertices.Holes != null)
            {
                foreach (Vertices holeVertices in vertices.Holes)
                {
                    Polygon.Polygon hole = new Polygon.Polygon();

                    foreach (var vertex in holeVertices)
                    {
                        hole.Points.Add(new TriangulationPoint(vertex.x, vertex.y));
                    }

                    poly.AddHole(hole);
                }
            }

            DTSweepContext tcx = new DTSweepContext();
            tcx.PrepareTriangulation(poly);
            DTSweep.Triangulate(tcx);

            List<Vertices> results = new List<Vertices>();

            foreach (DelaunayTriangle triangle in poly.Triangles)
            {
                Vertices v = new Vertices();
                foreach (TriangulationPoint p in triangle.Points)
                {
                    v.Add(new Vertex((float)p.X, (float)p.Y));
                }
                results.Add(v);
            }

            return results;
        }
    }
}

#endif