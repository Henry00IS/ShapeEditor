#if UNITY_EDITOR

using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// The 2D Shape Editor Window.
    /// </summary>
    public partial class ShapeEditorWindow : EditorWindow
    {
        /// <summary>The currently loaded project.</summary>
        [SerializeField]
        internal Project project = new Project();

        [MenuItem("Window/2D Shape Editor")]
        public static void Init()
        {
            // get existing open window or if none, make a new one:
            ShapeEditorWindow window = GetWindow<ShapeEditorWindow>();
            window.minSize = new float2(800, 600);
            window.Show();
            window.titleContent = new GUIContent("Shape Editor", ShapeEditorResources.Instance.shapeEditorIcon);
            window.minSize = new float2(128, 128);

            // ensure that we are subscribed to undo/redo notifications.
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        public static ShapeEditorWindow InitAndGetHandle()
        {
            Init();
            return GetWindow<ShapeEditorWindow>();
        }

        /// <summary>We use the static constructor to handle C# reloads.</summary>
        static ShapeEditorWindow()
        {
            // re-subscribe to undo/redo.
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnRepaint()
        {
            DrawViewport();
        }

        private void OnMouseDown(int button)
        {
            // possibly forward the event to a window.
            // we can not click on windows while a widget is active.
            activeWindow = FindWindowAtPosition(mousePosition);
            if (activeWindow != null && activeWidget == null)
            {
                activeWindow.OnMouseDown(button);
            }
            else
            {
                // always inform all widgets.
                var widgetsCount = widgets.Count;
                for (int i = 0; i < widgetsCount; i++)
                    widgets[i].OnMouseDown(button);

                // possibly forward the event to a widget.
                activeWidget = FindActiveWidget();
                if (activeWidget != null)
                {
                    // the active widget will receive the click twice.
                    activeWidget.OnMouseDown(button);
                }
                else
                {
                    activeTool.OnMouseDown(button);
                }
            }
            Repaint();
        }

        private void OnMouseUp(int button)
        {
            if (activeWindow != null && activeWidget == null)
            {
                activeWindow.OnMouseUp(button);
            }
            else
            {
                if (activeWidget != null && activeWidget.wantsActive)
                {
                    activeWidget.OnMouseUp(button);
                }
                else
                {
                    activeWidget = null;
                    var widgetsCount = widgets.Count;
                    for (int i = 0; i < widgetsCount; i++)
                        widgets[i].OnMouseUp(button);

                    activeTool.OnMouseUp(button);
                }
            }

            Repaint();
        }

        private void OnGlobalMouseUp(int button)
        {
            if (activeWindow != null && activeWidget == null)
            {
                activeWindow.OnGlobalMouseUp(button);
            }
            else
            {
                if (activeWidget != null && activeWidget.wantsActive)
                {
                    activeWidget.OnGlobalMouseUp(button);
                }
                else
                {
                    activeWidget = null;
                    var widgetsCount = widgets.Count;
                    for (int i = 0; i < widgetsCount; i++)
                        widgets[i].OnGlobalMouseUp(button);

                    activeTool.OnGlobalMouseUp(button);
                }
            }

            Repaint();
        }

        private void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            if (activeWindow != null && activeWidget == null)
            {
                activeWindow.OnMouseDrag(button, screenDelta);
            }
            else
            {
                if (activeWidget != null && activeWidget.wantsActive)
                {
                    activeWidget.OnMouseDrag(button, screenDelta, gridDelta);
                }
                else
                {
                    activeWidget = null;
                    var widgetsCount = widgets.Count;
                    for (int i = 0; i < widgetsCount; i++)
                        widgets[i].OnMouseDrag(button, screenDelta, gridDelta);

                    activeTool.OnMouseDrag(button, screenDelta, gridDelta);
                }

                // pan the viewport around with the right mouse button.
                if (isRightMousePressed)
                {
                    gridOffset += screenDelta;
                }
            }

            Repaint();
        }

        private void OnMouseMove(float2 screenDelta, float2 gridDelta)
        {
            // forward this event to the topmost window under the mouse position.
            var window = FindWindowAtPosition(mousePosition);
            if (window != null && activeWidget == null)
            {
                window.OnMouseMove(screenDelta);
            }
            else
            {
                if (activeWidget != null && activeWidget.wantsActive)
                {
                    activeWidget.OnMouseMove(screenDelta, gridDelta);
                }
                else
                {
                    activeWidget = null;
                    var widgetsCount = widgets.Count;
                    for (int i = 0; i < widgetsCount; i++)
                        widgets[i].OnMouseMove(screenDelta, gridDelta);

                    activeTool.OnMouseMove(screenDelta, gridDelta);
                }
            }

            Repaint();
        }

        private void OnMouseScroll(float delta)
        {
            var mouseBeforeZoom = ScreenPointToGrid(mousePosition);

            gridZoom *= math.pow(2, -delta / 24.0f); // what about math.exp(-delta / 24.0f); ?

            // recalculate the grid offset to zoom into whatever is under the mouse cursor.
            var mouseAfterZoom = ScreenPointToGrid(mousePosition);
            var mouseDifference = mouseAfterZoom - mouseBeforeZoom;
            gridOffset = GridPointToScreen(mouseDifference);

            Repaint();
        }

        private bool OnKeyDown(KeyCode keyCode)
        {
            // possibly forward the event to a window.
            if (activeWindow != null && activeWidget == null)
            {
                return activeWindow.OnKeyDown(keyCode);
            }
            else
            {
                // possibly forward the event to a widget.
                if (activeWidget != null)
                {
                    return activeWidget.OnKeyDown(keyCode);
                }
                else
                {
                    // possibly forward the event to the tool.
                    if (activeTool.OnKeyDown(keyCode))
                    {
                        return true;
                    }
                    else
                    {
                        switch (keyCode)
                        {
                            case KeyCode.H:
                                GridResetOffset();
                                GridResetZoom();
                                return true;

                            case KeyCode.Q:
                                SwitchToBoxSelectTool();
                                return true;

                            case KeyCode.W:
                                SwitchToTranslateTool();
                                return true;

                            case KeyCode.E:
                                SwitchToRotateTool();
                                return true;

                            case KeyCode.R:
                                SwitchToScaleTool();
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool OnKeyUp(KeyCode keyCode)
        {
            // possibly forward the event to a window.
            if (activeWindow != null && activeWidget == null)
            {
                return activeWindow.OnKeyUp(keyCode);
            }
            else
            {
                // possibly forward the event to a widget.
                if (activeWidget != null)
                {
                    return activeWidget.OnKeyUp(keyCode);
                }
                else
                {
                    // possibly forward the event to the tool.
                    if (activeTool.OnKeyUp(keyCode))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnTopToolbarGUI()
        {
            if (GUILayout.Button(new GUIContent(ShapeEditorResources.Instance.shapeEditorNew, "New Project (N)"), ShapeEditorResources.toolbarButtonStyle))
            {
                Undo.RecordObject(this, "New Project");
                project = new Project();
                Repaint();
            }

            GUILayout.FlexibleSpace();
        }

        private void OnBottomToolbarGUI()
        {
            GUILayout.Label("2D Shape Editor");

            GUILayout.FlexibleSpace();

            GUILayout.Label("Snap");
            gridSnap = EditorGUILayout.FloatField(gridSnap, GUILayout.Width(64f));

            GUILayout.Label("Zoom");
            gridZoom = EditorGUILayout.FloatField(gridZoom, GUILayout.Width(64f));
        }
    }
}

#endif