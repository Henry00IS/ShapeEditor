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