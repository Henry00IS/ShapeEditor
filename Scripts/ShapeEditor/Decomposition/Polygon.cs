#if UNITY_EDITOR

// contains source code from https://github.com/Genbox/VelcroPhysics (see Licenses/VelcroPhysics.txt).

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A collection of vertex points that make up a 2D/3D polygon.</summary>
    public partial class Polygon : List<Vertex>
    {
        /// <summary>[2D/3D] Creates a new empty polygon.</summary>
        public Polygon()
        {
        }

        /// <summary>[2D/3D] Creates a new empty polygon with the specified initial capacity.</summary>
        /// <param name="capacity">The number of vertices the polygon can hold without resizing.</param>
        public Polygon(int capacity) : base(capacity)
        {
        }

        /// <summary>[2D/3D] Creates a copy of the specified polygon.</summary>
        /// <param name="original">The original polygon to copy.</param>
        public Polygon(Polygon original) : base(original)
        {
        }

        /// <summary>[2D/3D] Creates a new polygon with the specified initial vertices.</summary>
        /// <param name="vertices">The initial vertices to add to the polygon.</param>
        public Polygon(IEnumerable<Vertex> vertices) : base(vertices) { }

        /// <summary>[2D/3D] Gets or sets the collection of holes that will be used during triangulation.</summary>
        public List<Polygon> Holes { get; set; }

        /// <summary>
        /// [2D/3D] The boolean operator of the polygon used by CSG targets <see
        /// cref="Project.GenerateConvexPolygons"/>. The use of holes with convex decomposition
        /// leads to many brushes, which can be avoided by using the subtractive brushes of the CSG algorithm.
        /// </summary>
        [SerializeField]
        public PolygonBooleanOperator booleanOperator = PolygonBooleanOperator.Union;

        /// <summary>[2D/3D] Gets the next index. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public int NextIndex(int index)
        {
            return index + 1 > Count - 1 ? 0 : index + 1;
        }

        /// <summary>[2D/3D] Gets the next vertex. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public Vertex NextVertex(int index)
        {
            return this[NextIndex(index)];
        }

        /// <summary>[2D/3D] Gets the previous index. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public int PreviousIndex(int index)
        {
            return index - 1 < 0 ? Count - 1 : index - 1;
        }

        /// <summary>[2D/3D] Gets the previous vertex. Used for iterating all the edges with wrap-around.</summary>
        /// <param name="index">The current index</param>
        public Vertex PreviousVertex(int index)
        {
            return this[PreviousIndex(index)];
        }

        /// <summary>[2D/3D] Translates the vertices with the specified vector.</summary>
        /// <param name="value">The vector.</param>
        public void Translate(Vector3 value)
        {
            Translate(ref value);
        }

        /// <summary>[2D/3D] Translates the vertices with the specified vector.</summary>
        /// <param name="value">The vector.</param>
        public void Translate(ref Vector3 value)
        {
            int count = Count;

            for (int i = 0; i < count; i++)
                this[i] = new Vertex(this[i].position + value, this[i].uv0, this[i].hidden);
        }

        /// <summary>[2D/3D] Scales the vertices with the specified vector.</summary>
        /// <param name="value">The vector.</param>
        public void Scale(Vector3 value)
        {
            Scale(ref value);
        }

        /// <summary>[2D/3D] Scales the vertices with the specified vector.</summary>
        /// <param name="value">The vector.</param>
        public void Scale(ref Vector3 value)
        {
            int count = Count;

            for (int i = 0; i < count; i++)
                this[i] = new Vertex(new Vector3(this[i].position.x * value.x, this[i].position.y * value.y, this[i].position.z * value.z), this[i].uv0, this[i].hidden);
        }

        /// <summary>[2D/3D] Converts the vertex positions to an array of vectors.</summary>
        /// <returns>The array of vectors.</returns>
        public Vector3[] GetVertices()
        {
            int count = Count;
            var vertices = new Vector3[count];
            for (int i = 0; i < count; i++)
                vertices[i] = this[i].position;
            return vertices;
        }

        /// <summary>[2D/3D] Converts the vertex uv0 coordinates to an array of vectors.</summary>
        /// <returns>The array of vectors.</returns>
        public Vector2[] GetUV0()
        {
            int count = Count;
            var uv0 = new Vector2[count];
            for (int i = 0; i < count; i++)
                uv0[i] = this[i].uv0;
            return uv0;
        }

        /// <summary>[2D/3D] Returns the triangles offset by the specified amount.</summary>
        /// <returns>The array of triangles.</returns>
        public int[] GetTriangles(int offset = 0)
        {
            List<int> triangles = new List<int>();
            var count = Count;

            var first = 0;
            int next = 1;
            for (int i = 2; i < count; i++)
            {
                triangles.Add(offset + next);
                triangles.Add(offset + first);
                triangles.Add(offset + first + i);
                next = first + i;
            }

            // todo: replace this with an array, we can know the amount of entries, right? math? anyone?
            return triangles.ToArray();
        }
    }
}

#endif