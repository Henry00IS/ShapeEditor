#if UNITY_EDITOR

using System;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Provides usage instructions for use with tooltips.</summary>
    public class InstructionsAttribute : Attribute
    {
        public InstructionsAttribute(string title, string description = null, string shortcut = null)
        {
            this.title = title;
            this.description = description;
            this.shortcut = shortcut;
        }

        public string title { get; }
        public string description { get; }
        public string shortcut { get; }

        /// <summary>Builds a fancy human-readable tooltip for mouse hover instructions.</summary>
        /// <param name="mode">The usage instructions display mode.</param>
        public string GetTooltip(InstructionsDisplayMode mode)
        {
            var tooltip = "";

            switch (mode)
            {
                case InstructionsDisplayMode.Menu:

                    if (description != null)
                        tooltip += description;

                    if (shortcut != null)
                        tooltip += " (" + shortcut + ")";

                    return tooltip;
            }

            tooltip = title;

            if (shortcut != null)
                tooltip += " (" + shortcut + ")";

            if (description != null)
                tooltip += "\n\n" + description;

            return tooltip;
        }
    }
}

#endif