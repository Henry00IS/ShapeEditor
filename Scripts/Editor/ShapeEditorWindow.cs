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
        internal bool isMouseBusy => isLeftMousePressed || isRightMousePressed || isToolBusy;

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
                        case KeyCode.Alpha1:
                            UserSwitchToVertexSelectMode();
                            return true;

                        case KeyCode.Alpha2:
                            UserSwitchToEdgeSelectMode();
                            return true;

                        case KeyCode.Alpha3:
                            UserSwitchToFaceSelectMode();
                            return true;

                        case KeyCode.A:
                            if (isCtrlPressed)
                            {
                                UserSelectAll();
                                return true;
                            }
                            return false;

                        case KeyCode.C:
                            if (isCtrlPressed)
                            {
                                UserCopy();
                                return true;
                            }
                            return false;

                        case KeyCode.Delete:
                            UserDeleteSelection();
                            return true;

                        case KeyCode.D:
                            if (isShiftPressed)
                            {
                                UserClearSelection();
                                return true;
                            }
                            else if (isCtrlPressed)
                            {
                                UserDuplicateSelectedShapes();
                                return true;
                            }
                            return false;

                        case KeyCode.H:
                            if (isShiftPressed)
                            {
                                UserFlipSelectionHorizonally();
                                return true;
                            }
                            else
                            {
                                UserResetCamera();
                            }
                            return true;

                        case KeyCode.I:
                            if (isCtrlPressed)
                            {
                                UserInvertSelection();
                                return true;
                            }
                            return false;

                        case KeyCode.Q:
                            UserSwitchToBoxSelectTool();
                            return true;

                        case KeyCode.R:
                            UserSwitchToRotateTool();
                            return true;

                        case KeyCode.S:
                            if (isCtrlPressed)
                            {
                                UserSaveProjectAs();
                                return true;
                            }
                            else if (isShiftPressed)
                            {
                                UserSnapSelectionToGrid();
                                return true;
                            }
                            UserSwitchToScaleTool();
                            return true;

                        case KeyCode.V:
                            if (isCtrlPressed)
                            {
                                UserPaste();
                                return true;
                            }
                            else if (isShiftPressed)
                            {
                                UserFlipSelectionVertically();
                                return true;
                            }
                            return false;

                        case KeyCode.W:
                            UserSwitchToTranslateTool();
                            return true;

                        case KeyCode.X:
                            UserDeleteSelection();
                            return true;

                        case KeyCode.Y:
                            if (hasFocus && isCtrlPressed)
                            {
                                UserRedo();
                                return true;
                            }
                            return true;

                        case KeyCode.Z:
                            if (hasFocus && isCtrlPressed)
                            {
                                UserUndo();
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

        private void OnWindowResize(float2 lastWindowSize, float2 screenDelta)
        {
            // keep the camera centered by panning the viewport the correct amount to counteract resizing.
            gridOffset += screenDelta * 0.5f;
        }

        private void OnDragDrop(string path)
        {
            if (FileEx.IsJsonFile(path))
            {
                OpenProject(path);
            }
            else
            {
                gridBackgroundImage = FileEx.LoadImage(path);
            }
        }
    }
}

#endif