#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).

using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public static class MathEx
    {
        /// <summary>Since floating-point math is imprecise we use a smaller value of 0.00001 (1e-5f).</summary>
        public const float EPSILON_5 = 1e-5f;

        /// <summary>Since floating-point math is imprecise we use a smaller value of 0.0001 (1e-4f).</summary>
        public const float EPSILON_4 = 1e-4f;

        /// <summary>Since floating-point math is imprecise we use a smaller value of 0.001 (1e-3f).</summary>
        public const float EPSILON_3 = 1e-3f;

        /// <summary>Since floating-point math is imprecise we use a smaller value of 0.01 (1e-2f).</summary>
        public const float EPSILON_2 = 1e-2f;

        /// <summary>Since floating-point math is imprecise we use a smaller value of 0.1 (1e-1f).</summary>
        public const float EPSILON_1 = 1e-1f;

        /// <summary>Since floating-point math is imprecise we use a smaller value (VelcroPhysics).</summary>
        public const float EPSILON_VELCRO = 1.192092896e-07f;

        public const float TwoPi = Mathf.PI * 2.0f;

        /// <summary>
        /// Determines whether two floats are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon5(this float a, float b) => math.abs(a - b) < EPSILON_5;

        /// <summary>
        /// Determines whether two floats are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon4(this float a, float b) => math.abs(a - b) < EPSILON_4;

        /// <summary>
        /// Determines whether two floats are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon3(this float a, float b) => math.abs(a - b) < EPSILON_3;

        /// <summary>
        /// Determines whether two floats are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon2(this float a, float b) => math.abs(a - b) < EPSILON_2;

        /// <summary>
        /// Determines whether two floats are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon1(this float a, float b) => math.abs(a - b) < EPSILON_1;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon1(this Vector3 a, Vector3 b) => math.abs(a.x - b.x) < EPSILON_1 && math.abs(a.y - b.y) < EPSILON_1 && math.abs(a.z - b.z) < EPSILON_1;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon2(this Vector3 a, Vector3 b) => math.abs(a.x - b.x) < EPSILON_2 && math.abs(a.y - b.y) < EPSILON_2 && math.abs(a.z - b.z) < EPSILON_2;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon3(this Vector3 a, Vector3 b) => math.abs(a.x - b.x) < EPSILON_3 && math.abs(a.y - b.y) < EPSILON_3 && math.abs(a.z - b.z) < EPSILON_3;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon4(this Vector3 a, Vector3 b) => math.abs(a.x - b.x) < EPSILON_4 && math.abs(a.y - b.y) < EPSILON_4 && math.abs(a.z - b.z) < EPSILON_4;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon5(this Vector3 a, Vector3 b) => math.abs(a.x - b.x) < EPSILON_5 && math.abs(a.y - b.y) < EPSILON_5 && math.abs(a.z - b.z) < EPSILON_5;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon1(this Vector2 a, Vector2 b) => math.abs(a.x - b.x) < EPSILON_1 && math.abs(a.y - b.y) < EPSILON_1;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon2(this Vector2 a, Vector2 b) => math.abs(a.x - b.x) < EPSILON_2 && math.abs(a.y - b.y) < EPSILON_2;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon3(this Vector2 a, Vector2 b) => math.abs(a.x - b.x) < EPSILON_3 && math.abs(a.y - b.y) < EPSILON_3;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon4(this Vector2 a, Vector2 b) => math.abs(a.x - b.x) < EPSILON_4 && math.abs(a.y - b.y) < EPSILON_4;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon5(this Vector2 a, Vector2 b) => math.abs(a.x - b.x) < EPSILON_5 && math.abs(a.y - b.y) < EPSILON_5;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon1(this float3 a, float3 b) => math.abs(a.x - b.x) < EPSILON_1 && math.abs(a.y - b.y) < EPSILON_1 && math.abs(a.z - b.z) < EPSILON_1;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon2(this float3 a, float3 b) => math.abs(a.x - b.x) < EPSILON_2 && math.abs(a.y - b.y) < EPSILON_2 && math.abs(a.z - b.z) < EPSILON_2;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon3(this float3 a, float3 b) => math.abs(a.x - b.x) < EPSILON_3 && math.abs(a.y - b.y) < EPSILON_3 && math.abs(a.z - b.z) < EPSILON_3;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon4(this float3 a, float3 b) => math.abs(a.x - b.x) < EPSILON_4 && math.abs(a.y - b.y) < EPSILON_4 && math.abs(a.z - b.z) < EPSILON_4;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon5(this float3 a, float3 b) => math.abs(a.x - b.x) < EPSILON_5 && math.abs(a.y - b.y) < EPSILON_5 && math.abs(a.z - b.z) < EPSILON_5;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon1(this float2 a, float2 b) => math.abs(a.x - b.x) < EPSILON_1 && math.abs(a.y - b.y) < EPSILON_1;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon2(this float2 a, float2 b) => math.abs(a.x - b.x) < EPSILON_2 && math.abs(a.y - b.y) < EPSILON_2;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon3(this float2 a, float2 b) => math.abs(a.x - b.x) < EPSILON_3 && math.abs(a.y - b.y) < EPSILON_3;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon4(this float2 a, float2 b) => math.abs(a.x - b.x) < EPSILON_4 && math.abs(a.y - b.y) < EPSILON_4;

        /// <summary>
        /// Determines whether two vector's are equal, allowing for floating point differences with
        /// an Epsilon value taken into account in per component comparisons
        /// </summary>
        public static bool EqualsWithEpsilon5(this float2 a, float2 b) => math.abs(a.x - b.x) < EPSILON_5 && math.abs(a.y - b.y) < EPSILON_5;

        /// <summary>Rounds to the closest multiple of snap.</summary>
        /// <param name="value">The value to be snapped.</param>
        /// <param name="snap">The multiple to snap against.</param>
        public static float2 Snap(this float2 value, float2 snap) => new float2(Snapping.Snap(value.x, snap.x), Snapping.Snap(value.y, snap.y));

        /// <summary>Rounds to the closest multiple of snap.</summary>
        /// <param name="value">The value to be snapped.</param>
        /// <param name="snap">The multiple to snap against.</param>
        public static float Snap(this float value, float snap) => Snapping.Snap(value, snap);

        /// <summary>
        /// Linearly interpolates through a and b and c by t.
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The second value.</param>
        /// <param name="c">The end value.</param>
        /// <param name="t">The interpolation value between the three floats.</param>
        public static float Lerp3(float a, float b, float c, float t)
        {
            if (t <= 0.5f)
                return Mathf.Lerp(a, b, t * 2.0f);
            else
                return Mathf.Lerp(b, c, (t - 0.5f) * 2.0f);
        }

        /// <summary>
        /// Linearly interpolates through a and b and c by t.
        /// </summary>
        /// <param name="a">The start color.</param>
        /// <param name="b">The second color.</param>
        /// <param name="c">The end color.</param>
        /// <param name="t">The interpolation value between the three colors.</param>
        public static Color Lerp3(Color a, Color b, Color c, float t)
        {
            if (t <= 0.5f)
                return Color.Lerp(a, b, t * 2.0f);
            else
                return Color.Lerp(b, c, (t - 0.5f) * 2.0f);
        }

        public static Rect RectXYXY(float x1, float y1, float x2, float y2)
        {
            var mx1 = math.min(x1, x2);
            var my1 = math.min(y1, y2);
            var mx2 = math.max(x1, x2);
            var my2 = math.max(y1, y2);

            return new Rect(mx1, my1, mx2 - mx1, my2 - my1);
        }

        public static Rect RectXYXY(float2 a, float2 b)
        {
            var mx1 = math.min(a.x, b.x);
            var my1 = math.min(a.y, b.y);
            var mx2 = math.max(a.x, b.x);
            var my2 = math.max(a.y, b.y);
            return new Rect(mx1, my1, mx2 - mx1, my2 - my1);
        }

        public static float2 RotatePointAroundPivot(float2 point, float2 pivot, float degrees)
        {
            point -= pivot;

            var r = degrees * Mathf.Deg2Rad;
            var x = point.x * math.cos(r) - point.y * math.sin(r);
            var y = point.y * math.cos(r) + point.x * math.sin(r);
            point.x = x;
            point.y = y;

            point += pivot;
            return point;
        }

        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            return Quaternion.Euler(angles) * (point - pivot) + pivot;
        }

        public static float2 ScaleAroundPivot(float2 point, float2 pivot, float2 scale)
        {
            point -= pivot;

            point *= scale;

            point += pivot;
            return point;
        }

        public static bool IsPointOnLine(float2 point, float2 from, float2 to, float maxDistance)
        {
            return math.abs(math.distance(from, point) + math.distance(to, point) - math.distance(from, to)) < maxDistance;
        }

        public static float PointDistanceFromLine(float2 point, float2 from, float2 to)
        {
            return math.abs(math.distance(from, point) + math.distance(to, point) - math.distance(from, to));
        }

        /// <summary>Returns a positive number if c is to the left of the line going from a to b.</summary>
        /// <returns>Positive number if point is left, negative if point is right, and 0 if points are collinear.</returns>
        public static float Area(float2 a, float2 b, float2 c)
        {
            return Area(ref a, ref b, ref c);
        }

        /// <summary>Returns a positive number if c is to the left of the line going from a to b.</summary>
        /// <returns>Positive number if point is left, negative if point is right, and 0 if points are collinear.</returns>
        public static float Area(ref float2 a, ref float2 b, ref float2 c)
        {
            return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        }

        /// <summary>Returns a positive number if c is to the left of the line going from a to b.</summary>
        /// <returns>Positive number if point is left, negative if point is right, and 0 if points are collinear.</returns>
        public static float Area(Vector2 a, Vector2 b, Vector2 c)
        {
            return Area(ref a, ref b, ref c);
        }

        /// <summary>Returns a positive number if c is to the left of the line going from a to b.</summary>
        /// <returns>Positive number if point is left, negative if point is right, and 0 if points are collinear.</returns>
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        }

        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// <para>2D function that ignores the Z-coordinate of Vector3.</para>
        /// </summary>
        /// <returns>
        /// Positive number if point is left, negative if point is right, and 0 if points are collinear.
        /// </returns>
        public static float Area2D(ref Vector3 a, ref Vector3 b, ref Vector3 c)
        {
            return a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        }

        public static float2 FindNearestPointOnLine(float2 point, float2 origin, float2 end)
        {
            // get heading.
            Vector2 heading = (end - origin);
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            // do projection from the point but clamp it.
            Vector2 lhs = point - origin;
            float dotP = Vector2.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + (float2)heading * dotP;
        }

        /// <summary>Gets the point on a 4-point curve.</summary>
        /// <param name="p0">The start point.</param>
        /// <param name="p1">The first pivot point.</param>
        /// <param name="p2">The second pivot point.</param>
        /// <param name="p3">The end point.</param>
        /// <param name="t">The interpolant along the curve.</param>
        /// <returns>The point on the curve.</returns>
        public static float2 BezierGetPoint(float2 p0, float2 p1, float2 p2, float2 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float OneMinusT = 1f - t;
            return
                OneMinusT * OneMinusT * OneMinusT * p0 +
                3f * OneMinusT * OneMinusT * t * p1 +
                3f * OneMinusT * t * t * p2 +
                t * t * t * p3;
        }

        // From Eric Jordan's convex decomposition library
        /// <summary>
        /// Check if the lines a0->a1 and b0->b1 cross. If they do, intersectionPoint will be filled with the point of
        /// crossing. Grazing lines should not return true.
        /// </summary>
        public static bool LineIntersect2(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1, out Vector2 intersectionPoint)
        {
            intersectionPoint = Vector2.zero;

            if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
                return false;

            float x1 = a0.x;
            float y1 = a0.y;
            float x2 = a1.x;
            float y2 = a1.y;
            float x3 = b0.x;
            float y3 = b0.y;
            float x4 = b1.x;
            float y4 = b1.y;

            //AABB early exit
            if (Mathf.Max(x1, x2) < Mathf.Min(x3, x4) || Mathf.Max(x3, x4) < Mathf.Min(x1, x2))
                return false;

            if (Mathf.Max(y1, y2) < Mathf.Min(y3, y4) || Mathf.Max(y3, y4) < Mathf.Min(y1, y2))
                return false;

            float ua = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
            float ub = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);
            float denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
            if (Mathf.Abs(denom) < EPSILON_VELCRO)
            {
                //Lines are too close to parallel to call
                return false;
            }
            ua /= denom;
            ub /= denom;

            if (0 < ua && ua < 1 && 0 < ub && ub < 1)
            {
                intersectionPoint.x = x1 + ua * (x2 - x1);
                intersectionPoint.y = y1 + ua * (y2 - y1);
                return true;
            }

            return false;
        }

        /// <summary>Determines if three vertices are collinear (ie. on a straight line).</summary>
        public static bool IsCollinear(ref Vector2 a, ref Vector2 b, ref Vector2 c, float tolerance = 0)
        {
            return FloatInRange(Area(ref a, ref b, ref c), -tolerance, tolerance);
        }

        /// <summary>Checks if a floating point Value is within a specified range of values (inclusive).</summary>
        /// <param name="value">The Value to check.</param>
        /// <param name="min">The minimum Value.</param>
        /// <param name="max">The maximum Value.</param>
        /// <returns>True if the Value is within the range specified, false otherwise.</returns>
        public static bool FloatInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect, and, if so, the point of intersection. Use the
        /// <paramref name="firstIsSegment" /> and <paramref name="secondIsSegment" /> parameters to set whether the intersection
        /// point must be on the first and second line segments. Setting these both to true means you are doing a line-segment to
        /// line-segment intersection. Setting one of them to true means you are doing a line to line-segment intersection test,
        /// and so on. Note: If two line segments are coincident, then no intersection is detected (there are actually infinite
        /// intersection points). Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="point">This is set to the intersection point if an intersection is detected.</param>
        /// <param name="firstIsSegment">Set this to true to require that the intersection point be on the first line segment.</param>
        /// <param name="secondIsSegment">Set this to true to require that the intersection point be on the second line segment.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 point)
        {
            point = new Vector2();

            // these are reused later.
            // each lettered sub-calculation is used twice, except
            // for b and d, which are used 3 times
            float a = point4.y - point3.y;
            float b = point2.x - point1.x;
            float c = point4.x - point3.x;
            float d = point2.y - point1.y;

            // denominator to solution of linear system
            float denom = (a * b) - (c * d);

            // if denominator is 0, then lines are parallel
            if (!(denom >= -EPSILON_VELCRO && denom <= EPSILON_VELCRO))
            {
                float e = point1.y - point3.y;
                float f = point1.x - point3.x;
                float oneOverDenom = 1.0f / denom;

                // numerator of first equation
                float ua = (c * e) - (a * f);
                ua *= oneOverDenom;

                // check if intersection point of the two lines is on line segment 1
                if (!firstIsSegment || ua >= 0.0f && ua <= 1.0f)
                {
                    // numerator of second equation
                    float ub = (b * e) - (d * f);
                    ub *= oneOverDenom;

                    // check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (!secondIsSegment || ub >= 0.0f && ub <= 1.0f)
                    {
                        // check if they are coincident (no collision in this case)
                        if (ua != 0f || ub != 0f)
                        {
                            //There is an intersection
                            point.x = point1.x + ua * b;
                            point.y = point1.y + ua * d;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method detects if two line segments (or lines) intersect, and, if so, the point of intersection. Use the
        /// <paramref name="firstIsSegment" /> and <paramref name="secondIsSegment" /> parameters to set whether the intersection
        /// point must be on the first and second line segments. Setting these both to true means you are doing a line-segment to
        /// line-segment intersection. Setting one of them to true means you are doing a line to line-segment intersection test,
        /// and so on. Note: If two line segments are coincident, then no intersection is detected (there are actually infinite
        /// intersection points). Author: Jeremy Bell
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection point if an intersection is detected.</param>
        /// <param name="firstIsSegment">Set this to true to require that the intersection point be on the first line segment.</param>
        /// <param name="secondIsSegment">Set this to true to require that the intersection point be on the second line segment.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, bool firstIsSegment, bool secondIsSegment, out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
        }

        /// <summary>
        /// This method detects if two line segments intersect, and, if so, the point of intersection. Note: If two line
        /// segments are coincident, then no intersection is detected (there are actually infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(ref Vector2 point1, ref Vector2 point2, ref Vector2 point3, ref Vector2 point4, out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        /// <summary>
        /// This method detects if two line segments intersect, and, if so, the point of intersection. Note: If two line
        /// segments are coincident, then no intersection is detected (there are actually infinite intersection points).
        /// </summary>
        /// <param name="point1">The first point of the first line segment.</param>
        /// <param name="point2">The second point of the first line segment.</param>
        /// <param name="point3">The first point of the second line segment.</param>
        /// <param name="point4">The second point of the second line segment.</param>
        /// <param name="intersectionPoint">This is set to the intersection point if an intersection is detected.</param>
        /// <returns>True if an intersection is detected, false otherwise.</returns>
        public static bool LineIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        /// <summary>Represents a spline that uses 3 points.</summary>
        [System.Serializable]
        public class Spline3
        {
            /// <summary>The points of the spline.</summary>
            [SerializeField]
            private Vector3[] points;

            /// <summary>Initializes a new instance of the <see cref="Spline3"/> class.</summary>
            /// <param name="points">The points that make up the spline.</param>
            public Spline3(Vector3[] points)
            {
                Debug.Assert(points.Length >= 3, "Tried to create a Spline3 with less than 3 points.");
                this.points = points;
            }

            /// <summary>Gets the point on a 3-point curve.</summary>
            /// <param name="p0">The start point.</param>
            /// <param name="p1">The pivot point.</param>
            /// <param name="p2">The end point.</param>
            /// <param name="t">The interpolant along the curve.</param>
            /// <returns>The point on the curve.</returns>
            public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
            {
                t = Mathf.Clamp01(t);
                float oneMinusT = 1f - t;
                return
                    oneMinusT * oneMinusT * p0 +
                    2f * oneMinusT * t * p1 +
                    t * t * p2;
            }

            public static Vector3 GetFirstDerivative(Vector3 p0, Vector3 p1, Vector3 p2, float t)
            {
                return
                    2f * (1f - t) * (p1 - p0) +
                    2f * t * (p2 - p1);
            }

            /// <summary>Gets a point on the spline.</summary>
            /// <param name="t">The interpolant on the spline to get the position for.</param>
            /// <returns>The point on the spline.</returns>
            public Vector3 GetPoint(float t)
            {
                int i;
                if (t >= 1f)
                {
                    t = 1f;
                    i = points.Length - 3;
                }
                else
                {
                    t = Mathf.Clamp01(t) * CurveCount;
                    i = (int)t;
                    t -= i;
                    i *= 2;
                }
                return GetPoint(points[i], points[i + 1], points[i + 2], t);
            }

            /// <summary>Gets the velocity on the spline.</summary>
            /// <param name="t">The interpolant on the spline to get the velocity for.</param>
            /// <returns>The velocity on the spline.</returns>
            public Vector3 GetVelocity(float t)
            {
                int i;
                if (t >= 1f)
                {
                    t = 1f;
                    i = points.Length - 3;
                }
                else
                {
                    t = Mathf.Clamp01(t) * CurveCount;
                    i = (int)t;
                    t -= i;
                    i *= 2;
                }
                return GetFirstDerivative(points[i], points[i + 1], points[i + 2], t);
            }

            /// <summary>Gets the direction the spline is moving in.</summary>
            /// <param name="t">The interpolant on the spline to get the direction for.</param>
            /// <returns>The direction on the spline.</returns>
            public Vector3 GetDirection(float t)
            {
                return GetVelocity(t).normalized;
            }

            /// <summary>Gets the amount of curves in this spline.</summary>
            /// <value>The amount of curves.</value>
            public int CurveCount
            {
                get
                {
                    return (points.Length - 1) / 2;
                }
            }

            public Vector3 GetRight(float t)
            {
                var A = GetPoint(t - 0.001f);
                var B = GetPoint(t + 0.001f);
                var delta = (B - A);
                return new Vector3(-delta.z, 0, delta.x).normalized;
            }

            public Vector3 GetForward(float t)
            {
                var A = GetPoint(t - 0.001f);
                var B = GetPoint(t + 0.001f);
                return (B - A).normalized;
            }

            public Vector3 GetUp(float t)
            {
                var A = GetPoint(t - 0.001f);
                var B = GetPoint(t + 0.001f);
                var delta = (B - A).normalized;
                return Vector3.Cross(delta, GetRight(t));
            }

            /// <summary>Calculates the physical length of the spline.</summary>
            public float GetLength(int segments)
            {
                float length = 0.0f;
                Vector3 lastPoint = GetPoint(0.0f);
                for (int i = 1; i < segments + 1; i++)
                {
                    Vector3 nextPoint = GetPoint(i / (float)segments);
                    length += Vector3.Distance(lastPoint, nextPoint);
                    lastPoint = nextPoint;
                }
                return length;
            }

            /// <summary>
            /// Walks through the spline and tries to find the interpolant that's closest to the
            /// specified position.
            /// </summary>
            public float FindNearestPointForwards(Vector3 position, int segments)
            {
                float bestDistance = float.MaxValue;
                float bestT = 0.0f;
                for (int i = 0; i < segments + 1; i++)
                {
                    float t = i / (float)segments;
                    Vector3 point = GetPoint(t);
                    float distance = Vector3.Distance(point, position);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestT = t;
                    }
                }
                return bestT;
            }
        }
    }
}

#endif