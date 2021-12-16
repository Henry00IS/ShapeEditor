#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private bool isLeftMousePressed;
        private bool isRightMousePressed;
        private float2 mousePosition;

        /// <summary>Called by the Unity Editor whenever an undo/redo was performed.</summary>
        private static void OnUndoRedoPerformed()
        {
            // unsubscribe if the shape editor window is not open.
            if (!HasOpenInstances<ShapeEditorWindow>())
            {
                Undo.undoRedoPerformed -= OnUndoRedoPerformed;
                return;
            }

            // repaint the window.
            var window = GetWindow<ShapeEditorWindow>();
            window.Repaint();
        }

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
                if (IsMousePositionInViewport(e.mousePosition))
                {
                    // ensure we have input focus.
                    GUI.FocusControl(null);

                    mousePosition = e.mousePosition;
                    if (e.button == 0) isLeftMousePressed = true;
                    if (e.button == 1) isRightMousePressed = true;
                    OnMouseDown(e.button);

                    e.Use();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                if (IsMousePositionInViewport(e.mousePosition))
                {
                    mousePosition = e.mousePosition;
                    if (e.button == 0) isLeftMousePressed = false;
                    if (e.button == 1) isRightMousePressed = false;
                    OnMouseUp(e.button);

                    e.Use();
                }
            }

            if (e.type == EventType.MouseDrag)
            {
                if (IsMousePositionInViewport(e.mousePosition))
                {
                    mousePosition = e.mousePosition;
                    OnMouseDrag(e.button, e.delta);

                    e.Use();
                }
            }

            if (e.type == EventType.ScrollWheel)
            {
                if (IsMousePositionInViewport(e.mousePosition))
                {
                    mousePosition = e.mousePosition;
                    OnMouseScroll(e.delta.y);

                    e.Use();
                }
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

            // top toolbar:
            GUILayout.BeginHorizontal(ShapeEditorResources.toolbarStyle);
            OnTopToolbarGUI();
            GUILayout.EndHorizontal();

            // skip vertical viewport area.
            GUILayout.FlexibleSpace();

            // bottom toolbar:
            GUILayout.BeginHorizontal(ShapeEditorResources.toolbarStyle);
            OnBottomToolbarGUI();
            GUILayout.EndHorizontal();
        }

        private Rect GetViewportRect()
        {
            return new Rect(0, 21, position.width, position.height);
        }

        private bool IsMousePositionInViewport(float2 mousePosition)
        {
            return new Rect(0, 21, position.width, position.height - (21 * 2)).Contains(mousePosition);
        }
    }
}

#endif