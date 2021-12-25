#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Used with JSON to load an unknown project file.</summary>
    [Serializable]
    public class ProjectUnknown
    {
        /// <summary>The project version.</summary>
        [SerializeField]
        public int version = 0;
    }
}

#endif