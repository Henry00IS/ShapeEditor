#if UNITY_EDITOR

using AeternumGames.ShapeEditor.PolyBoolCS;
using System.Collections.Generic;
using UnityEngine;
using PolyboolPolygon = AeternumGames.ShapeEditor.PolyBoolCS.Polygon;
using Region = AeternumGames.ShapeEditor.PolyBoolCS.PointList;

namespace AeternumGames.ShapeEditor
{
    // contains source code from https://github.com/velipso/polybooljs (see Licenses/PolyBoolJS.txt).

    public static class PolyboolExtensions
    {
        // Threshold for detecting "thin" connections (T-Junctions)
        // A connection is considered a singularity if the edge length ratio is below this threshold
        private const float ThinEdgeRatioThreshold = 0.001f;
        
        // Minimum distance threshold for detecting T-Junction points (in screen coordinates)
        // Used when checking if a vertex lies on an edge
        private const float TJunctionPointThreshold = 1e-6f;

        public static PolyboolPolygon ToPolybool(this Polygon polygon)
        {
            var count = polygon.Count;
            var points = new Region(count);
            for (int i = 0; i < count; i++)
                points.Add(new Point(polygon[i].position.x, polygon[i].position.y));

            var p1 = new PolyboolPolygon
            {
                regions = new List<Region>() { points }
            };

            return p1;
        }

        private class MyNode
        {
            public Region region;
            public List<MyNode> children = new List<MyNode>();

            public MyNode(Region region)
            {
                this.region = region;
            }
        }

        /// <summary>
        /// Detects and splits polygons that have singularities (T-Junctions) caused by Boolean operations
        /// incorrectly merging shapes at a single point. This prevents BayazitDecomposer stack overflow.
        /// </summary>
        /// <param name="polygons">The list of concave polygons to process.</param>
        /// <returns>A new list with singularities split into separate polygons.</returns>
        public static List<Polygon> SplitSingularities(this List<Polygon> polygons)
        {
            var result = new List<Polygon>();
            
            foreach (var polygon in polygons)
            {
                var splitPolygons = SplitPolygonSingularities(polygon);
                result.AddRange(splitPolygons);
            }
            
            return result;
        }

        /// <summary>
        /// Splits a polygon if it contains singularities (T-Junctions) that would cause 
        /// BayazitDecomposer to fail with stack overflow.
        /// </summary>
        private static List<Polygon> SplitPolygonSingularities(Polygon polygon)
        {
            if (polygon.Count < 3)
                return new List<Polygon> { polygon };

            // First, try to detect and split at true T-Junctions (vertices lying on edges)
            var tjunctionResults = SplitAtTrueTJunctions(polygon);
            if (tjunctionResults != null && tjunctionResults.Count > 1)
            {
                return tjunctionResults;
            }

            // Fallback: try the old thin edge detection
            // Calculate the average edge length
            float totalEdgeLength = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                var p1 = polygon[i].position;
                var p2 = polygon.NextVertex(i).position;
                totalEdgeLength += Vector2.Distance(p1, p2);
            }
            float averageEdgeLength = totalEdgeLength / polygon.Count;

            // Find edges that are significantly shorter than the average
            // These are potential T-Junction connections
            var thinEdges = new List<int>(); // indices of thin edges
            
            for (int i = 0; i < polygon.Count; i++)
            {
                var p1 = polygon[i].position;
                var p2 = polygon.NextVertex(i).position;
                float edgeLength = Vector2.Distance(p1, p2);
                
                if (edgeLength < averageEdgeLength * ThinEdgeRatioThreshold)
                {
                    thinEdges.Add(i);
                }
            }

            // If we found potential T-Junction edges, try to split the polygon
            if (thinEdges.Count > 0)
            {
                var splitResult = TrySplitAtThinEdges(polygon, thinEdges);
                if (splitResult != null && splitResult.Count > 0)
                {
                    return splitResult;
                }
            }

            // Last resort: try geometric split based on bottleneck detection
            var geometricResult = TryGeometricSplit(polygon);
            if (geometricResult != null && geometricResult.Count > 1)
            {
                return geometricResult;
            }

            return new List<Polygon> { polygon };
        }

