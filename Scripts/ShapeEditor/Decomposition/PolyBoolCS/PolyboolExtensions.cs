#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public static class PolyboolExtensions
    {
        public static PolyBoolCS.Polygon ToPolyboolCS(this Polygon polygon)
        {
            var count = polygon.Count;
            var points = new PolyBoolCS.PointList(count);
            for (int i = 0; i < count; i++)
                points.Add(new PolyBoolCS.Point(polygon[i].position.x, polygon[i].position.y));

            var p1 = new PolyBoolCS.Polygon
            {
                regions = new List<PolyBoolCS.PointList>() { points }
            };

            return p1;
        }

        public static List<Polygon> ToPolygonsCS(this PolyBoolCS.Polygon polygon)
        {
            var result = new List<Polygon>();

            Debug.Log(polygon.inverted);
            foreach (var region in polygon.regions)
            {
                var poly = new Polygon();
                foreach (var point in region)
                {
                    poly.Add(new Vertex((float)point.x, (float)point.y));
                }
                result.Add(poly);
            }

            return result;
        }
    }
}

#endif