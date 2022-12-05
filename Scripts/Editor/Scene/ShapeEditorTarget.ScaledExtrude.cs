#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // extrudes the shape to a point or clipped/scaled.

        [Min(MathEx.EPSILON_3)]
        public float scaledExtrudeDistance = 1.0f;

        [Min(0f)]
        public Vector2 scaledExtrudeFrontScale = new Vector2(1.0f, 1.0f);

        [Min(0f)]
        public Vector2 scaledExtrudeBackScale = new Vector2(0.0f, 0.0f);

        public Vector2 scaledExtrudeOffset;

        private void ScaledExtrude_Rebuild()
        {
            RequireConvexPolygons2D();

            var mesh = MeshGenerator.CreateScaleExtrudedMesh(convexPolygons2D, scaledExtrudeDistance, scaledExtrudeFrontScale, scaledExtrudeBackScale, scaledExtrudeOffset);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif