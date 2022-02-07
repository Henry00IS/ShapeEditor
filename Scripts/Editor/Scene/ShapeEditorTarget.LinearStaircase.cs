#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        // builds a linear staircase.

        [SerializeField]
        [Min(1)]
        internal int linearStaircasePrecision = 8;

        [SerializeField]
        [Min(MathEx.EPSILON_3)]
        internal float linearStaircaseDistance = 1f;

        [SerializeField]
        internal float linearStaircaseHeight = 0.75f;

        [SerializeField]
        internal bool linearStaircaseSloped = false;

        private void LinearStaircase_Rebuild()
        {
            var mesh = MeshGenerator.CreateLinearStaircaseMesh(convexPolygons2D, linearStaircasePrecision, linearStaircaseDistance, linearStaircaseHeight, linearStaircaseSloped);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif