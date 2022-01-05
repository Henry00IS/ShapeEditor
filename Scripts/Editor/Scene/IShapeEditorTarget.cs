#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Represents a mesh target that, when selected, can receive meshes created by the 2D Shape Editor.
    /// </summary>
    public interface IShapeEditorTarget
    {
        /// <summary>Called by the shape editor when a target gets assigned.</summary>
        /// <param name="project">The shape editor project to be used.</param>
        public void OnShapeEditorUpdateProject(Project project);

        /// <summary>Rebuilds the mesh with the current configuration.</summary>
        public void Rebuild();
    }
}

#endif