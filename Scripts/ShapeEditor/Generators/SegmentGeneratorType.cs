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
        /// <summary>Generates a sine wave between two segments.</summary>
        Sine,
        /// <summary>Repeats x amount of previous segments.</summary>
        Repeat,
        /// <summary>Generates a well-known architectural arch between two segments.</summary>
        Arch,
    }
}

#endif