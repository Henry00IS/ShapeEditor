#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of vertex points that make up a 3D polygon.</summary>
    public class Polygon3D : List<Vector3>
    {
        /// <summary>
        /// After calling <see cref="RecalculatePlane"/> a plane that approximately resembles the polygon.
        /// </summary>
        public Plane plane;

        public Polygon3D()
        {
        }

        public Polygon3D(int capacity) : base(capacity) { }

        public Polygon3D(IEnumerable<Vector3> vertices)
        {
            AddRange(vertices);
        }

        public Polygon3D(Polygon2D polygon)
        {
            int count = polygon.Count;
            for (int i = 0; i < count; i++)
                Add(new Vector3(polygon[i].x, -polygon[i].y));
        }

        /// <summary>Calculates a plane that approximately resembles the polygon.</summary>
        public Plane RecalculatePlane()
        {
            int count = Count;
            Debug.Assert(count >= 3, "Attempted to calculate plane of a 3D polygon with less than 3 vertices.");

            plane = new Plane(this[0], this[1], this[2]);

            // hack: if the plane's normal is zero and there's more than 3 vertices,
            // try using alternative vertices to construct the plane.
            if (plane.normal == Vector3.zero && count > 3)
            {
                // iterate through the available vertices.
                for (int i = 1; i < count; i++)
                {
                    if (i + 1 >= count || i + 2 >= count) break;

                    // use the next 3 vertices construct a new plane.
                    plane = new Plane(this[i], this[i + 1], this[i + 2]);
                    // stop once we found a valid normal.
                    if (plane.normal != Vector3.zero)
                        break;
                }
            }

            Debug.Assert(plane.normal != Vector3.zero, "Attempted to calculate the plane of a 3D polygon but got a zero normal.");
            return plane;
        }

        /// <summary>
        /// Extrudes this polygon by the specified distance along its normal and returns the
        /// extruded polygons.
        /// </summary>
        /// <param name="distance">The distance to extrude by.</param>
        /// <returns>The extruded polygons.</returns>
        public List<Polygon3D> Extrude(float distance)
        {
            int count = Count;
            Debug.Assert(count >= 3, "Attempted to extrude a 3D polygon with less than 3 vertices.");
            var results = new List<Polygon3D>(count - 1);

            RecalculatePlane();
            var normal = plane.normal;

            for (int i = 0; i < count - 1; i++)
            {
                results.Add(new Polygon3D(new Vector3[] {
                    this[i],
                    this[i] - normal * distance,
                    this[i + 1] - normal * distance,
                    this[i + 1],
                }));
            }

            return results;
        }

        /// <summary>
        /// Extrudes this polygon by the distance from the given plane along its normal and returns
        /// the extruded polygons.
        /// </summary>
        /// <param name="clippingPlane">The plane to extrude against.</param>
        /// <returns>The extruded polygons.</returns>
        public List<Polygon3D> ExtrudeAgainstPlane(Plane clippingPlane)
        {
            int count = Count;
            Debug.Assert(count >= 3, "Attempted to extrude a 3D polygon with less than 3 vertices.");
            var results = new List<Polygon3D>(count - 1);

            RecalculatePlane();
            var normal = plane.normal;

            for (int i = 0; i < count - 1; i++)
            {
                results.Add(new Polygon3D(new Vector3[] {
                    this[i],
                    this[i] + normal * clippingPlane.GetDistanceToPoint(this[i]),
                    this[i + 1] + normal * clippingPlane.GetDistanceToPoint(this[i + 1]),
                    this[i + 1],
                }));
            }

            return results;
        }

        /// <summary>Generates UV0 coordinates using the AutoUV algorithm of SabreCSG.</summary>
        public Vector2[] GenerateUV_SabreCSG()
        {
            int count = Count;
            var results = new Vector2[count];

            RecalculatePlane();
            var normal = plane.normal;

            var cancellingRotation = Quaternion.Inverse(Quaternion.LookRotation(-normal));
            // Sets the UV at each point to the position on the plane
            for (int i = 0; i < results.Length; i++)
            {
                Vector3 position = this[i];
                Vector2 uv = new Vector3(0.5f, 0.5f, 0f) + (cancellingRotation * position);
                results[i] = uv;
            }

            return results;
        }

        /// <summary>Gets a flipped copy of the polygon.</summary>
        public Polygon3D flipped
        {
            get
            {
                var polygon = new Polygon3D(this);
                polygon.Reverse();
                return polygon;
            }
        }

        /// <summary>
        /// Translates this polygon by the specified amount.
        /// </summary>
        /// <param name="position">The position to be added to all vertices.</param>
        public void Translate(Vector3 position)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                this[i] += position;
        }

        /// <summary>
        /// Translates the vertices along the polygon normal against the plane.
        /// </summary>
        /// <param name="projectPlane">The plane to project the vertices against.</param>
        public void ProjectOnPlane(Plane projectPlane)
        {
            int count = Count;

            RecalculatePlane();
            var normal = plane.normal;

            for (int i = 0; i < count; i++)
                this[i] -= normal * projectPlane.GetDistanceToPoint(this[i]);
        }
    }
}

#endif