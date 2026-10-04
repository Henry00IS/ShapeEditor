#if UNITY_EDITOR
using System.Collections.Generic;

namespace AeternumGames.ShapeEditor.PolyBoolCS
{
    internal static class SingularRegionSplitter
    {
        internal static List<PointList> Split(List<PointList> regions)
        {
            var result = new List<PointList>();
            foreach (var region in regions)
            {
                var boundary = InsertContacts(region);
                var path = new PointList();
                for (int i = 0; i <= boundary.Count && boundary.Count > 0; i++)
                {
                    var point = boundary[i % boundary.Count];
                    var previous = FindPoint(path, point);
                    if (previous < 0)
                    {
                        path.Add(point);
                        continue;
                    }

                    var loop = new PointList();
                    for (int j = previous; j < path.Count; j++)
                        loop.Add(path[j]);
                    if (loop.Count >= 3)
                        result.Add(loop);
                    path.RemoveRange(previous + 1, path.Count - previous - 1);
                }
            }
            return result;
        }

        private static PointList InsertContacts(PointList region)
        {
            var boundary = new PointList();
            for (int i = 0; i < region.Count; i++)
            {
                var left = region[i];
                var right = region[(i + 1) % region.Count];
                if (Epsilon.pointsSame(left, right))
                    continue;
                boundary.Add(left);
                var contacts = new PointList();
                for (int j = 0; j < region.Count; j++)
                {
                    var candidate = region[j];
                    if (Epsilon.pointsCollinear(left, right, candidate) &&
                        Epsilon.pointBetween(candidate, left, right) &&
                        FindPoint(contacts, candidate) < 0)
                        contacts.Add(candidate);
                }
                contacts.Sort((a, b) => DistanceSquared(left, a).CompareTo(DistanceSquared(left, b)));
                boundary.AddRange(contacts);
            }
            return boundary;
        }

        private static int FindPoint(PointList points, Point candidate)
        {
            for (int i = 0; i < points.Count; i++)
                if (Epsilon.pointsSame(points[i], candidate))
                    return i;
            return -1;
        }

        private static double DistanceSquared(Point a, Point b)
        {
            var x = a.x - b.x;
            var y = a.y - b.y;
            return x * x + y * y;
        }
    }
}
#endif
