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
            RequireConvexPolygons2D();

            var parent = CleanAndGetBrushParent();

            var polygonsCount = convexPolygons2D.Count;
            for (int i = 0; i < polygonsCount; i++)
            {
                var polygon = convexPolygons2D[i];
                ExternalRealtimeCSG.CreateExtrudedBrushesFromPolygon(parent, "Shape Editor Brush", polygon.GetVertices2D(), fixedExtrudeDistance, polygon.booleanOperator);
            }

            ExternalRealtimeCSG.AddCSGOperationComponent(gameObject);
            ExternalRealtimeCSG.UpdateSelection();
        }
    }
}

#endif