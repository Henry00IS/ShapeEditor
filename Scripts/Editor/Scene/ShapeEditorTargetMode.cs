#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    /// <summary>The operating mode of the target object.</summary>
    public enum ShapeEditorTargetMode
    {
        Polygon = 0,
        FixedExtrude = 1,
        ScaledExtrude = 5,
        SplineExtrude = 2,
        RevolveExtrude = 3,
        LinearStaircase = 4,
    }
}

#endif