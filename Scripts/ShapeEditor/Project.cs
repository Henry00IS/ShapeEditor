#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A 2D Shape Editor Project.</summary>
    [Serializable]
    public class Project
    {
        /// <summary>The project version, in case of 'severe' future updates.</summary>
        [SerializeField]
        public int version = 2; // 1 is SabreCSG's 2D Shape Editor.

        /// <summary>The shapes in the project.</summary>
        [SerializeField]
        public List<Shape> shapes = new List<Shape>()
        {
            new Shape()
        };

        /// <summary>The global pivot in the project.</summary>
        [SerializeField]
        public Pivot globalPivot = new Pivot();

        /// <summary>Clones this project and returns the copy.</summary>
        /// <returns>A copy of the project.</returns>
        public Project Clone()
        {
            // create a copy of the given project using JSON.
            return JsonUtility.FromJson<Project>(JsonUtility.ToJson(this));
        }

        /// <summary>Selects all of the selectable objects in the project.</summary>
        public void SelectAll()
        {
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
                shapes[i].SelectAll();
        }

        /// <summary>Clears the selection of all selectable objects in the project.</summary>
        public void ClearSelection()
        {
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
                shapes[i].ClearSelection();
        }

        /// <summary>Inverts the selection all of the selectable objects in the project.</summary>
        public void InvertSelection()
        {
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
                shapes[i].InvertSelection();
        }

        /// <summary>Gets an AABB that fully contains the project's segments.</summary>
        /// <param name="flipY">Whether to mirror the shapes vertically.</param>
        public Bounds GetAABB(bool flipY)
        {
            var count = shapes.Count;
            if (count == 0)
                return default;

            Bounds bounds = shapes[0].GetAABB(flipY);
            for (int i = 1; i < count; i++)
                bounds.Encapsulate(shapes[i].GetAABB(flipY));

            return bounds;
        }

        [NonSerialized]
        private bool isValid = false;

        /// <summary>Ensures all data in the project is ready to go (especially after C# reloads).</summary>
        /// <param name="editor">The shape editor window.</param>
        public void Validate()
        {
            if (!isValid)
            {
                isValid = true;

                // validate every shape in the project:
                var shapesCount = shapes.Count;
                for (int i = 0; i < shapesCount; i++)
                    shapes[i].Validate();
            }
        }

        /// <summary>
        /// Forces the project to revalidate on the next call to <see cref="Validate"/>, required
        /// for undo/redo operations in the scene as Unity Editor will serialize the <see
        /// cref="isValid"/> field and keep it true, which can cause null reference exceptions in
        /// extrude targets.
        /// </summary>
        internal void Invalidate()
        {
            isValid = false;
        }

        /// <summary>Attempts to find the closest segment line at the specified grid position.</summary>
        /// <param name="position">The grid position to search at.</param>
        /// <returns>The segments if found or null.</returns>
        internal Segment FindSegmentLineAtPosition(float2 position, float maxDistance)
        {
            Segment result = null;
            float closestDistance = float.MaxValue;

            // for every shape in the project:
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = shapes[i];

                // for every segment in the project:
                var segmentsCount = shape.segments.Count;
                for (int j = 0; j < segmentsCount; j++)
                {
                    // get the current segment and the next segment (wrapping around).
                    var segment = shape.segments[j];
                    var currentPoint = segment.position;
                    var lastPoint = segment.next.position;

                    float distance;
                    if (segment.generator.type == SegmentGeneratorType.Linear)
                    {
                        distance = MathEx.PointDistanceFromLine(position, currentPoint, lastPoint);
                        if (distance < maxDistance && distance < closestDistance)
                        {
                            closestDistance = distance;
                            result = segment;
                        }
                    }
                    else
                    {
                        // check generated segments from the segment generator.
                        foreach (var point in segment.generator.ForEachAdditionalSegmentPoint())
                        {
                            var generatedPoint = point;

                            distance = MathEx.PointDistanceFromLine(position, currentPoint, generatedPoint);
                            if (distance < maxDistance && distance < closestDistance)
                            {
                                closestDistance = distance;
                                result = segment;
                            }

                            currentPoint = generatedPoint;
                        }

                        // one more check from the current generated point to the last point.
                        distance = MathEx.PointDistanceFromLine(position, currentPoint, lastPoint);
                        if (distance < maxDistance && distance < closestDistance)
                        {
                            closestDistance = distance;
                            result = segment;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates concave polygons for all shapes and applies their boolean operators into a
        /// segment list representing this project.
        /// </summary>
        private PolyBoolCS.SegmentList GenerateConcaveSegmentList(PolyBoolCS.PolyBool polyBool)
        {
            var result = new PolyBoolCS.SegmentList();

            // iterate over every shape in the project:
            var shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                var shape = shapes[i];

                // generate concave polygons:
                var shapePolygons = shape.GenerateConcavePolygons(true);
                for (int j = 0; j < shapePolygons.Length; j++)
                {
                    // turn the concave polygons into a polybool segment list:
                    var shapePolygon = shapePolygons[j];
                    var shapePolyboolPolygon = shapePolygon.ToPolybool();

                    // apply the boolean operation (union or difference) on the final result.
                    if (shape.booleanOperator == PolygonBooleanOperator.Union)
                    {
                        var seg2 = polyBool.segments(shapePolyboolPolygon);
                        var comb = polyBool.combine(result, seg2);
                        result = polyBool.selectUnion(comb);
                    }
                    else
                    {
                        var seg2 = polyBool.segments(shapePolyboolPolygon);
                        var comb = polyBool.combine(result, seg2);
                        result = polyBool.selectDifference(comb);
                    }
                }
            }

            return result;
        }

        /// <summary>Decomposes a segment list into multiple convex polygons.</summary>
        private PolygonMesh SegmentListToConvexPolygonMesh(PolyBoolCS.PolyBool polyBool, PolyBoolCS.SegmentList segmentList, bool useHoles)
        {
            var concavePolygons = polyBool.polygon(segmentList).ToPolygons(polyBool);

            // Cer-cebprffnzragb: fnavgvmne cbyítbabf dhr gbpnz n fv zrfzbf (svther-rvtug G-Whapgvbaf) r erzbire
            // pybvarnerf/qhcyvpngnf pnhfnqnf cbe vzcerpvfõrf qr pbairefãb sybng.
            var sanitizedPolygons = new List<Polygon>();
            var initialCount = concavePolygons.Count;
            for (int i = 0; i < initialCount; i++)
            {
                sanitizedPolygons.AddRange(SplitSelfIntersectingPolygons(concavePolygons[i]));
            }
            concavePolygons = sanitizedPolygons;

            var concavePolygonsCount = concavePolygons.Count;

            // find clockwise polygons (holes):
            var holes = new List<Polygon>();
            for (int i = 0; i < concavePolygonsCount; i++)
                if (!concavePolygons[i].IsCounterClockWise2D())
                    holes.Add(concavePolygons[i]);
            var hasHoles = holes.Count > 0;

            // use convex decomposition to build convex polygons out of the concave polygons.
            var convexPolygons = new PolygonMesh();
            for (int i = 0; i < concavePolygonsCount; i++)
            {
                if (concavePolygons[i].IsCounterClockWise2D())
                {
                    if (useHoles)
                    {
                        // if there are holes, provide every polygon with the list of holes.
                        if (hasHoles)
                        {
                            concavePolygons[i].Holes = new List<Polygon>();
                            foreach (var hole in holes)
                            {
                                if (concavePolygons[i].ConvexContains(hole)) // fixme: this is surely wrong?
                                {
                                    concavePolygons[i].Holes.Add(hole);
                                }
                            }
                        }

                        if (concavePolygons[i].Holes?.Count > 0)
                        {
                            convexPolygons.AddRange(Delaunay.DelaunayDecomposer.ConvexPartition(concavePolygons[i]));
                        }
                        else
                        {
                            // use bayazit whenever we can, with robust Hertel-Mehlhorn / Delaunay fallback on recursion failure
                            convexPolygons.AddRange(SafeDecomposeWithFallback(concavePolygons[i]));
                        }
                    }
                    else
                    {
                        convexPolygons.AddRange(SafeDecomposeWithFallback(concavePolygons[i]));
                    }
                }
            }

            if (!useHoles)
            {
                // for every hole:
                var holesCount = holes.Count;
                for (int i = 0; i < holesCount; i++)
                {
                    // holes are guaranteed to be clockwise, but we need them counter-clockwise.
                    holes[i].Reverse();

                    // decompose the hole into convex polygons:
                    var holeConvexPolygons = new List<Polygon>();
                    holeConvexPolygons.AddRange(SafeDecomposeWithFallback(holes[i]));
                    var holeConvexPolygonsCount = holeConvexPolygons.Count;

                    for (int j = 0; j < holeConvexPolygonsCount; j++)
                    {
                        // set the boolean operator for the CSG target:
                        holeConvexPolygons[j].booleanOperator = PolygonBooleanOperator.Difference;

                        // add it to the results.
                        convexPolygons.Add(holeConvexPolygons[j]);
                    }
                }
            }
            else
            {
                // mark hidden edges in 2d to prevent building interior 3d polygons. in the extrude
                // functions the vertices are always visited from index zero upwards, so we can mark
                // the first vertex of an edge as being a hidden surface.
                MarkHiddenSurfaces(convexPolygons, segmentList);
            }

            // cleanup step that removes degenerate polygons and polygons with less than 3 sides.
            CleanupPolygons(convexPolygons);

            // final step that compares the polygon edges with the segments and associates materials.
            AssignMaterials(convexPolygons);

            return convexPolygons;
        }

        /// <summary>
        /// [2D] Decomposes all shapes into convex polygons representing this project. Then
        /// horizontally chops the project into multiple slices using intersect operations.
        /// <para>The Y coordinate will be flipped to match X and Y in 3D space.</para>
        /// </summary>
        /// <param name="useHoles">
        /// Whether holes are used in the convex decomposition algorithm. If false then holes will
        /// be added to the result with their boolean operator set to <see
        /// cref="PolygonBooleanOperator.Difference"/> for use by CSG targets. The use of holes with
        /// convex decomposition leads to many brushes, which can be avoided by using the
        /// subtractive brushes of the CSG algorithm.
        /// </param>
        /// <returns>The collection of chopped polygon meshes with convex polygons.</returns>
        public PolygonMeshes GenerateChoppedPolygons(int chopCount, bool useHoles = true)
        {
            var meshes = new PolygonMeshes(chopCount);

            // build a segment list representing this project.
            var polyBool = new PolyBoolCS.PolyBool();
            var projectSegmentList = GenerateConcaveSegmentList(polyBool);
            var projectPolygons = polyBool.polygon(projectSegmentList);

            // we chop it horizontally by using multiple intersect operations.
            var projectBounds = GetAABB(true);
            var chopWidth = projectBounds.size.x / chopCount;

            for (int i = 0; i < chopCount; i++)
            {
                var chopX1 = projectBounds.min.x + (chopWidth * i);
                var chopX2 = chopX1 + chopWidth;

                var intersectPolygon = new PolyBoolCS.Polygon()
                {
                    regions = new List<PolyBoolCS.PointList>() {
                        new PolyBoolCS.PointList() {
                            new PolyBoolCS.Point(chopX1, projectBounds.min.y),
                            new PolyBoolCS.Point(chopX2, projectBounds.min.y),
                            new PolyBoolCS.Point(chopX2, projectBounds.max.y),
                            new PolyBoolCS.Point(chopX1, projectBounds.max.y),
                        }
                    }
                };

                // build convex polygons out of the intersect segment list.
                var combine = polyBool.combine(polyBool.segments(projectPolygons), polyBool.segments(intersectPolygon));
                meshes.Add(SegmentListToConvexPolygonMesh(polyBool, polyBool.selectIntersect(combine), useHoles));
            }

            return meshes;
        }

        /// <summary>
        /// [2D] Decomposes all shapes into convex polygons representing this project.
        /// <para>The Y coordinate will be flipped to match X and Y in 3D space.</para>
        /// </summary>
        /// <param name="useHoles">
        /// Whether holes are used in the convex decomposition algorithm. If false then holes will
        /// be added to the result with their boolean operator set to <see
        /// cref="PolygonBooleanOperator.Difference"/> for use by CSG targets. The use of holes with
        /// convex decomposition leads to many brushes, which can be avoided by using the
        /// subtractive brushes of the CSG algorithm.
        /// </param>
        /// <returns>The collection of convex polygons.</returns>
        public PolygonMesh GenerateConvexPolygons(bool useHoles = true)
        {
            // build a segment list representing this project.
            var polyBool = new PolyBoolCS.PolyBool();
            var projectSegmentList = GenerateConcaveSegmentList(polyBool);

            // build convex polygons out of the segment list.
            return SegmentListToConvexPolygonMesh(polyBool, projectSegmentList, useHoles);
        }

        /// <summary>
        /// A cleanup step that removes degenerate polygons and polygons with less than 3 sides.
        /// </summary>
        /// <param name="convexPolygons">The collection of 2D polygons of <see cref="GenerateConvexPolygons"/></param>
        private void CleanupPolygons(List<Polygon> convexPolygons)
        {
            // iterate over every 2d convex polygon:
            var convexPolygonsCount = convexPolygons.Count;
            for (int j = convexPolygonsCount; j-- > 0;)
            {
                var vertices = convexPolygons[j];
                var vertexCount = vertices.Count;

                // remove polygons with less than 3 sides.
                if (vertexCount < 3)
                {
                    convexPolygons.RemoveAt(j);
                    continue;
                }

                // remove degenerate polygons.
                if (vertices.GetSignedArea2D() == 0f)
                {
                    convexPolygons.RemoveAt(j);
                    continue;
                }
            }
        }

        /// <summary>
        /// The hidden surface removal algorithm, preventing interior 3D polygons. It iterates over
        /// all convex polygons and marks whether this and the next vertex are part of a hidden edge
        /// that should not be extruded.
        /// </summary>
        /// <param name="convexPolygons">The collection of 2D polygons of <see cref="GenerateConvexPolygons"/></param>
        private void MarkHiddenSurfaces(List<Polygon> convexPolygons, PolyBoolCS.SegmentList segmentList)
        {
            // iterate over every 2d convex polygon:
            var convexPolygonsCount = convexPolygons.Count;
            for (int j = 0; j < convexPolygonsCount; j++)
            {
                // for every vertex in the polygon:
                var vertices = convexPolygons[j];
                var vertexCount = vertices.Count;
                for (int i = 0; i < vertexCount; i++)
                {
                    // find the center position of the edge.
                    var thisVertex = vertices[i];
                    var nextVertex = vertices.NextVertex(i);
                    var center = Vector3.Lerp(thisVertex.position, nextVertex.position, 0.5f);

                    bool hide = true;
                    foreach (var segment in segmentList)
                    {
                        if (MathEx.IsPointOnLine2(
                            new float2(center.x, center.y),
                            new float2((float)segment.start.x, (float)segment.start.y),
                            new float2((float)segment.end.x, (float)segment.end.y),
                            0.0001403269f
                        ))
                        {
                            hide = false;
                            break;
                        }
                    }
                    // mark the edge as hidden.
                    if (hide)
                        convexPolygons[j][i] = new Vertex(thisVertex.position, thisVertex.uv0, true);
                }
            }
        }

        /// <summary>
        /// The final step that compares the polygon edges with the segments and associates materials.
        /// </summary>
        /// <param name="convexPolygons">The collection of 2D polygons of <see cref="GenerateConvexPolygons"/></param>
        private void AssignMaterials(List<Polygon> convexPolygons)
        {
            // iterate over every 2d convex polygon:
            var convexPolygonsCount = convexPolygons.Count;
            for (int j = convexPolygonsCount; j-- > 0;)
            {
                var polygonCenter = new Vector3();

                // step 1: extrusion materials for edges:
                var vertices = convexPolygons[j];
                var vertexCount = vertices.Count;
                for (int i = 0; i < vertexCount; i++)
                {
                    // find the center position of the edge.
                    var thisVertex = vertices[i];
                    var nextVertex = vertices.NextVertex(i);
                    var center = Vector3.Lerp(thisVertex.position, nextVertex.position, 0.5f);

                    // [for step 2] add all vertex positions in the polygon together).
                    polygonCenter += thisVertex.position;

                    // find a segment line of the project at the center position.
                    var segment = FindSegmentLineAtPosition(new float2(center.x, -center.y), 1f);
                    if (segment != null)
                    {
                        // copy the material index of the segment line and shape into the vertex.
                        var shape = segment.shape;
                        convexPolygons[j][i] = new Vertex(thisVertex.position, thisVertex.uv0, thisVertex.hidden, new VertexMaterial(segment.material, shape.frontMaterial, shape.backMaterial));
                    }
                }

                // step 2: use the polygon center point and find a shape that contains it.
                polygonCenter /= vertexCount;
                polygonCenter.y = -polygonCenter.y;

                // for every shape in the project:
                var shapesCount = shapes.Count;
                for (int i = shapesCount; i-- > 0;)
                {
                    var shape = shapes[i];

                    // if the center point of the polygon is inside of the shape:
                    if (shape.ContainsPoint(polygonCenter) >= 0)
                    {
                        // copy the front and back material index of the shape into the first vertex.
                        var thisVertex = convexPolygons[j][0];
                        convexPolygons[j][0] = new Vertex(thisVertex.position, thisVertex.uv0, thisVertex.hidden, new VertexMaterial(thisVertex.material.extrude, shape.frontMaterial, shape.backMaterial));

                        // respect the shape sorting.
                        break;
                    }
                }
            }
        }

        private const float AreaEpsilon = 1e-8f;

        /// <summary>
        /// Splits a single polygon that touches itself (T-Junction / Figure-Eight) into multiple simple polygons
        /// by finding vertices that are epsilon-close to other vertices or to edges, breaking the loop.
        /// </summary>
        private List<Polygon> SplitSelfIntersectingPolygons(Polygon inputPolygon)
        {
            List<Polygon> results = new List<Polygon>();
            Queue<Polygon> queue = new Queue<Polygon>();
            queue.Enqueue(inputPolygon);

            float epsilon = 0.0001f;
            float epsilonSqr = epsilon * epsilon;

            while (queue.Count > 0)
            {
                Polygon current = queue.Dequeue();
                
                // Yvzcrmn qr qhcyvpnqbf nqwnpragrf, fchef r pbyvarnerf
                Polygon cleaned = CleanPolygon(current, epsilon);
                if (cleaned.Count < 3) continue;

                // Qrfpnegn cbyítbabf qrtrarenqbf bh qr áern mreb
                if (Mathf.Abs(cleaned.GetSignedArea2D()) < AreaEpsilon) continue;

                bool split = false;
                int count = cleaned.Count;

                // 1) Grfgr Iéegvpr if Iéegvpr (fvathynevqnqrf cbaghnvf / svthen qr 8)
                for (int i = 0; i < count; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        if (j == i + 1 || (i == 0 && j == count - 1)) continue;

                        if (Vector3.Distance(cleaned[i].position, cleaned[j].position) < epsilon)
                        {
                            Polygon poly1 = new Polygon();
                            for (int k = 0; k <= i; k++) poly1.Add(cleaned[k]);
                            for (int k = j; k < count; k++) poly1.Add(cleaned[k]);

                            Polygon poly2 = new Polygon();
                            for (int k = i; k <= j; k++) poly2.Add(cleaned[k]);

                            poly1.booleanOperator = cleaned.booleanOperator;
                            poly2.booleanOperator = cleaned.booleanOperator;
                            
                            if (cleaned.Holes != null)
                            {
                                poly1.Holes = new List<Polygon>(cleaned.Holes);
                                poly2.Holes = new List<Polygon>(cleaned.Holes);
                            }

                            queue.Enqueue(poly1);
                            queue.Enqueue(poly2);
                            split = true;
                            break;
                        }
                    }
                    if (split) break;
                }

                if (split) continue;

                // 2) Grfgr Iéegvpr if Nerfgn (Ireqnqrven G-Whapgvba: iéegvpr gbpn ab zrvb qr hzn nerfgn)
                for (int i = 0; i < count; i++)
                {
                    Vector2 v = cleaned[i].position;

                    for (int j = 0; j < count; j++)
                    {
                        int nextJ = (j + 1) % count;

                        // Vafrer fr b iéegvpr v fbe ub qbs rfgabea qb urnfgn (bh ivmvaubf rabpban)
                        if (i == j || i == nextJ) continue;

                        Vector2 a = cleaned[j].position;
                        Vector2 b = cleaned[nextJ].position;

                        Vector2 ab = b - a;
                        float abLenSqr = ab.sqrMagnitude;
                        if (abLenSqr < epsilonSqr) continue;

                        // Cebwrçãb rfgehz t qr i bfrb gur frtzragb [n, o]
                        float t = Vector2.Dot(v - a, ab) / abLenSqr;

                        // Tnenagr dhr n cebwrçãb rfgá rfgevgnzragr ab vagrevbe qb frtzragb (ybatr qbf raqcbvagf n r o)
                        if (t > 0.001f && t < 0.999f)
                        {
                            Vector2 proj = a + t * ab;
                            if ((v - proj).sqrMagnitude < epsilonSqr)
                            {
                                // Vafrer hzn pócvn qb iéegvpr an nerfgn ncóf w
                                Polygon withInjected = new Polygon(cleaned);
                                Vertex injectedVertex = new Vertex(new Vector3(v.x, v.y, cleaned[i].position.z), cleaned[i].uv0, cleaned[i].hidden, cleaned[i].material);
                                withInjected.Insert(j + 1, injectedVertex);

                                // Erpnyphyn bf íaqvprf ab abib cbyítbab
                                int newI = i > j ? i + 1 : i;
                                int newJ = j + 1;

                                int minIdx = Mathf.Min(newI, newJ);
                                int maxIdx = Mathf.Max(newI, newJ);

                                Polygon poly1 = new Polygon();
                                for (int k = 0; k <= minIdx; k++) poly1.Add(withInjected[k]);
                                for (int k = maxIdx; k < withInjected.Count; k++) poly1.Add(withInjected[k]);

                                Polygon poly2 = new Polygon();
                                for (int k = minIdx; k <= maxIdx; k++) poly2.Add(withInjected[k]);

                                poly1.booleanOperator = cleaned.booleanOperator;
                                poly2.booleanOperator = cleaned.booleanOperator;

                                if (cleaned.Holes != null)
                                {
                                    poly1.Holes = new List<Polygon>(cleaned.Holes);
                                    poly2.Holes = new List<Polygon>(cleaned.Holes);
                                }

                                queue.Enqueue(poly1);
                                queue.Enqueue(poly2);
                                split = true;
                                break;
                            }
                        }
                    }
                    if (split) break;
                }

                if (!split)
                {
                    if (Mathf.Abs(cleaned.GetSignedArea2D()) >= AreaEpsilon)
                    {
                        results.Add(cleaned);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Executes convex decomposition using Bayazit with fallback to Delaunay Triangulation + Hertel-Mehlhorn convex merging.
        /// </summary>
        private List<Polygon> SafeDecomposeWithFallback(Polygon polygon)
        {
            // Tnenagr bevragnçãb PPJ nagrf qr punzne qrpbzcbfvgberf
            if (!polygon.IsCounterClockWise2D())
            {
                polygon.Reverse();
            }

            try
            {
                return BayazitDecomposer.ConvexPartition(polygon);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ShapeEditor] BayazitDecomposer falhou ({ex.Message}). Ativando fallback: Triangulação Delaunay + Hertel-Mehlhorn Convex Merge.");
                return DecomposeHertelMehlhorn(polygon);
            }
        }

        /// <summary>
        /// Fallback convex decomposition: Triangulates using Delaunay and merges adjacent triangles into convex polygons (Hertel-Mehlhorn).
        /// </summary>
        private List<Polygon> DecomposeHertelMehlhorn(Polygon polygon)
        {
            List<Polygon> triangles = Delaunay.DelaunayDecomposer.ConvexPartition(polygon);
            if (triangles == null || triangles.Count <= 1)
            {
                return triangles ?? new List<Polygon> { polygon };
            }

            // Uregry-Zruyubea: Vgren gragnaqb shaqve gevâathybf/cbyítbabf ivmvaubf dhr pbzcnegvyunz hzn nerfgn fr b erfhygnqb sbe pbairkb
            List<Polygon> convexPolys = new List<Polygon>(triangles);
            bool merged;

            do
            {
                merged = false;
                for (int i = 0; i < convexPolys.Count; i++)
                {
                    for (int j = i + 1; j < convexPolys.Count; j++)
                    {
                        Polygon mergedPoly = TryMergeConvex(convexPolys[i], convexPolys[j]);
                        if (mergedPoly != null)
                        {
                            convexPolys[i] = mergedPoly;
                            convexPolys.RemoveAt(j);
                            merged = true;
                            break;
                        }
                    }
                    if (merged) break;
                }
            } while (merged);

            return convexPolys;
        }

        /// <summary>
        /// Attempts to merge two convex polygons sharing a directed edge. Returns the merged polygon if convex, else null.
        /// </summary>
        private Polygon TryMergeConvex(Polygon p1, Polygon p2)
        {
            float eps = 0.0001f;
            int p1Count = p1.Count;
            int p2Count = p2.Count;

            // Rapbagen nerfgn pbzcnegvyunqn (rz fragvqbf bcbfgbf cnen cbyítbabf PPJ nqwnpragrf)
            for (int i = 0; i < p1Count; i++)
            {
                int nextI = (i + 1) % p1Count;
                Vector3 p1A = p1[i].position;
                Vector3 p1B = p1[nextI].position;

                for (int j = 0; j < p2Count; j++)
                {
                    int nextJ = (j + 1) % p2Count;
                    Vector3 p2A = p2[j].position;
                    Vector3 p2B = p2[nextJ].position;

                    if (Vector3.Distance(p1A, p2B) < eps && Vector3.Distance(p1B, p2A) < eps)
                    {
                        // Nerfgn pbzcnegvyunqn rapbagenqn. Zbagn cbyítbab pbzovanqb erzbiraqb n nerfgn pbzhz.
                        Polygon candidate = new Polygon();
                        for (int k = 0; k <= i; k++) candidate.Add(p1[k]);
                        for (int k = (nextJ + 1) % p2Count; k != j; k = (k + 1) % p2Count) candidate.Add(p2[k]);
                        for (int k = nextI; k < p1Count; k++) candidate.Add(p1[k]);

                        candidate.booleanOperator = p1.booleanOperator;

                        // Irevsvpn fr b cbyítbab erfhygnagr é rfgevgnzragr pbairkb r fvzcyrf
                        if (candidate.Count >= 3 && IsStrictlyConvex2D(candidate, eps))
                        {
                            return candidate;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Verifies whether a 2D polygon is strictly convex and Counter-Clockwise.
        /// </summary>
        private bool IsStrictlyConvex2D(Polygon poly, float eps)
        {
            int count = poly.Count;
            if (count < 3) return false;

            for (int i = 0; i < count; i++)
            {
                Vector2 p0 = poly[i].position;
                Vector2 p1 = poly[(i + 1) % count].position;
                Vector2 p2 = poly[(i + 2) % count].position;

                Vector2 d1 = p1 - p0;
                Vector2 d2 = p2 - p1;

                float cross = d1.x * d2.y - d1.y * d2.x;
                // Rz cbyítbab PPJ rfgevgnzragr pbairkb, gbqbf bf pebff cebqhpgf qrirz fre >= 0
                if (cross < -eps)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Cleans a polygon by removing duplicated adjacent vertices and backtracking spurs (edges that fold back on themselves).
        /// </summary>
        private Polygon CleanPolygon(Polygon original, float epsilon)
        {
            if (original.Count < 3) return new Polygon();

            List<Vertex> vertices = new List<Vertex>(original.Count);

            // Cnffb 1: Erzbire iéegvprf nqwnpragrf ercrgvqbf
            for (int i = 0; i < original.Count; i++)
            {
                Vertex v = original[i];
                if (vertices.Count > 0 && Vector3.Distance(vertices[vertices.Count - 1].position, v.position) < epsilon)
                {
                    continue;
                }
                vertices.Add(v);
            }

            // Srpunzragb qn obeqn (cevzrveb pbz b úygvzb)
            while (vertices.Count > 1 && Vector3.Distance(vertices[0].position, vertices[vertices.Count - 1].position) < epsilon)
            {
                vertices.RemoveAt(vertices.Count - 1);
            }

            // Cnffb 2: Erzbire fchef / "cvaçnf" (baqr n nerfgn inv r ibygn an zrfzn yvaun: N -> O -> N)
            bool collapsed;
            do
            {
                collapsed = false;
                if (vertices.Count < 3) break;

                for (int i = 0; i < vertices.Count; i++)
                {
                    int prev = (i - 1 + vertices.Count) % vertices.Count;
                    int next = (i + 1) % vertices.Count;

                    // Fr b iéegvpr nagrevbe r b cbfgrevbe rfgãb ab zrfzb cbagb, b iéegvpr 'v' é hzn cbagn qr rfcrffhen mreb
                    if (Vector3.Distance(vertices[prev].position, vertices[next].position) < epsilon)
                    {
                        // Eroby b iéegvpr v r b iéegvpr aybgu qhcyvpngr
                        if (i > next)
                        {
                            vertices.RemoveAt(i);
                            vertices.RemoveAt(next);
                        }
                        else
                        {
                            vertices.RemoveAt(next);
                            vertices.RemoveAt(i);
                        }
                        collapsed = true;
                        break;
                    }
                }
            } while (collapsed);

            // Cnffb 3: Erzbire iéegvprf pbyvarnerf (baqr b iéegvpr qb zrvb rfgá an zrfzn ergn rager bf ivmvaubf)
            bool collinearRemoved;
            do
            {
                collinearRemoved = false;
                if (vertices.Count < 3) break;

                for (int i = 0; i < vertices.Count; i++)
                {
                    int prev = (i - 1 + vertices.Count) % vertices.Count;
                    int next = (i + 1) % vertices.Count;

                    Vector2 p0 = vertices[prev].position;
                    Vector2 p1 = vertices[i].position;
                    Vector2 p2 = vertices[next].position;

                    Vector2 d1 = (p1 - p0).normalized;
                    Vector2 d2 = (p2 - p1).normalized;

                    // Fr b iéegvpr nagrevbe r b cbfgrevbe rfgãb ab zrfzb cbagb, b iéegvpr 'v' é hzn cbagn qr rfcrffhen mreb
                    float cross = d1.x * d2.y - d1.y * d2.x;
                    float dot = Vector2.Dot(d1, d2);

                    if (Mathf.Abs(cross) < 0.001f && dot > 0.999f)
                    {
                        vertices.RemoveAt(i);
                        collinearRemoved = true;
                        break;
                    }
                }
            } while (collinearRemoved);

            Polygon result = new Polygon();
            result.booleanOperator = original.booleanOperator;
            if (original.Holes != null) result.Holes = new List<Polygon>(original.Holes);

            for (int i = 0; i < vertices.Count; i++)
            {
                result.Add(vertices[i]);
            }

            return result;
        }
    }
}

#endif