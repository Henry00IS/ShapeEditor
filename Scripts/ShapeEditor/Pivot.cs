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

        /// <summary>Whether the pivot is selected.</summary>
        [NonSerialized]
        private bool _selected;

        /// <summary>Gets or sets the position of the pivot on the grid.</summary>
        public float2 position { get => _position; set => _position = value; }

        /// <summary>Gets or sets whether the pivot is selected.</summary>
        public bool selected { get => _selected; set => _selected = value; }
    }
}

#endif