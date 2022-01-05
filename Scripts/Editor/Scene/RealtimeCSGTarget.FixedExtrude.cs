#if UNITY_EDITOR

using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class RealtimeCSGTarget
    {
        // builds an extruded polygon with fixed distance.
        [SerializeField]
        [Min(MathEx.EPSILON_3)]
        internal float fixedExtrudeDistance = 0.25f;

        private void FixedExtrude_Rebuild()
        {
            var meshes = MeshGenerator.CreateExtrudedPolygonMeshes(convexPolygons2D, fixedExtrudeDistance);

            var parent = CleanAndGetBrushParent();

            var meshesCount = meshes.Count;
            for (int i = 0; i < meshesCount; i++)
            {
                var mesh = meshes[i];
                var brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Shape Editor Brush", mesh.ToPlanes());
                if (brush != null)
                    brush.transform.SetParent(parent, false);
            }
        }
    }
}

#endif