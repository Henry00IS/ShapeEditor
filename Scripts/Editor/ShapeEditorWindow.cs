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
        private static ShapeEditorWindow s_Instance;

        /// <summary>Gets the singleton shape editor window instance or creates it.</summary>
        public static ShapeEditorWindow Instance
        {
            get
            {
                // if known, immediately return the instance.
                if (s_Instance) return s_Instance;

                // load the shape editor window.
                Init();

                return s_Instance;
            }
        }

        /// <summary>The currently loaded project.</summary>
        [SerializeField]
        internal Project project = new Project();

        /// <summary>Whether the mouse is actively in use by a widget or pressed.</summary>
        internal bool isMouseBusy => isLeftMousePressed || isRightMousePressed || isToolBusy;

        [MenuItem("Window/2D Shape Editor")]
        public static void Init()
        {
            // get existing open window or if none, make a new one:
            ShapeEditorWindow window = GetWindow<ShapeEditorWindow>();
            s_Instance = window;
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
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnMouseDown(button);
            }
            else
            {
                // on click we find a window under the mouse cursor.
                var window = FindWindowAtPosition(mousePosition);
                if (window != null)
                {
                    // try to focus the window.
                    if (TrySwitchActiveEventReceiver(window))
                    {
                        eventReceiver = window;
                        MoveWindowToFront(window);
                    }
                }
                else
                {
                    // always inform all widgets so they can calculate input focus.
                    // this means they get the on mouse down event twice.
                    var widgetsCount = widgets.Count;
                    for (int i = 0; i < widgetsCount; i++)
                        widgets[i].OnMouseDown(button);

                    // find a widget that wants focus.
                    var widget = FindActiveWidget();
                    if (widget != null)
                    {
                        // try to focus the widget.
                        if (TrySwitchActiveEventReceiver(widget))
                        {
                            eventReceiver = widget;
                        }
                    }
                    else
                    {
                        // try to focus the active tool.
                        if (TrySwitchActiveEventReceiver(activeTool))
                        {
                            eventReceiver = activeTool;
                        }
                    }
                }

                eventReceiver.OnMouseDown(button);
            }

            Repaint();
        }

        private void OnMouseUp(int button)
        {
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnMouseUp(button);
            }
            else
            {
                // handle widgets that no longer wish to be active.
                if (activeEventReceiverIsWidget)
                {
                    var widget = GetActiveEventReceiver<Widget>();
                    if (!widget.wantsActive)
                    {
                        eventReceiver.OnMouseUp(button);

                        // try to focus the active tool.
                        if (TrySwitchActiveEventReceiver(activeTool))
                        {
                            eventReceiver = activeTool;
                        }
                    }
                    else
                    {
                        eventReceiver.OnMouseUp(button);
                    }
                }
                else
                {
                    eventReceiver.OnMouseUp(button);
                }
            }

            Repaint();
        }

        private void OnGlobalMouseUp(int button)
        {
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnGlobalMouseUp(button);
            }
            else
            {
                // handle widgets that no longer wish to be active.
                if (activeEventReceiverIsWidget)
                {
                    var widget = GetActiveEventReceiver<Widget>();
                    if (!widget.wantsActive)
                    {
                        eventReceiver.OnGlobalMouseUp(button);

                        // try to focus the active tool.
                        if (TrySwitchActiveEventReceiver(activeTool))
                        {
                            eventReceiver = activeTool;
                        }
                    }
                    else
                    {
                        eventReceiver.OnGlobalMouseUp(button);
                    }
                }
                else
                {
                    eventReceiver.OnGlobalMouseUp(button);
                }
            }

            Repaint();
        }

        private void OnMouseDrag(int button, float2 screenDelta, float2 gridDelta)
        {
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnMouseDrag(button, screenDelta, gridDelta);
            }
            else
            {
                eventReceiver.OnMouseDrag(button, screenDelta, gridDelta);
            }

            // maybe check for a return value and disable this behavior.
            if (activeEventReceiverIsTool || activeEventReceiverIsWidget)
            {
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
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnMouseMove(screenDelta, gridDelta);
            }
            else
            {
                eventReceiver.OnMouseMove(screenDelta, gridDelta);
            }

            Repaint();
        }

        private void OnMouseScroll(float delta)
        {
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                // possibly forward the event to a window.
                if (eventReceiver.OnMouseScroll(delta)) { Repaint(); return; }
            }
            else
            {
                // possibly forward the event to a window.
                if (eventReceiver.OnMouseScroll(delta)) { Repaint(); return; }
            }

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
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnKeyDown(keyCode);
            }
            else
            {
                var used = eventReceiver.OnKeyDown(keyCode);

                // in tool mode we provide default keyboard shortcuts.
                if (activeEventReceiverIsTool && !used)
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
                            return true;

                        case KeyCode.Z:
                            if (hasFocus && isCtrlPressed)
                            {
                                OnUndo();
                                return true;
                            }
                            return true;
                    }
                }
            }

            // focus check, was this required? do this in the editor code?
            if (hasFocus)
            {
                Repaint();
                return true;
            }

            return false;
        }

        private bool OnKeyUp(KeyCode keyCode)
        {
            var eventReceiver = GetActiveEventReceiver();

            // when the event receiver is busy it has exclusive rights to this event.
            if (eventReceiver.IsBusy())
            {
                eventReceiver.OnKeyUp(keyCode);
            }
            else
            {
                eventReceiver.OnKeyUp(keyCode);
            }

            // focus check, was this required? do this in the editor code?
            if (hasFocus)
            {
                Repaint();
                return true;
            }

            return false;
        }

        internal void OnAssignProjectToTargets()
        {
            var transform = Selection.activeTransform;
            if (transform)
            {
                var target = transform.GetComponent<ShapeEditorTarget>();
                if (target)
                {
                    target.OnShapeEditorUpdateProject(project);
                }
            }
        }
    }
}

#endif