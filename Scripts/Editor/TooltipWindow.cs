#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public class TooltipWindow : EditorWindow
    {
        /// <summary>The shape editor window.</summary>
        public ShapeEditorWindow editor { get; private set; }

        /// <summary>Must be called upon creation of the tooltip window.</summary>
        /// <param name="parent"></param>
        public void Initialize(ShapeEditorWindow editor)
        {
            this.editor = editor;
        }

        /// <summary>Called by the Unity Editor to process events.</summary>
        private void OnGUI()
        {
            // continue to request mouse move events, as this flag can get reset upon c# reloads.
            wantsMouseMove = true;

            var e = Event.current;

            if (e.type == EventType.MouseMove)
            {
                Close();
            }
        }

        /// <summary>Called by Unity Editor when the editor window is closed.</summary>
        private void OnDestroy()
        {
            // if we got focus then give the focus back to the shape editor.
            if (hasFocus && editor)
                editor.Focus();
        }
    }
}

#endif