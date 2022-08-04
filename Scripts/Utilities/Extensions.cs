#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public static class Extensions
    {
        /// <summary>
        /// Pushes the specified value onto the list similar to <see cref="Stack{T}.Push(T)"/>.
        /// <para>Effectively the same as calling Insert(0, value).</para>
        /// <para>
        /// The <see cref="Stack{T}"/> class is not serialized by Unity but <see cref="List{T}"/>
        /// is. These methods are useful to simulate the behaviour of <see cref="Stack{T}"/> with
        /// regular <see cref="List{T}"/>'s that get serialized.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Specifies the type of elements in the collection.</typeparam>
        /// <param name="self">The 'this' reference for the extension method.</param>
        /// <param name="value">The value to be pushed.</param>
        public static void Push<T>(this List<T> self, T value)
        {
            self.Insert(0, value);
        }

        /// <summary>
        /// Pops the specified value from the list similar to <see cref="Stack{T}.Pop(T)"/>.
        /// <para>Effectively the same as retrieving the 0th element then removing it.</para>
        /// <para>
        /// The <see cref="Stack{T}"/> class is not serialized by Unity but <see cref="List{T}"/>
        /// is. These methods are useful to simulate the behaviour of <see cref="Stack{T}"/> with
        /// regular <see cref="List{T}"/>'s that get serialized.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Specifies the type of elements in the collection.</typeparam>
        /// <param name="self">The 'this' reference for the extension method.</param>
        /// <returns>The value on top of the collection.</returns>
        public static T Pop<T>(this List<T> self)
        {
            T last = self[0];
            self.RemoveAt(0);
            return last;
        }

        /// <summary>Moves the item at the specified index to the front of the list.</summary>
        public static void MoveItemAtIndexToFront<T>(this List<T> list, int index)
        {
            T item = list[index];
            for (int i = index; i > 0; i--)
                list[i] = list[i - 1];
            list[0] = item;
        }

        public static List<T> Splice<T>(this List<T> source, int index, int count)
        {
            var items = source.GetRange(index, count);
            source.RemoveRange(index, count);
            return items;
        }

        /// <summary>Parents this transform as a sibling of the active editor transform.</summary>
        public static void SetSiblingOfActiveTransform(this Transform transform)
        {
            // place the new game object below the current selection in the editor.
            var parent = Selection.activeTransform;
            if (parent && parent.parent)
            {
                transform.SetParent(parent.parent);
                transform.SetSiblingIndex(parent.GetSiblingIndex() + 1);
            }
            else if (parent)
            {
                transform.SetSiblingIndex(parent.GetSiblingIndex() + 1);
            }
        }

        /// <summary>Whether the event is the "UndoRedoPerformed" command.</summary>
        public static bool HasPerformedUndoRedo(this Event current)
        {
            if (current == null) return false;
            return (current.type == EventType.ValidateCommand || current.type == EventType.ExecuteCommand) && current.commandName == "UndoRedoPerformed";
        }

        /// <summary>
        /// Gets a method by name in a type that matches all of the parameter names.
        /// </summary>
        /// <param name="type">The type to search inside of.</param>
        /// <param name="name">The name of the method.</param>
        /// <param name="parameterNames">The parameter names to match.</param>
        /// <returns>Returns the method or null if not found.</returns>
        public static MethodInfo GetMethodByName(this Type type, string name, params string[] parameterNames)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                // method names must match.
                if (method.Name != name) continue;

                // the amount of parameters must match.
                var parameters = method.GetParameters();
                if (parameters.Length != parameterNames.Length) continue;

                // the individual parameter names must match.
                for (int i = 0; i < parameters.Length; i++)
                    if (parameters[i].Name != parameterNames[i]) continue;

                return method;
            }

            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="InstructionsAttribute"/> associated with this action or null.
        /// </summary>
        public static InstructionsAttribute GetInstructions(this Action action) => action.Method.GetInstructions();

        /// <summary>
        /// Retrieves the <see cref="InstructionsAttribute"/> associated with this method or null.
        /// </summary>
        public static InstructionsAttribute GetInstructions(this MethodBase method) => method.GetCustomAttribute<InstructionsAttribute>();

        /// <summary>The cached show tooltip method after initialization.</summary>
        private static MethodInfo showTooltipMethod = null;

        /// <summary>
        /// Calls the internal <see cref="EditorWindow.ShowTooltip"/> function that creates a
        /// borderless window without taking input focus.
        /// </summary>
        public static void ShowTooltip(this EditorWindow window)
        {
            if (showTooltipMethod == null)
                showTooltipMethod = typeof(EditorWindow).GetMethodByName("ShowTooltip");
            showTooltipMethod.Invoke(window, null);
        }

        /// <summary>The cached get current mouse position method after initialization.</summary>
        private static MethodInfo getCurrentMousePositionMethod = null;

        /// <summary>
        /// Calls the internal <see cref="Editor.GetCurrentMousePosition"/> function that returns
        /// the actual mouse coordinates of the operating system.
        /// </summary>
        public static Vector2 GetCurrentMousePosition()
        {
            if (getCurrentMousePositionMethod == null)
                getCurrentMousePositionMethod = typeof(Editor).GetMethodByName("GetCurrentMousePosition");
            return (Vector2)getCurrentMousePositionMethod.Invoke(null, null);
        }
    }
}

#endif