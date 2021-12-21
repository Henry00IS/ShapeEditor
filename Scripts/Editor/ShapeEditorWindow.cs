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

        /// <summary>Whether the mouse is actively in use by a widget or pressed.</summary>
        internal bool isMouseBusy => isLeftMousePressed || isRightMousePressed || activeWidget != null;

        [MenuItem("Window/2D Shape Editor")]
        public static void Init()
        {
            // get existing open window or if none, make a new one:
            ShapeEditorWindow window = GetWindow<ShapeEditorWindow>();
            window.minSize = new float2(800, 600);
            window.Show();
            window.titleContent = new GUIContent("Shape Editor", ShapeEditorResources.Instance.shapeEditorIcon);
            window.minSize = new float2(128, 128);
        }

        public static ShapeEditorWindow InitAndGetHandle()
        {
            Init();
            return GetWindow<ShapeEditorWindow>();
        }

        private void OnRepaint()
        {
            // check for and delete unused string meshes.
            BmFontCache.OnRenderFrame();

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
            // possibly forward the event to a window.
            if (activeWindow != null && activeWindow.OnMouseScroll(delta)) { Repaint(); return; }

            // possibly forward the event to a widget.
            if (activeWidget != null && activeWidget.OnMouseScroll(delta)) { Repaint(); return; }

            // possibly forward the event to a tool.
            if (activeTool.OnMouseScroll(delta)) { Repaint(); return; }

            // otherwise we provide default behavior: zoom.

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

                            case KeyCode.Delete:
                                DeleteSelection();
                                return true;

                            case KeyCode.Y:
                                if (hasFocus && isCtrlPressed)
                                {
                                    OnRedo();
                                    return true;
                                }
                                return false;

                            case KeyCode.Z:
                                if (hasFocus && isCtrlPressed)
                                {
                                    OnUndo();
                                    return true;
                                }
                                return false;
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

        internal void OnNewProject()
        {
            project = new Project();
            Repaint();
        }
    }
}

#endif