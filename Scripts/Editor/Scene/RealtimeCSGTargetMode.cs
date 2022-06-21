#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    /// <summary>The operating mode of the target object.</summary>
    public enum RealtimeCSGTargetMode
    {
        FixedExtrude = 0,
        ScaledExtrude = 4,
        [UnityEngine.InspectorName("Spline Extrude (Experimental!)")]
        SplineExtrude = 1,
        RevolveChopped = 5,
        RevolveExtrude = 2,
        LinearStaircase = 3,
    }
}

#endif