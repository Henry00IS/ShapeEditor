#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// A vertex (plural vertices) describes the position of a point in 3D space. With additional
    /// attributes for texturing.
    /// </summary>
    public struct Vertex
    {
        /// <summary>The position of the vertex.</summary>
        public Vector3 position;

        /// <summary>The UV channel 0 coordinates of the vertex.</summary>
        public Vector2 uv0;

        /// <summary>
        /// Whether this and the next vertex are part of a hidden edge that should not be extruded.
        /// This is used for the hidden surface removal algorithm, preventing interior 3D polygons.
        /// </summary>
        public bool hidden;

        /// <summary>Gets the x-coordinate of the vertex position.</summary>
        public float x => position.x;

        /// <summary>Gets the y-coordinate of the vertex position.</summary>
        public float y => position.y;

        /// <summary>Gets the z-coordinate of the vertex position.</summary>
        public float z => position.z;

        /// <summary>Gets the UV channel 0 u-coordinate of the vertex.</summary>
        public float u => uv0.x;

        /// <summary>Gets the UV channel 0 v-coordinate of the vertex.</summary>
        public float v => uv0.y;

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="position">The vertex position.</param>
        /// <param name="uv0">The UV channel 0 coordinates of the vertex.</param>
        public Vertex(Vector3 position, Vector2 uv0)
        {
            this.position = position;
            this.uv0 = uv0;
            hidden = false;
        }

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="position">The vertex position.</param>
        /// <param name="uv0">The UV channel 0 coordinates of the vertex.</param>
        /// <param name="hidden">Whether this and the next vertex are part of a hidden edge.</param>
        public Vertex(Vector3 position, Vector2 uv0, bool hidden)
        {
            this.position = position;
            this.uv0 = uv0;
            this.hidden = hidden;
        }

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="position">The vertex position.</param>
        public Vertex(Vector3 position) : this(position, Vector2.zero) { }

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="x">The x-coordinate of the vertex position.</param>
        /// <param name="y">The y-coordinate of the vertex position.</param>
        public Vertex(float x, float y) : this(new Vector3(x, y), Vector2.zero) { }

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="x">The x-coordinate of the vertex position.</param>
        /// <param name="y">The y-coordinate of the vertex position.</param>
        /// <param name="z">The z-coordinate of the vertex position.</param>
        public Vertex(float x, float y, float z) : this(new Vector3(x, y, z), Vector2.zero) { }

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="x">The x-coordinate of the vertex position.</param>
        /// <param name="y">The y-coordinate of the vertex position.</param>
        /// <param name="z">The z-coordinate of the vertex position.</param>
        /// <param name="uv0">The UV channel 0 coordinates of the vertex.</param>
        public Vertex(float x, float y, float z, Vector2 uv0) : this(new Vector3(x, y, z), uv0) { }

        /// <summary>Creates a new vertex located at the specified position.</summary>
        /// <param name="x">The x-coordinate of the vertex position.</param>
        /// <param name="y">The y-coordinate of the vertex position.</param>
        /// <param name="z">The z-coordinate of the vertex position.</param>
        /// <param name="u">The UV channel 0 u-coordinate of the vertex.</param>
        /// <param name="v">The UV channel 0 v-coordinate of the vertex.</param>
        public Vertex(float x, float y, float z, float u, float v) : this(new Vector3(x, y, z), new Vector2(u, v)) { }

        /// <summary>Creates a copy of the specified vertex.</summary>
        /// <param name="vertex">The vertex to be copied.</param>
        public Vertex(Vertex vertex) : this(vertex.position, vertex.uv0) { }
    }
}

#endif