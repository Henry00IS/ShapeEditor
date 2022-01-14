#if UNITY_EDITOR

using AeternumGames.ShapeEditor.PolyBoolCS;
using System.Collections.Generic;
using PolyboolPolygon = AeternumGames.ShapeEditor.PolyBoolCS.Polygon;
using Region = AeternumGames.ShapeEditor.PolyBoolCS.PointList;

namespace AeternumGames.ShapeEditor
{
    // contains source code from https://github.com/velipso/polybooljs (see Licenses/PolyBoolJS.txt).

    public static class PolyboolExtensions
    {
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