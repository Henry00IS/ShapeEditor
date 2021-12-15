#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private bool isLeftMousePressed;
        private bool isRightMousePressed;

        /// <summary>Called by the Unity Editor to process events.</summary>
        private void OnGUI()
        {
            var e = Event.current;

            if (e.type == EventType.Repaint)
            {
                OnRepaint();
            }

            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0) isLeftMousePressed = true;
                if (e.button == 1) isRightMousePressed = true;
                OnMouseDown(e.button);
            }

            if (e.type == EventType.MouseUp)
            {
                if (e.button == 0) isLeftMousePressed = false;
                if (e.button == 1) isRightMousePressed = false;
                OnMouseUp(e.button);
            }

            if (e.type == EventType.MouseDrag)
            {
                OnMouseDrag(e.button, e.delta);
            }

            if (e.type == EventType.ScrollWheel)
            {
                OnMouseScroll(e.delta.y);
            }

            if (e.type == EventType.KeyDown)
            {
                if (OnKeyDown(e.keyCode))
                    e.Use();
            }

            if (e.type == EventType.KeyUp)
            {
                if (OnKeyUp(e.keyCode))
                    e.Use();
            }

            GUILayout.BeginHorizontal(ShapeEditorResources.toolbarStyle);

            GUIStyle createBrushStyle = new GUIStyle(EditorStyles.toolbarButton);
            if (GUILayout.Button(new GUIContent(ShapeEditorResources.Instance.shapeEditorNew, "New Project (N)"), createBrushStyle))
            {
            }

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        private Rect GetViewportRect()
        {
            return new Rect(0, 0, position.width, position.height);
        }
    }
}

#endif