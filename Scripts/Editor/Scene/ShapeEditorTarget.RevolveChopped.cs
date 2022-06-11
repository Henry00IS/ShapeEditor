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
        [Min(MathEx.EPSILON_3)]
        internal float revolveChoppedDistance = 0.25f;

        private void RevolveChopped_Rebuild()
        {
            RequireChoppedPolygons2D(revolveChoppedPrecision);

            // clamp the degrees to be at least between -0.1f and 0.1f but never 0.0f.
            if (revolveChoppedDegrees >= 0.0f && revolveChoppedDegrees < 0.1f)
                revolveChoppedDegrees = 0.1f;
            else if (revolveChoppedDegrees < 0.0f && revolveChoppedDegrees > -0.1f)
                revolveChoppedDegrees = -0.1f;

            var mesh = MeshGenerator.CreateRevolveChoppedMesh(choppedPolygons2D, revolveChoppedDegrees, revolveChoppedDistance);
            OnShapeEditorMesh(mesh);
        }
    }
}

#endif