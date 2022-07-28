#if UNITY_EDITOR

using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        internal double lastInteraction;
        internal float lastRenderTime;
        internal bool isLeftMousePressed;
        private bool isRightMousePressed;
        internal float2 mousePosition;
        internal float2 mouseGridPosition;
        internal float2 mouseInitialPosition;
        internal float2 mouseGridInitialPosition;
        internal bool isCtrlPressed;
        internal bool isShiftPressed;
        private int desiredMouseCursorTimer;
        private MouseCursor desiredMouseCursor;
        private Texture2D customMouseCursor;
        private float2 customMouseHotspot;
        private string desiredTooltipText;

        /// <summary>Called by the Unity Editor to process events.</summary>
        private void OnGUI()
        {
            // ensure the project is ready, it has data that's lost on deserialization.
            project.Validate();

            // continue to request mouse move events, as this flag can get reset upon c# reloads.
            wantsMouseMove = true;

            var e = Event.current;

            if (e.type == EventType.Repaint)
            {
                // set the desired tooltip.
                if (desiredTooltipText != null)
                {
                    GUI.Label(GetViewportRect(), new GUIContent("", desiredTooltipText));
                    desiredTooltipText = null;
                }

                var time = Time.realtimeSinceStartup;
                OnRepaint();
                lastRenderTime = Time.realtimeSinceStartup - time;

                // set the desired mouse cursor.
                if (desiredMouseCursor != MouseCursor.Arrow)
                {
                    EditorGUIUtility.AddCursorRect(GetViewportRect(), desiredMouseCursor);

                    // set the custom mouse cursor texture.
                    if (customMouseCursor != null)
                    {
                        Cursor.SetCursor(customMouseCursor, customMouseHotspot, CursorMode.Auto);
                        customMouseCursor = null;
                    }
                    else
                    {
                        // reset the mouse cursor here, when the function is no longer getting called.
                        if (--desiredMouseCursorTimer <= 0)
                        {
                            customMouseCursor = null;
                            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                            desiredMouseCursor = MouseCursor.Arrow;
                        }
                    }
                }
            }

            // get the mouse position.
            float2 eMousePosition = e.mousePosition;

            // use a passive hot control to detect mouse up outside of the editor window.
            int hotControlId = GUIUtility.GetControlID(FocusType.Passive);

            if (e.type == EventType.MouseDown)
            {
                if (IsMousePositionInViewport(eMousePosition))
                {
                    UpdateLastInteractedTime();

                    // ensure we have input focus (e.g. not some imgui textbox).
                    GUI.FocusControl(null);

                    // set a hot control to detect mouse up outside of the editor window.
                    GUIUtility.hotControl = hotControlId;

                    mousePosition = eMousePosition;
                    mouseGridPosition = ScreenPointToGrid(mousePosition);
                    if (e.button == 0)
                    {
                        // keep a copy of the initial click positions.
                        mouseInitialPosition = mousePosition;
                        mouseGridInitialPosition = mouseGridPosition;
                        isLeftMousePressed = true;
                    }
                    if (e.button == 1) isRightMousePressed = true;
                    OnMouseDown(e.button);

                    e.Use();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                OnGlobalMouseUp(e.button);

                if (IsMousePositionInViewport(eMousePosition))
                {
                    UpdateLastInteractedTime();

                    // reset the hot control.
                    GUIUtility.hotControl = 0;

                    mousePosition = eMousePosition;
                    mouseGridPosition = ScreenPointToGrid(mousePosition);
                    if (e.button == 0) isLeftMousePressed = false;
                    if (e.button == 1) isRightMousePressed = false;
                    OnMouseUp(e.button);

                    e.Use();
                }
            }

            if (e.type != EventType.MouseUp && e.rawType == EventType.MouseUp)
            {
                UpdateLastInteractedTime();

                // reset the hot control.
                GUIUtility.hotControl = 0;

                mousePosition = eMousePosition;
                mouseGridPosition = ScreenPointToGrid(mousePosition);
                if (e.button == 0) isLeftMousePressed = false;
                if (e.button == 1) isRightMousePressed = false;
                OnGlobalMouseUp(e.button);
            }

            if (e.type == EventType.MouseDrag)
            {
                if (IsMousePositionInViewport(eMousePosition))
                {
                    var previousMouseGridPosition = mouseGridPosition;
                    mousePosition = eMousePosition;
                    mouseGridPosition = ScreenPointToGrid(mousePosition);
                    OnMouseDrag(e.button, e.delta, mouseGridPosition - previousMouseGridPosition);

                    e.Use();
                }
            }

            if (e.type == EventType.MouseMove)
            {
                var previousMouseGridPosition = mouseGridPosition;
                mousePosition = eMousePosition;
                mouseGridPosition = ScreenPointToGrid(mousePosition);
                OnMouseMove(e.delta, mouseGridPosition - previousMouseGridPosition);
            }

            if (e.type == EventType.ScrollWheel)
            {
                if (IsMousePositionInViewport(eMousePosition))
                {
                    UpdateLastInteractedTime();

                    mousePosition = eMousePosition;
                    mouseGridPosition = ScreenPointToGrid(mousePosition);
                    OnMouseScroll(e.delta.y);

                    e.Use();
                }
            }

            if (e.type == EventType.KeyDown)
            {
                UpdateLastInteractedTime();

                isCtrlPressed = e.modifiers.HasFlag(EventModifiers.Control);
                isShiftPressed = e.modifiers.HasFlag(EventModifiers.Shift);

                if (OnKeyDown(e.keyCode))
                    e.Use();
            }

            if (e.type == EventType.KeyUp)
            {
                isCtrlPressed = e.modifiers.HasFlag(EventModifiers.Control);
                isShiftPressed = e.modifiers.HasFlag(EventModifiers.Shift);

                if (OnKeyUp(e.keyCode))
                    e.Use();
            }

            if (e.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                e.Use();
            }

            if (e.type == EventType.DragPerform)
            {
                UpdateLastInteractedTime();

                DragAndDrop.AcceptDrag();

                if (DragAndDrop.paths.Length > 0)
                {
                    OpenProject(DragAndDrop.paths[0]);
                }

                e.Use();
            }
        }

        /// <summary>Records the time of the last interaction with this window.</summary>
        private void UpdateLastInteractedTime()
        {
            lastInteraction = Time.realtimeSinceStartupAsDouble;
        }

        /// <summary>Called when a new 2D Shape Editor window is created.</summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("Shape Editor", ShapeEditorResources.Instance.shapeEditorIcon);

            // this window has just been created and is thus the last interacted one.
            UpdateLastInteractedTime();
        }

        /// <summary>
        /// Called by the "Window/2D Shape Editor" entry in the main menu and opens it.
        /// </summary>
        [MenuItem("Window/2D Shape Editor")]
        public static void Init()
        {
            // get existing open window or if none, make a new one:
            ShapeEditorWindow window = GetWindow<ShapeEditorWindow>();
            window.minSize = new float2(800, 600);
            window.Show();
            window.minSize = new float2(128, 128);
        }

        /// <summary>Gets an existing open window or if none, make a new one and opens it.</summary>
        public static ShapeEditorWindow InitAndGetHandle()
        {
            // find all shape editor windows.
            var windows = Resources.FindObjectsOfTypeAll<ShapeEditorWindow>();

            // find the candidate window that was last interacted with.
            ShapeEditorWindow candidate = null;
            double highestInteractionTime = double.NegativeInfinity;
            for (int i = 0; i < windows.Length; i++)
            {
                var window = windows[i];
                if (window.lastInteraction > highestInteractionTime)
                {
                    highestInteractionTime = window.lastInteraction;
                    candidate = windows[i];
                }
            }

            // return the most likely candidate window if found.
            if (candidate)
            {
                candidate.Focus();
                return candidate;
            }

            // else create and return a new window.
            Init();
            return GetWindow<ShapeEditorWindow>();
        }

        /// <summary>Adds additional items to the "Add Tab" menu.</summary>
        public override IEnumerable<System.Type> GetExtraPaneTypes()
        {
            // allow the user to create multiple shape editor windows.
            return new System.Type[] { typeof(ShapeEditorWindow) };
        }

        internal Rect GetViewportRect()
        {
            return new Rect(0, 0, position.width, position.height);
        }

        private bool IsMousePositionInViewport(float2 mousePosition)
        {
            return new Rect(0, 0, position.width, position.height).Contains(mousePosition);
        }

        /// <summary>While this function is called every repaint, it will set the mouse cursor.</summary>
        /// <param name="cursor">The mouse cursor to use.</param>
        internal void SetMouseCursor(MouseCursor cursor)
        {
            desiredMouseCursor = cursor;
            desiredMouseCursorTimer = 1;
        }

        /// <summary>While this function is called every repaint, it will set the mouse cursor.</summary>
        /// <param name="cursor">The mouse cursor texture to use.</param>
        /// <param name="hotspot">The offset from the top left of the texture to use as the target point.</param>
        internal void SetMouseCursor(Texture2D cursor, float2 hotspot = default)
        {
            desiredMouseCursor = MouseCursor.CustomCursor;
            customMouseCursor = cursor;
            customMouseHotspot = hotspot;
            desiredMouseCursorTimer = 1;
        }

        /// <summary>While this function is called every repaint, it will set the tooltip text.</summary>
        /// <param name="tooltip">The tooltip text to display when the mouse is idling.</param>
        internal void SetTooltipText(string tooltip)
        {
            if (tooltip == null) return;
            desiredTooltipText = tooltip;
        }

        /// <summary>Whether the Ctrl or Shift key is pressed.</summary>
        internal bool isModifierPressed => isCtrlPressed || isShiftPressed;
    }
}

#endif