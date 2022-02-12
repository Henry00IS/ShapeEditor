#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    /// <summary>The operating mode of the target object.</summary>
    public enum ChiselTargetMode
    {
        FixedExtrude = 0,
        ScaledExtrude = 3,
        RevolveExtrude = 1,
        LinearStaircase = 2,
    }
}

#endif