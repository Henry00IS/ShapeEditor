#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    public struct VertexMaterial
    {
        /// <summary>
        /// The material index for extruded edges but at the end of the mesh creation pipeline the
        /// material index to be used when building this polygon.
        /// </summary>
        public byte extrude;

        /// <summary>The material index to be used for the front of an extruded shape.</summary>
        public byte front;

        /// <summary>The material index to be used for the back of an extruded shape.</summary>
        public byte back;

        public VertexMaterial(byte extrude, byte front, byte back)
        {
            this.extrude = extrude;
            this.front = front;
            this.back = back;
        }
    }
}

#endif