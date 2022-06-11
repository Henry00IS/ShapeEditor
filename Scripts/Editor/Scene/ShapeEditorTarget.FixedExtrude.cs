#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds an extruded polygon with fixed distance.
        [SerializeField]
        [Min(MathEx.EPSILON_3)]
        internal float fixedExtrudeDistance = 0.25f;

        private void FixedExtrude_Rebuild()
        {
            RequireConvexPolygons2D();

            var mesh = MeshGenerator.CreateExtrudedPolygonMesh(convexPolygons2D, fixedExtrudeDistance);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif