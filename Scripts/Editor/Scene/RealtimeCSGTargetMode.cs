#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    /// <summary>The operating mode of the target object.</summary>
    public enum RealtimeCSGTargetMode
    {
        FixedExtrude,
        [UnityEngine.InspectorName("Spline Extrude (Experimental!)")]
        SplineExtrude,
        RevolveExtrude,
    }
}

#endif