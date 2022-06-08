#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The clipboard contents used to copy data to other shape editors.</summary>
    [Serializable]
    public class ClipboardData
    {
        /// <summary>The shapes in the clipboard.</summary>
        [SerializeField]
        public List<Shape> shapes = new List<Shape>();
    }
}

#endif