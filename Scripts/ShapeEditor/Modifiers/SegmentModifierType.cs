#if UNITY_EDITOR

namespace AeternumGames.ShapeEditor
{
    /// <summary>The segment modifier type.</summary>
    public enum SegmentModifierType
    {
        /// <summary>The default type when no modifier is active.</summary>
        Nothing,
        /// <summary>Generates a bezier curve between two segments.</summary>
        Bezier,
    }
}

#endif