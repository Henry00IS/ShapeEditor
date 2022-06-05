#if UNITY_EDITOR

using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>The inspector window with context sensitive properties.</summary>
    public class BackgroundSettingsWindow : GuiWindow
    {
        private static readonly float2 windowSize = new float2(170, 85);
        private GuiFloatTextbox backgroundScaleTextbox;
        private GuiFloatTextbox backgroundTransparencyTextbox;

        public BackgroundSettingsWindow() : base(float2.zero, windowSize) { }

        public override void OnActivate()
        {
            base.OnActivate();
            var resources = ShapeEditorResources.Instance;

            position = GetBottomRightPosition();

            Add(new GuiWindowTitle("Background Settings"));

            var layout = new GuiTableLayout(this, 5, 24);

            layout.Add(new GuiLabel("Image File"));
            layout.Space(60);
            layout.Add(new GuiButton(resources.shapeEditorOpen, "...", new float2(40, 16), SelectBackgroundImage));
            layout.Add(new GuiButton("Clear", new float2(40, 16), ClearBackgroundImage));

            layout.NextRow(4);

            layout.Add(new GuiLabel("Scale"));
            layout.Space(60);
            layout.Add(backgroundScaleTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false });

            layout.NextRow(4);

            layout.Add(new GuiLabel("Transparency"));
            layout.Space(60);
            layout.Add(backgroundTransparencyTextbox = new GuiFloatTextbox(new float2(40, 16)) { allowNegativeNumbers = false, maxValue = 1f });
        }

        public override void OnRender()
        {
            editor.gridBackgroundScale = backgroundScaleTextbox.UpdateValue(editor.gridBackgroundScale);
            editor.gridBackgroundAlpha = backgroundTransparencyTextbox.UpdateValue(editor.gridBackgroundAlpha);

            base.OnRender();
        }

        private float2 GetBottomRightPosition()
        {
            return new float2(
                Mathf.RoundToInt(editor.position.width - windowSize.x - 20),
                Mathf.RoundToInt(editor.position.height - windowSize.y - 42)
            );
        }

        private void ClearBackgroundImage()
        {
            editor.gridBackgroundImage = null;
        }

        private void SelectBackgroundImage()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Load Background Image", "", new string[] { "Image Files", "png,jpg,jpeg" });
            if (path.Length != 0)
            {
                // when the file can not be read, LoadImage() will display a question mark.
                byte[] fileBytes = null;
                try { fileBytes = File.ReadAllBytes(path); } catch (System.Exception) { }

                var backgroundImage = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                backgroundImage.LoadImage(fileBytes);
                editor.gridBackgroundImage = backgroundImage;
            }
        }
    }
}

#endif