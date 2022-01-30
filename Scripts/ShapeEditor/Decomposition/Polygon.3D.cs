#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).
// contains source code from https://github.com/sabresaurus/SabreCSG (see Licenses/SabreCSG.txt).

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of vertex points that make up a 2D/3D polygon.</summary>
    public partial class Polygon : List<Vertex>
    {
        /// <summary>
        /// [3D] After calling <see cref="RecalculatePlane"/> a plane that approximately resembles the polygon.
        /// </summary>
        public Plane plane;

        /// <summary>[3D] Calculates a plane that approximately resembles the polygon.</summary>
        public Plane RecalculatePlane()
        {
            int count = Count;
            Debug.Assert(count >= 3, "Attempted to calculate plane of a 3D polygon with less than 3 vertices.");

            plane = new Plane(this[0].position, this[1].position, this[2].position);

            // hack: if the plane's normal is zero and there's more than 3 vertices,
            // try using alternative vertices to construct the plane.
            if (plane.normal == Vector3.zero && count > 3)
            {
                // iterate through the available vertices.
                for (int i = 1; i < count; i++)
                {
                    if (i + 1 >= count || i + 2 >= count) break;

                    // use the next 3 vertices construct a new plane.
                    plane = new Plane(this[i].position, this[i + 1].position, this[i + 2].position);
                    // stop once we found a valid normal.
                    if (plane.normal != Vector3.zero)
                        break;
                }
            }

            Debug.Assert(plane.normal != Vector3.zero, "Attempted to calculate the plane of a 3D polygon but got a zero normal.");
            return plane;
        }

        /// <summary>[3D] Gets a flipped copy of the polygon by reversing the winding order.</summary>
        public Polygon flipped
        {
            get
            {
                var polygon = new Polygon(this);
                polygon.Reverse();
                return polygon;
            }
        }

        /// <summary>[3D] Rotates this polygon by the specified amount.</summary>
        /// <param name="rotation">The rotation to rotate all vertices by.</param>
        public void Rotate(Quaternion rotation)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
                this[i] = new Vertex(rotation * this[i].position, this[i].uv0, this[i].hidden);
        }

        /// <summary>[3D] Generates UV0 coordinates using the AutoUV algorithm of SabreCSG.</summary>
        /// <param name="offset">The offset to be added to the UV coordinates.</param>
        public void ApplySabreCSGAutoUV0(Vector2 offset)
        {
            int count = Count;
            RecalculatePlane();

            var cancellingRotation = Quaternion.Inverse(Quaternion.LookRotation(plane.normal));
            // sets the uv at each point to the position on the plane.
            for (int i = 0; i < count; i++)
            {
                Vector2 uv = (cancellingRotation * (new Vector3(offset.x, offset.y) + this[i].position));
                this[i] = new Vertex(this[i].position, uv);
            }
        }

        /// <summary>
        /// [3D] Extrudes this polygon by the specified distance along its normal and returns the
        /// extruded polygons.
        /// </summary>
        /// <param name="distance">The distance to extrude by.</param>
        /// <returns>The extruded polygons.</returns>
        public List<Polygon> Extrude(float distance)
        {
            int count = Count;
            var results = new List<Polygon>(count);

            // attempted to extrude a 3D polygon with less than 3 vertices.
            if (count < 3)
                return results;

            RecalculatePlane();
            var normal = plane.normal;

            for (int i = 0; i < count - 1; i++)
            {
                if (this[i].hidden) continue;

                results.Add(new Polygon(new Vertex[] {
                    this[i],
                    new Vertex(this[i].position + normal * distance, this[i].uv0),
                    new Vertex(this[i + 1].position + normal * distance, this[i + 1].uv0),
                    this[i + 1],
                }));
            }

            // one more face that wraps around to index 0.
            if (!this[count - 1].hidden)
            {
                results.Add(new Polygon(new Vertex[] {
                    this[count - 1],
                    new Vertex(this[count - 1].position + normal * distance, this[count - 1].uv0),
                    new Vertex(this[0].position + normal * distance, this[0].uv0),
                    this[0],
                }));
            }

            return results;
        }

        /// <summary>
        /// [3D] Extrudes this polygon along a 3 point spline and returns all of the polygons. This
        /// function may crack quads depending on how the spline moves.
        /// </summary>
        /// <param name="spline">The spline to be followed.</param>
        /// <param name="precision">The spline precision.</param>
        public List<Polygon> ExtrudeAlongSpline(MathEx.Spline3 spline, int precision)
        {
            int count = Count;
            var results = new List<Polygon>(count * precision);

            // attempted to extrude a 3D polygon with less than 3 vertices.
            if (count < 3)
                return results;

            // keep track of the last polygon target.
            var lastPoly = new Polygon(this);
            {
                var there = 0f;
                var tnext = 1f / precision;
                var avgforward = (spline.GetForward(there) + spline.GetForward(tnext)).normalized;

                // position and rotate it to the desired target.
                lastPoly.Rotate(Quaternion.LookRotation(avgforward, -spline.GetUp(there)));
                lastPoly.Translate(spline.GetPoint(there));
            }

            // add the front face.
            results.Add(lastPoly);

            for (int p = 1; p < precision + 1; p++)
            {
                // clone the initial polygon.
                var poly = new Polygon(this);
                var there = p / (float)precision;
                var tnext = (p + 1) / (float)precision;
                var avgforward = (spline.GetForward(there) + spline.GetForward(tnext)).normalized;

                // position and rotate it to the desired target.
                poly.Rotate(Quaternion.LookRotation(avgforward, -spline.GetUp(there)));
                poly.Translate(spline.GetPoint(there));

                // fill the gap with quads "extruding" the shape.
                for (int i = 0; i < count - 1; i++)
                {
                    if (poly[i].hidden) continue;

                    results.Add(new Polygon(new Vertex[] {
                        lastPoly[i],
                        poly[i],
                        poly[i + 1],
                        lastPoly[i + 1],
                    }));
                }

                // one more face that wraps around to index 0.
                if (!poly[count - 1].hidden)
                {
                    results.Add(new Polygon(new Vertex[] {
                        lastPoly[count - 1],
                        poly[count - 1],
                        poly[0],
                        lastPoly[0],
                    }));
                }

                lastPoly = poly;
            }

            // add the back face.
            lastPoly.Reverse();
            results.Add(lastPoly);

            return results;
        }

        /// <summary>
        /// [3D] Extrudes this polygon along a 3 point spline and returns all of the brushes for use
        /// with CSG algorithms. This function may crack quads depending on how the spline moves.
        /// </summary>
        /// <param name="spline">The spline to be followed.</param>
        /// <param name="precision">The spline precision.</param>
        public List<PolygonMesh> ExtrudeBrushesAlongSpline(MathEx.Spline3 spline, int precision)
        {
            int count = Count;
            var results = new List<PolygonMesh>(precision);

            // attempted to extrude a 3D polygon with less than 3 vertices.
            if (count < 3)
                return results;

            // keep track of the last polygon target.
            var lastPoly = new Polygon(this);
            {
                var there = 0f;
                var tnext = 1f / precision;
                var avgforward = (spline.GetForward(there) + spline.GetForward(tnext)).normalized;

                // position and rotate it to the desired target.
                lastPoly.Rotate(Quaternion.LookRotation(avgforward, -spline.GetUp(there)));
                lastPoly.Translate(spline.GetPoint(there));
            }

            for (int p = 1; p < precision + 1; p++)
            {
                var polygons = new List<Polygon>();

                // clone the initial polygon.
                var poly = new Polygon(this);
                var there = p / (float)precision;
                var tnext = (p + 1) / (float)precision;
                var avgforward = (spline.GetForward(there) + spline.GetForward(tnext)).normalized;

                // position and rotate it to the desired target.
                poly.Rotate(Quaternion.LookRotation(avgforward, -spline.GetUp(there)));
                poly.Translate(spline.GetPoint(there));

                // add the front face.
                polygons.Add(lastPoly);

                // fill the gap with quads "extruding" the shape.
                for (int i = 0; i < count - 1; i++)
                {
                    polygons.Add(new Polygon(new Vertex[] {
                        lastPoly[i],
                        poly[i],
                        poly[i + 1],
                        lastPoly[i + 1],
                    }));
                }

                // one more face that wraps around to index 0.
                polygons.Add(new Polygon(new Vertex[] {
                    lastPoly[count - 1],
                    poly[count - 1],
                    poly[0],
                    lastPoly[0],
                }));

                // add the back face.
                var back = new Polygon(poly);
                back.Reverse();
                polygons.Add(back);

                // add the polygon mesh brush.
                var brush = new PolygonMesh(polygons);

                // copy the boolean operator of the 2D polygon into the polygon mesh.
                brush.booleanOperator = booleanOperator;

                results.Add(brush);

                lastPoly = poly;
            }

            return results;
        }

        /// <summary>
        /// [3D] A simple UV algorithm that takes the largest change between vertex local positions,
        /// so XY, XZ or YZ and converts them to U and V coordinates. Since the 2D Shape Editor
        /// works in the metric scale, textures will also cover 1m² in 3D space.
        /// </summary>
        /// <param name="offset">The offset to be added to the UV coordinates.</param>
        public void ApplyPositionBasedUV0(Vector2 offset)
        {
            int count = Count;
            if (count < 1) return;

            var xavg = 0f;
            var yavg = 0f;
            var zavg = 0f;
            for (int i = 1; i < count; i++)
            {
                var vertex1 = this[0];
                var vertex2 = this[i];
                xavg += Mathf.Abs(vertex2.x - vertex1.x);
                yavg += Mathf.Abs(vertex2.y - vertex1.y);
                zavg += Mathf.Abs(vertex2.z - vertex1.z);
            }

            if (xavg > yavg && xavg > zavg && yavg > zavg)
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.x, offset.y + vertex.position.y));
                }
            }
            else if (xavg >= yavg && xavg >= zavg && zavg >= yavg)
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.x, offset.y + vertex.position.z));
                }
            }
            else if (yavg > xavg && yavg > zavg && xavg > zavg)
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.x, offset.y + vertex.position.y));
                }
            }
            else if (yavg >= xavg && yavg >= zavg && zavg >= xavg)
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.z, offset.y + vertex.position.y));
                }
            }
            else if (zavg > xavg && zavg > yavg && xavg > yavg)
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.x, offset.y + vertex.position.z));
                }
            }
            else if (zavg >= xavg && zavg >= yavg && yavg >= xavg)
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, new Vector2(offset.x + vertex.position.z, offset.y + vertex.position.y));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var vertex = this[i];
                    this[i] = new Vertex(vertex.position, Vector2.zero);
                }
            }
        }

        /// <summary>
        /// Maps all 3D vertices (x, y, z) of the polygon to 2D (x, z). Useful for 2D polygon algorithms.
        /// <para>Returns the matrix used for the 3D to 2D operation for use with <see cref="MapTo3D"/>.</para>
        /// <para>You may have to call <see cref="CalculatePlane"/> first if you modified the polygon.</para>
        /// </summary>
        /// <remarks>
        /// Special thanks to jwatte from http://xboxforums.create.msdn.com/forums/t/16529.aspx for
        /// the algorithm.
        /// </remarks>
        /// <returns>The matrix used for the 3D to 2D operation so it can be used in <see cref="MapTo3D"/>.</returns>
        public Matrix4x4 MapTo2D()
        {
            RecalculatePlane();
            // calculate a 3d to 2d matrix.
            Vector3 right, backward;
            if (Mathf.Abs(plane.normal.x) > Mathf.Abs(plane.normal.z))
                right = Vector3.Cross(plane.normal, new Vector3(0, 0, 1));
            else
                right = Vector3.Cross(plane.normal, new Vector3(1, 0, 0));
            right = Vector3.Normalize(right);
            backward = Vector3.Cross(right, plane.normal);
            Matrix4x4 m = new Matrix4x4(new Vector4(right.x, plane.normal.x, backward.x, 0), new Vector4(right.y, plane.normal.y, backward.y, 0), new Vector4(right.z, plane.normal.z, backward.z, 0), new Vector4(0, 0, 0, 1));
            // multiply all vertices by the matrix.
            var count = Count;
            for (int p = 0; p < count; p++)
                this[p] = new Vertex(m * this[p].position, this[p].uv0);
            return m;
        }

        /// <summary>
        /// Maps all 2D vertices (x, z) of the polygon to 3D (x, y, z). Requires matrix from <see cref="MapTo2D"/>.
        /// </summary>
        /// <param name="matrix">The matrix from the <see cref="MapTo2D"/> operation.</param>
        public void MapTo3D(Matrix4x4 matrix)
        {
            // inverse the 3d to 2d matrix.
            Matrix4x4 m = matrix.inverse;
            // multiply all vertices by the matrix.
            var count = Count;
            for (int p = 0; p < count; p++)
                this[p] = new Vertex(m * this[p].position, this[p].uv0);
        }
    }
}

#endif