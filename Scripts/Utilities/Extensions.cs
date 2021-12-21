#if UNITY_EDITOR

using System.Collections.Generic;

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
    }
}

#endif