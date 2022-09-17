#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public static class FileEx
    {
        /// <summary>Reads the first byte of a file and checks whether it begins with "{".</summary>
        /// <param name="path">The file path to be opened.</param>
        /// <returns>True when the file contents begins with "{" else false.</returns>
        public static bool IsJsonFile(string path)
        {
            try
            {
                using var stream = File.OpenRead(path);
                return stream.ReadByte() == 123; // {
            }
            catch (System.Exception) { }
            return false;
        }

        /// <summary>
        /// Attempts to open the file at the specified path as an image. It will also load project
        /// assets and materials.
        /// </summary>
        /// <param name="path">The file path to be opened.</param>
        /// <returns>The texture that was loaded (is a question mark on failure).</returns>
        public static Texture2D LoadImage(string path)
        {
            // if the file is in the project directory:
            if (path.StartsWith("Assets") || path.StartsWith("Packages"))
            {
                // try loading the asset directly.
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (assetType != null)
                {
                    if (assetType == typeof(Texture2D))
                    {
                        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        if (texture.format == TextureFormat.RGBA32)
                            return texture;
                    }
                    else if (assetType == typeof(Material))
                    {
                        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                        if (material.mainTexture is Texture2D texture)
                            return texture;
                    }
                }
            }

            // when the file can not be read, LoadImage() will display a question mark.
            byte[] fileBytes = null;
            try { fileBytes = File.ReadAllBytes(path); } catch (System.Exception) { }

            var image = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            image.LoadImage(fileBytes);
            return image;
        }
    }
}

#endif