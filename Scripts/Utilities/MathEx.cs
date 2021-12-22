#if UNITY_EDITOR

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
    }
}

#endif