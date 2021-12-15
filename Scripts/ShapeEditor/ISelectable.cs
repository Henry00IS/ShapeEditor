#if UNITY_EDITOR

using Unity.Mathematics;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Any object that can be selected in the 2D Shape Editor.</summary>
    public interface ISelectable
    {
        /// <summary>The position of the object on the grid.</summary>
        float2 position { get; set; }
    }
}

#endif