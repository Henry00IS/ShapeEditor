#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Displays a tooltip with text at the current mouse position.</summary>
    public class TooltipWindow : EditorWindow
    {
        internal static readonly Color windowBorderColor = new Color(0.102f, 0.102f, 0.102f);
        internal static readonly Color windowBackgroundColor = new Color(0.247f, 0.247f, 0.247f);

        /// <summary>
        /// Whether an instance of this tooltip window is probably active. Used for lag reduction as
        /// <see cref="Resources.FindObjectsOfTypeAll"/> is slow.
        /// </summary>
        private static bool isWindowProbablyActive;

        /// <summary>The shape editor window.</summary>
        public ShapeEditorWindow editor { get; private set; }

        /// <summary>The tooltip text to be shown.</summary>
        private string tooltipText;

        /// <summary>Gets an existing open tooltip window or if none, makes a new one and opens it.</summary>
        public static TooltipWindow ShowTooltip(ShapeEditorWindow editor, string tooltipText)
        {
            isWindowProbablyActive = true;

            // calculate the required size to the fit the word wrapped tooltip text.
            var size = EditorStyles.wordWrappedLabel.CalcSize(new GUIContent(tooltipText));
            size.x = size.x < 303 - 8 ? size.x + 8 : 303;
            size.y = EditorStyles.wordWrappedLabel.CalcHeight(new GUIContent(tooltipText), size.x - 8) + 4;

            // calculate a position based on the size and mouse cursor.
            var pos = Extensions.GetCurrentMousePosition() + new Vector2(-Mathf.FloorToInt(size.x / 2), 20);

            // find and update a tooltip window that already exists.
            var windows = Resources.FindObjectsOfTypeAll<TooltipWindow>();
            if (windows.Length > 0)
            {
                windows[0].tooltipText = tooltipText;
                windows[0].position = new Rect(pos, size);
                return windows[0];
            }

            // create and return a new window.
            var popup = CreateInstance<TooltipWindow>();
            popup.hideFlags = HideFlags.DontSave;
            popup.editor = editor;
            popup.tooltipText = tooltipText;
            popup.minSize = new Vector2(1, 1);
            popup.position = new Rect(pos, size);
            popup.ShowTooltip();
            return popup;
        }

        public static void CloseTooltips()
        {
            if (!isWindowProbablyActive) return;
            isWindowProbablyActive = false;

            // find and close all tooltip windows.
            var windows = Resources.FindObjectsOfTypeAll<TooltipWindow>();
            for (int i = 0; i < windows.Length; i++)
                windows[i].Close();
        }

        /// <summary>Called by the Unity Editor to process events.</summary>
        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(Vector2.zero, position.size), windowBorderColor);
            EditorGUI.DrawRect(new Rect(Vector2.one, position.size - Vector2.one * 2), windowBackgroundColor);

            GUILayout.Label(tooltipText, EditorStyles.wordWrappedLabel);
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