        /// <summary>
        /// Detects and splits polygons at true T-Junctions where vertices lie on other edges.
        /// This is the core fix for the T-Junction bug.
        /// </summary>
        private static List<Polygon> SplitAtTrueTJunctions(Polygon polygon)
        {
            if (polygon.Count < 4)
                return null;

            // Calculate polygon bounds to normalize our threshold
            var bounds = polygon.GetAABB();
            float polygonSize = Mathf.Max(bounds.size.x, bounds.size.y);
            float threshold = polygonSize * ThinEdgeRatioThreshold;
            if (threshold < TJunctionPointThreshold)
                threshold = TJunctionPointThreshold;

            // Find vertices that lie on other edges (T-Junction points)
            var tjunctionVertices = new List<int>(); // indices of vertices that lie on other edges

            for (int i = 0; i < polygon.Count; i++)
            {
                var vi = polygon[i].position;
                
                // Check if this vertex lies on any OTHER edge of the polygon
                for (int edgeStart = 0; edgeStart < polygon.Count; edgeStart++)
                {
                    int edgeEnd = (edgeStart + 1) % polygon.Count;
                    
                    // Skip edges that are incident to this vertex
                    if (edgeStart == i || edgeEnd == i)
                        continue;
                    
                    var vStart = polygon[edgeStart].position;
                    var vEnd = polygon[edgeEnd].position;
                    
                    // Check if vertex vi lies on the edge from vStart to vEnd
                    if (IsPointOnEdge(vi, vStart, vEnd, threshold))
                    {
                        // Found a T-Junction! This vertex lies on another edge
                        tjunctionVertices.Add(i);
                        break; // Only need to find one T-Junction per vertex
                    }
                }
            }

            // If we found T-Junction vertices, try to split at each one
            if (tjunctionVertices.Count > 0)
            {
                // Sort in descending order to remove from end first (avoid index shifting)
                tjunctionVertices.Sort((a, b) => b.CompareTo(a));
                
                foreach (int tjunctionIndex in tjunctionVertices)
                {
                    var result = TrySplitAtTJunctionVertex(polygon, tjunctionIndex);
                    if (result != null && result.Count > 1)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Checks if a point lies on an edge (within a threshold distance).
        /// </summary>
        private static bool IsPointOnEdge(Vector2 point, Vector2 edgeStart, Vector2 edgeEnd, float threshold)
        {
            // First check if point is collinear with the edge using cross product (z-component)
            // Cross product of (edgeEnd - edgeStart) and (point - edgeStart)
            float crossZ = (edgeEnd.x - edgeStart.x) * (point.y - edgeStart.y) - 
                           (edgeEnd.y - edgeStart.y) * (point.x - edgeStart.x);
            float edgeLength = (edgeEnd - edgeStart).magnitude;
            
            if (edgeLength < threshold)
                return false; // Edge is too short
                
            // Check if point is collinear (cross product is near zero)
            if (Mathf.Abs(crossZ) / edgeLength > threshold)
                return false; // Not collinear
                
            // Check if point is within the bounding box of the edge
            float minX = Mathf.Min(edgeStart.x, edgeEnd.x) - threshold;
            float maxX = Mathf.Max(edgeStart.x, edgeEnd.x) + threshold;
            float minY = Mathf.Min(edgeStart.y, edgeEnd.y) - threshold;
            float maxY = Mathf.Max(edgeStart.y, edgeEnd.y) + threshold;
            
            if (point.x < minX || point.x > maxX || point.y < minY || point.y > maxY)
                return false; // Point is outside edge bounds
                
            // Check if point is between the endpoints (not at endpoints)
            float dotProduct = Vector2.Dot(point - edgeStart, edgeEnd - edgeStart);
            float squaredLength = (edgeEnd - edgeStart).sqrMagnitude;
            
            // Point is on the edge (but not at endpoints) if 0 < dot < squaredLength
            return dotProduct > threshold && dotProduct < squaredLength - threshold;
        }

        /// <summary>
        /// Attempts to split the polygon at a T-Junction vertex.
        /// </summary>
        private static List<Polygon> TrySplitAtTJunctionVertex(Polygon polygon, int tjunctionIndex)
        {
            int n = polygon.Count;
            if (n < 5)
                return null;

            // Split the polygon at the T-Junction vertex
            // We create two polygons: one going from T-Junction to T-Junction, the other as the rest
            
            // Try different split points to find one that works
            for (int offset = 2; offset < n - 2; offset++)
            {
                int splitIndex = (tjunctionIndex + offset) % n;
                
                var poly1 = new Polygon();
                var poly2 = new Polygon();
                
                // First polygon: from tjunctionIndex to splitIndex (inclusive)
                int count1 = 0;
                for (int i = tjunctionIndex; ; i = (i + 1) % n)
                {
                    poly1.Add(polygon[i]);
                    count1++;
                    if (i == splitIndex)
                        break;
                    if (count1 > n) break; // Safety check
                }
                
                // Second polygon: from splitIndex to tjunctionIndex (inclusive)
                int count2 = 0;
                for (int i = splitIndex; ; i = (i + 1) % n)
                {
                    poly2.Add(polygon[i]);
                    count2++;
                    if (i == tjunctionIndex)
                        break;
                    if (count2 > n) break; // Safety check
                }

                // Verify both are valid polygons with area
                if (poly1.Count >= 3 && poly2.Count >= 3)
                {
                    float area1 = Mathf.Abs(poly1.GetSignedArea2D());
                    float area2 = Mathf.Abs(poly2.GetSignedArea2D());
                    
                    if (area1 > 0.001f && area2 > 0.001f && poly1.IsSimple() && poly2.IsSimple())
                    {
                        // Verify the split actually separated the T-Junction
                        bool stillHasTJunction = false;
                        for (int i = 0; i < poly1.Count && !stillHasTJunction; i++)
                        {
                            var vi = poly1[i].position;
                            for (int edgeStart = 0; edgeStart < poly1.Count && !stillHasTJunction; edgeStart++)
                            {
                                int edgeEnd = (edgeStart + 1) % poly1.Count;
                                if (edgeStart == i || edgeEnd == i) continue;
                                if (IsPointOnEdge(vi, poly1[edgeStart].position, poly1[edgeEnd].position, 0.001f))
                                    stillHasTJunction = true;
                            }
                        }
                        
                        if (!stillHasTJunction)
                        {
                            return new List<Polygon> { poly1, poly2 };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Attempts to split the polygon at the thin edges (potential T-Junctions).
        /// </summary>
        private static List<Polygon> TrySplitAtThinEdges(Polygon polygon, List<int> thinEdgeIndices)
        {
            // Try each thin edge as a potential split point
            foreach (int thinEdgeIndex in thinEdgeIndices)
            {
                var result = TrySplitAtEdge(polygon, thinEdgeIndex);
                if (result != null && result.Count > 1)
                {
                    // Successfully split - recursively check the resulting polygons
                    var finalResult = new List<Polygon>();
                    foreach (var subPolygon in result)
                    {
                        if (subPolygon.Count >= 3)
                        {
                            finalResult.Add(subPolygon);
                        }
                    }
                    if (finalResult.Count > 1)
                    {
                        return finalResult;
                    }
                }
            }

            // If simple edge splitting doesn't work, try a more sophisticated approach
            return TryGeometricSplit(polygon);
        }

        /// <summary>
        /// Attempts to split the polygon at the specified edge by dividing it into two polygons.
        /// </summary>
        private static List<Polygon> TrySplitAtEdge(Polygon polygon, int edgeIndex)
        {
            if (polygon.Count < 4)
                return null;

            int n = polygon.Count;
            
            // Get the vertices around the thin edge
            int prevIndex = (edgeIndex - 1 + n) % n;
            int nextIndex = (edgeIndex + 1) % n;

            var v1 = polygon[prevIndex].position;
            var v2 = polygon[edgeIndex].position;
            var v3 = polygon[nextIndex].position;

            // Create two polygons by cutting at the thin edge
            // First polygon: from edgeIndex+1 to edgeIndex (inclusive)
            var poly1 = new Polygon();
            for (int i = edgeIndex; i < n; i++)
            {
                poly1.Add(polygon[i]);
            }
            for (int i = 0; i <= prevIndex; i++)
            {
                poly1.Add(polygon[i]);
            }

            // Second polygon: from prevIndex+1 to prevIndex (the other way around)
            var poly2 = new Polygon();
            for (int i = prevIndex; i < n; i++)
            {
                poly2.Add(polygon[i]);
            }
            for (int i = 0; i <= edgeIndex; i++)
            {
                poly2.Add(polygon[i]);
            }

            // Check if both resulting polygons are valid (have area)
            if (poly1.Count >= 3 && poly2.Count >= 3 &&
                Mathf.Abs(poly1.GetSignedArea2D()) > 0.001f &&
                Mathf.Abs(poly2.GetSignedArea2D()) > 0.001f)
            {
                // Verify the split makes geometric sense (both polygons should be simple)
                if (poly1.IsSimple() && poly2.IsSimple())
                {
                    return new List<Polygon> { poly1, poly2 };
                }
            }

            return null;
        }

        /// <summary>
        /// A more sophisticated geometric approach to split polygons with singularities.
        /// Uses the fact that T-Junction polygons have a "bottleneck" - a very narrow connection.
        /// </summary>
        private static List<Polygon> TryGeometricSplit(Polygon polygon)
        {
            if (polygon.Count < 3)
                return new List<Polygon> { polygon };

            // Find the vertex with the minimum distance to other non-adjacent vertices
            // This is likely the T-Junction point
            int bottleneckVertex = -1;
            float minBottleneckDistance = float.MaxValue;

            for (int i = 0; i < polygon.Count; i++)
            {
                var vi = polygon[i].position;
                
                for (int j = i + 2; j < polygon.Count; j++)
                {
                    // Skip adjacent vertices
                    if (j == i + 1 || (i == 0 && j == polygon.Count - 1))
                        continue;

                    var vj = polygon[j].position;
                    float dist = Vector2.Distance(vi, vj);
                    
                    if (dist < minBottleneckDistance)
                    {
                        minBottleneckDistance = dist;
                        bottleneckVertex = i;
                    }
                }
            }

            if (bottleneckVertex >= 0)
            {
                // Calculate the polygon dimensions to normalize the threshold
                var bounds = polygon.GetAABB();
                float size = Mathf.Max(bounds.size.x, bounds.size.y);
                float threshold = size * ThinEdgeRatioThreshold;

                if (minBottleneckDistance < threshold)
                {
                    // This polygon has a bottleneck - try to split it
                    return TrySplitAtVertex(polygon, bottleneckVertex);
                }
            }

            return new List<Polygon> { polygon };
        }

        /// <summary>
        /// Attempts to split the polygon at the specified vertex.
        /// </summary>
        private static List<Polygon> TrySplitAtVertex(Polygon polygon, int vertexIndex)
        {
            int n = polygon.Count;
            if (n < 5) // Need at least 5 vertices to have a valid split
                return new List<Polygon> { polygon };

            // Create two polygons by going around the original polygon
            // starting from the bottleneck vertex
            var poly1 = new Polygon();
            var poly2 = new Polygon();

            // First polygon: vertexIndex to vertexIndex + n/2
            int splitPoint = n / 2;
            for (int i = 0; i <= splitPoint; i++)
            {
                poly1.Add(polygon[(vertexIndex + i) % n]);
            }

            // Second polygon: the rest
            for (int i = splitPoint; i < n; i++)
            {
                poly2.Add(polygon[(vertexIndex + i) % n]);
            }

            // Add the split point to both to close them properly
            poly1.Add(polygon[(vertexIndex + splitPoint) % n]);
            poly2.Add(polygon[vertexIndex]);

            // Verify both are valid
            if (poly1.Count >= 3 && poly2.Count >= 3 &&
                Mathf.Abs(poly1.GetSignedArea2D()) > 0.001f &&
                Mathf.Abs(poly2.GetSignedArea2D()) > 0.001f &&
                poly1.IsSimple() && poly2.IsSimple())
            {
                return new List<Polygon> { poly1, poly2 };
            }

            return new List<Polygon> { polygon };
        }

        public static List<Polygon> ToPolygons(this PolyboolPolygon poly, PolyBool polyBool)
        {
            // make sure out polygon is clean
            poly = polyBool.polygon(polyBool.segments(poly));

            var roots = newNode(null);

            // add all regions to the root
            for (var i = 0; i < poly.regions.Count; i++)
            {
                var region = poly.regions[i];
                if (region.Count < 3) // regions must have at least 3 points (sanity check)
                    continue;
                addChild(roots, region);
            }

            // with our heirarchy, we can distinguish between exterior borders, and interior holes
            // the root nodes are exterior, children are interior, children's children are exterior,
            // children's children's children are interior, etc

            // while we're at it, exteriors are counter-clockwise, and interiors are clockwise

            var geopolys = new List<Region>();

            // root nodes are exterior
            for (var i = 0; i < roots.children.Count; i++)
                addExterior(roots.children[i], geopolys);

            var sePolys = new List<Polygon>();
            foreach (var region in geopolys)
            {
                var sePoly = new Polygon();
                var pointCount = region.Count;
                for (int i = 0; i < pointCount; i++)
                {
                    sePoly.Add(new Vertex((float)region[i].x, (float)region[i].y));
                }
                sePolys.Add(sePoly);
            }

            // Apply singularity splitting to prevent BayazitDecomposer stack overflow
            sePolys = sePolys.SplitSingularities();

            return sePolys;
        }

        // test if r1 is inside r2
        private static bool regionInsideRegion(Region r1, Region r2)
        {
            // we're guaranteed no lines intersect (because the polygon is clean), but a vertex
            // could be on the edge -- so we just average pt[0] and pt[1] to produce a point on the
            // edge of the first line, which cannot be on an edge
            return Epsilon.pointInsideRegion(new Point(
                (r1[0].x + r1[1].x) * 0.5,
                (r1[0].y + r1[1].y) * 0.5
            ), r2);
        }

        // calculate inside hierarchy
        //
        //  _____________________   _______    roots -> A       -> F
        // |          A          | |   F   |            |          |
        // |  _______   _______  | |  ___  |            +-- B      +-- G
        // | |   B   | |   C   | | | |   | |            |   |
        // | |  ___  | |  ___  | | | |   | |            |   +-- D
        // | | | D | | | | E | | | | | G | |            |
        // | | |___| | | |___| | | | |   | |            +-- C
        // | |_______| |_______| | | |___| |                |
        // |_____________________| |_______|                +-- E
        private static MyNode newNode(Region region)
        {
            return new MyNode(region);
        }

        private static void addChild(MyNode root, Region region)
        {
            // first check if we're inside any children
            for (var i = 0; i < root.children.Count; i++)
            {
                var child = root.children[i];
                if (regionInsideRegion(region, child.region))
                {
                    // we are, so insert inside them instead
                    addChild(child, region);
                    return;
                }
            }

            // not inside any children, so check to see if any children are inside us
            var node = newNode(region);
            for (var i = 0; i < root.children.Count; i++)
            {
                var child = root.children[i];
                if (regionInsideRegion(child.region, region))
                {
                    // oops... move the child beneath us, and remove them from root
                    node.children.Add(child);
                    root.children.Splice(i, 1);
                    i--;
            }

            // now we can add ourselves
            root.children.Add(node);
        }

        private static Region forceWinding(Region region, bool clockwise)
        {
            // first, see if we're clockwise or counter-clockwise
            // https://en.wikipedia.org/wiki/Shoelace_formula
            var winding = 0.0;
            var last_x = region[region.Count - 1].x;
            var last_y = region[region.Count - 1].y;
            var copy = new Region();
            for (var i = 0; i < region.Count; i++)
            {
                var curr_x = region[i].x;
                var curr_y = region[i].y;
                copy.Add(new Point(curr_x, curr_y)); // create a copy while we're at it
                winding += curr_y * last_x - curr_x * last_y;
                last_x = curr_x;
                last_y = curr_y;
            }
            // this assumes Cartesian coordinates (Y is positive going up)
            var isclockwise = winding < 0;
            if (isclockwise != clockwise)
                copy.Reverse();
            // while we're here, the last point must be the first point...
            //copy.Add(new Point(copy[0].x, copy[0].y));
            return copy;
        }

        private static void addExterior(MyNode node, List<Region> polygons)
        {
            var poly = new List<Region>();
            poly.Add(forceWinding(node.region, false));
            polygons.AddRange(poly);
            // children of exteriors are interior
            for (var i = 0; i < node.children.Count; i++)
                polygons.Add(getInterior(node.children[i], polygons));
        }

        private static Region getInterior(MyNode node, List<Region> polygons)
        {
            // children of interiors are exterior
            for (var i = 0; i < node.children.Count; i++)
                addExterior(node.children[i], polygons);
            // return the clockwise interior
            return forceWinding(node.region, true);
        }
    }
}

#endif
