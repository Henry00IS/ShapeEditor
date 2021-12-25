#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Caches string meshes over 4 frames, to reduce garbage collection of meshes.</summary>
    public static class BmFontCache
    {
        /// <summary>Represents a cached string.</summary>
        private class BmStringData
        {
            public Mesh mesh;
            public int lifetime = 4;
        }

        /// <summary>The cache by font and then by string of mesh data.</summary>
        private static Dictionary<BmFont, Dictionary<string, BmStringData>> cache = new Dictionary<BmFont, Dictionary<string, BmStringData>>();

        /// <summary>Called by the shape editor whenever it begins rendering another frame.</summary>
        public static void OnRenderFrame()
        {
            Dictionary<BmFont, Dictionary<string, BmStringData>> keep = new Dictionary<BmFont, Dictionary<string, BmStringData>>();

            foreach (var strings in cache)
            {
                foreach (var text in strings.Value)
                {
                    if (text.Value.lifetime-- > 0)
                    {
                        Dictionary<string, BmStringData> keepStrings;
                        if (!keep.TryGetValue(strings.Key, out keepStrings))
                        {
                            keepStrings = new Dictionary<string, BmStringData>();
                            keep.Add(strings.Key, keepStrings);
                        }

                        keepStrings.Add(text.Key, text.Value);
                    }
                }
            }

            cache = keep;
        }

        private static Mesh emptyMesh;

        public static Mesh GetStringMesh(BmFont font, string text)
        {
            // we can't handle null strings so return an empty mesh.
            if (text == null) { if (emptyMesh == null) emptyMesh = new Mesh(); return emptyMesh; };

            // find the strings associated with the font.
            Dictionary<string, BmStringData> strings;
            if (!cache.TryGetValue(font, out strings))
            {
                // not found, create a dictionary of strings.
                strings = new Dictionary<string, BmStringData>();
                cache.Add(font, strings);
            }

            // find font data associated with the text.
            BmStringData stringData;
            if (!strings.TryGetValue(text, out stringData))
            {
                // not found, create string data.
                stringData = new BmStringData();
                stringData.mesh = font.BuildStringMesh(text);
                strings.Add(text, stringData);
            }

            // after exiting play mode the mesh may be null (why?) so check for that too.
            if (!stringData.mesh)
            {
                stringData.mesh = font.BuildStringMesh(text);
            }

            // reset the lifetime.
            stringData.lifetime = 4;

            return stringData.mesh;
        }
    }
}

#endif