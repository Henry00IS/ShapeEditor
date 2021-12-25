#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds an extruded polygon with fixed distance.
        [Min(MathEx.EPSILON_3)]
        public float fixedExtrudeDistance = 0.25f;

        private void FixedExtrude_Rebuild()
        {
            var mesh = MeshGenerator.CreateExtrudedPolygonMesh(convexPolygons2D, fixedExtrudeDistance);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif