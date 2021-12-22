#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    /// <summary>The segment generator type.</summary>
    public enum SegmentGeneratorType
    {
        /// <summary>The default type creating straight lines.</summary>
        Linear,
        /// <summary>Generates a bezier curve between two segments.</summary>
        Bezier,
    }
}

#endif