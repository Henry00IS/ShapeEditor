#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        /// <summary>Represents an undoable operation used in the undo/redo system.</summary>
        [System.Serializable]
        private class Undoable
        {
            /// <summary>
            /// Undo: The project before the edits mentioned in <see cref="name"/> were done.
            /// <para>Redo: The project with the edits mentioned in <see cref="name"/> applied.</para>
            /// </summary>
            public Project project;

            /// <summary>The name of this undoable operation (e.g. "Translate Selection").</summary>
            public string name;

            /// <summary>Creates a new undoable action by cloning the given project.</summary>
            /// <param name="project">The project that will be cloned.</param>
            /// <param name="name">The name of this undoable operation.</param>
            public Undoable(Project project, string name)
            {
                this.project = project.Clone();
                this.name = name;
            }
        }

        /// <summary>The undo stack contains copies of the project before changes, so we can undo.</summary>
        private List<Undoable> undoStack = new List<Undoable>();

        /// <summary>The redo stack contains copies of the project that were undone.</summary>
        private List<Undoable> redoStack = new List<Undoable>();

        /// <summary>The maximum depth of undo history.</summary>
        private int undoStackDepth = 32;

        /// <summary>Stores a copy of the project on the undo stack.</summary>
        /// <param name="name">The name of the undo operation.</param>
        internal void RegisterUndo(string name)
        {
            // clear all redo operations.
            redoStack.Clear();

            // push a clone of the current project onto the undo stack.
            undoStack.Push(new Undoable(project, name));

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
                // pop an operation from the undo stack.
                var undoable = undoStack.Pop();

                // push a clone of the current project onto the redo stack.
                redoStack.Push(new Undoable(project, undoable.name));

                // use the undo operation project as the current project.
                project = undoable.project;
            }
        }

        /// <summary>Called on CTRL+Y when the editor window has focus.</summary>
        private void OnRedo()
        {
            if (canRedo)
            {
                // pop an operation from the redo stack.
                var undoable = redoStack.Pop();

                // push a clone of the current project onto the undo stack.
                undoStack.Push(new Undoable(project, undoable.name));

                // use the redo operation project as the current project.
                project = undoable.project;
            }
        }

        /// <summary>Gets whether an undo operation is available.</summary>
        internal bool canUndo => undoStack.Count > 0;

        /// <summary>Gets whether a redo operation is available.</summary>
        internal bool canRedo => redoStack.Count > 0;

        /// <summary>Gets the name of an undo operation if available.</summary>
        internal string canUndoName => undoStack.Count > 0 ? undoStack[0].name : "";

        /// <summary>Gets the name of a redo operation if available.</summary>
        internal string canRedoName => redoStack.Count > 0 ? redoStack[0].name : "";
    }
}

#endif