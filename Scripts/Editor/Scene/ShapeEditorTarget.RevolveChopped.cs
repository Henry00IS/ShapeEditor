#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorTarget
    {
        [SerializeField]
        [Min(1)]
        internal int revolveChoppedPrecision = 8;

        [SerializeField]
        [Range(-360f, 360.0f)]
        internal float revolveChoppedDegrees = 90f;

        [SerializeField]
        [Min(0f)]
        internal float revolveChoppedRadius = 2f;

        [SerializeField]
        [Min(MathEx.EPSILON_3)]
        internal float revolveChoppedDistance = 0.25f;

        private void RevolveChopped_Rebuild()
        {
            RequireChoppedPolygons2D(revolveChoppedPrecision);

            var mesh = MeshGenerator.CreateRevolveChoppedMesh(choppedPolygons2D, revolveChoppedDegrees, revolveChoppedRadius, revolveChoppedDistance);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif