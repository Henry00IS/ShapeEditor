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
            var parent = CleanAndGetBrushParent();

            var polygonsCount = convexPolygons2D.Count;
            for (int i = 0; i < polygonsCount; i++)
            {
                var polygon = convexPolygons2D[i];
                var brushes = ExternalRealtimeCSG.CreateExtrudedBrushesFromPolygon("Shape Editor Brush", polygon.GetVertices2D(), fixedExtrudeDistance);
                if (brushes == null) continue;

                var brushesCount = brushes.Count;
                for (int j = 0; j < brushesCount; j++)
                    brushes[j].transform.SetParent(parent, false);
            }

            /*
            var meshes = MeshGenerator.CreateExtrudedPolygonMeshes(convexPolygons2D, fixedExtrudeDistance);
            var meshesCount = meshes.Count;
            for (int i = 0; i < meshesCount; i++)
            {
                var mesh = meshes[i];
                var brush = ExternalRealtimeCSG.CreateBrushFromPlanes("Shape Editor Brush", mesh.ToPlanes());
                if (brush != null)
                    brush.transform.SetParent(parent, false);
            }*/
        }
    }
}

#endif