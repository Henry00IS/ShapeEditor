#if UNITY_EDITOR

using System.Collections.Generic;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        private List<Widget> widgets = new List<Widget>();
        /// <summary>The widget that currently has input focus or null.</summary>
        internal Widget activeWidget;

        /// <summary>Removes all of the widgets.</summary>
        private void ClearWidgets()
        {
            int widgetsCount = widgets.Count;
            for (int i = 0; i < widgetsCount; i++)
                widgets[i].OnDeactivate();

            widgets.Clear();
        }

        /// <summary>Adds a new widget to the viewport.</summary>
        internal void AddWidget(Widget widget)
        {
            widget.editor = this;
            widgets.Add(widget);

            widget.OnActivate();
        }

        private void DrawWidgets()
        {
            int widgetsCount = widgets.Count;
            for (int i = 0; i < widgetsCount; i++)
                widgets[i].OnRender();
        }

        /// <summary>Attempts to find the first widget that reports as being active.</summary>
        /// <returns>The widget instance if found else null.</returns>
        private Widget FindActiveWidget()
        {
            var widgetsCount = widgets.Count;
            for (int i = 0; i < widgetsCount; i++)
                if (widgets[i].wantsActive)
                    return widgets[i];
            return null;
        }
    }
}

#endif