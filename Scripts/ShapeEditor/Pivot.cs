#if UNITY_EDITOR

using System;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>A 2D Shape Editor Pivot.</summary>
    [Serializable]
    public class Pivot : ISelectable
    {
        /// <summary>The position of the pivot on the grid.</summary>
        [SerializeField]
        private float2 _position;

        /// <summary>The position of the pivot on the grid.</summary>
        public float2 position
        {
            get { return _position; }
            set { _position = value; }
        }
    }
}

#endif