#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        /// <summary>The undo stack contains copies of the project before changes, so we can undo.</summary>
        private List<Project> undoStack = new List<Project>();

        /// <summary>The redo stack contains copies of the project that were undone.</summary>
        private List<Project> redoStack = new List<Project>();

        /// <summary>The maximum depth of undo history.</summary>
        private int undoStackDepth = 32;

        /// <summary>Stores a copy of the project on the undo stack.</summary>
        /// <param name="name">The name of the undo operation.</param>
        internal void RegisterUndo(string name)
        {
            redoStack.Clear();
            undoStack.Push(project.Clone());

            // remove history that exceeds the maximum depth.
            while (undoStack.Count > undoStackDepth)
                undoStack.RemoveAt(undoStack.Count - 1);
        }

        /// <summary>Discards a single <see cref="RegisterUndo"/> operation (e.g. for tool cancel).</summary>
        internal void DiscardUndo()
        {
            // prevent future mistakes.
            if (redoStack.Count > 0)
                Debug.LogWarning("Attempted to discard an undo operation that was not on top of the undo/redo stack.");

            // pop a project from the undo stack.
            undoStack.Pop();
        }

        /// <summary>Called on CTRL+Z when the editor window has focus.</summary>
        private void OnUndo()
        {
            if (canUndo)
            {
                // push the current project onto the redo stack.
                redoStack.Push(project.Clone());

                // pop a project from the undo stack.
                project = undoStack.Pop();
            }
        }

        /// <summary>Called on CTRL+Y when the editor window has focus.</summary>
        private void OnRedo()
        {
            if (canRedo)
            {
                // push the current project onto the undo stack.
                undoStack.Push(project.Clone());

                // pop a project from the redo stack.
                project = redoStack.Pop();
            }
        }

        /// <summary>Gets whether an undo operation is available.</summary>
        internal bool canUndo => undoStack.Count > 0;

        /// <summary>Gets whether a redo operation is available.</summary>
        internal bool canRedo => redoStack.Count > 0;
    }
}

#endif