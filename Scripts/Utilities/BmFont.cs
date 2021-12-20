#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>Simple XML Font importer for BMFont.</summary>
    public class BmFont
    {
        /// <summary>Represents a font character.</summary>
        public class BmCharacter
        {
            public int x;
            public int y;
            public int width;
            public int height;
            public int xoffset;
            public int yoffset;
            public int xadvance;
        }

        /// <summary>Collection containing all of the font characters.</summary>
        public readonly Dictionary<int, BmCharacter> characters = new Dictionary<int, BmCharacter>(255);

        /// <summary>How far the cursor should be moved vertically when moving to the next line.</summary>
        public readonly int lineHeight;

        /// <summary>How far from the top the base of the characters should be placed.</summary>
        public readonly int baseHeight;

        /// <summary>The texture containing all of the characters.</summary>
        public readonly Texture2D texture;

        /// <summary>Whether the font has been loaded successfully.</summary>
        private readonly bool ready;

        /// <summary>
        /// Loads the specified BmFont XML file and texture (multiple textures are not supported).
        /// </summary>
        /// <param name="xmlTextAsset">The BmFont XML file as a Unity TextAsset to be loaded.</param>
        /// <param name="texture">The BmFont Texture containing all of the characters.</param>
        public BmFont(TextAsset xmlTextAsset, Texture2D texture)
        {
            if (!xmlTextAsset) { ready = false; Debug.LogWarning("2DSE: Missing a required font xml textasset reference!"); return; }
            if (!texture) { ready = false; Debug.LogWarning("2DSE: Missing a required font texture reference!"); return; }

            this.texture = texture;

            try
            {
                var document = new XmlDocument();
                using (var reader = new StringReader(xmlTextAsset.text))
                    document.Load(reader);

                // load the common information.
                var commonNodes = document.SelectNodes("//font/common");
                foreach (XmlNode node in commonNodes)
                {
                    lineHeight = int.Parse(node.Attributes.GetNamedItem("lineHeight").Value);
                    baseHeight = int.Parse(node.Attributes.GetNamedItem("base").Value);
                    break;
                }

                // load the characters.
                var nodes = document.SelectNodes("//font/chars/char");
                foreach (XmlNode node in nodes)
                {
                    characters.Add(int.Parse(node.Attributes.GetNamedItem("id").Value), new BmCharacter()
                    {
                        x = int.Parse(node.Attributes.GetNamedItem("x").Value),
                        y = int.Parse(node.Attributes.GetNamedItem("y").Value),
                        width = int.Parse(node.Attributes.GetNamedItem("width").Value),
                        height = int.Parse(node.Attributes.GetNamedItem("height").Value),
                        xoffset = int.Parse(node.Attributes.GetNamedItem("xoffset").Value),
                        yoffset = int.Parse(node.Attributes.GetNamedItem("yoffset").Value),
                        xadvance = int.Parse(node.Attributes.GetNamedItem("xadvance").Value),
                    });
                }
            }
            catch (System.Exception ex)
            {
                ready = false;
                Debug.LogWarning("2DSE: Unable to load a font xml file! " + ex.Message);
                return;
            }

            ready = true;
        }

        private List<Vector3> vertices = new List<Vector3>();
        private List<Vector2> uvs = new List<Vector2>();
        private List<int> triangles = new List<int>();
        private float2 offset;

        /// <summary>Builds a mesh for the given string that should be rendered with the texture.</summary>
        /// <param name="text">The text string to create a mesh for.</param>
        /// <returns>The mesh that when rendered shows the text in this font.</returns>
        public Mesh BuildStringMesh(string text)
        {
            var mesh = new Mesh();
            mesh.name = "BmFont String Mesh";

            // if the font failed to load then we stop here.
            if (!ready) return mesh;

            vertices.Clear();
            uvs.Clear();
            triangles.Clear();
            offset = float2.zero;

            foreach (var character in text)
                MeshAddCharacter(character);

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);

            return mesh;
        }

        private void MeshAddCharacter(char characterCode)
        {
            if (characters.TryGetValue(characterCode, out var character))
            {
                Rect rect = new Rect(offset.x + character.xoffset, offset.y + character.yoffset, character.width, character.height);

                vertices.Add(new Vector3(rect.x, rect.y));
                vertices.Add(new Vector3(rect.x + rect.width, rect.y));
                vertices.Add(new Vector3(rect.x + rect.width, rect.y + rect.height));
                vertices.Add(new Vector3(rect.x, rect.y + rect.height));

                var verticesCount = vertices.Count;
                triangles.Add(verticesCount - 1);
                triangles.Add(verticesCount - 4);
                triangles.Add(verticesCount - 2);
                triangles.Add(verticesCount - 2);
                triangles.Add(verticesCount - 4);
                triangles.Add(verticesCount - 3);

                var usize = (1.0f / texture.width);
                var vsize = (1.0f / texture.height);

                var cx = character.x * usize;
                var cy = character.y * usize;
                var cw = character.width * vsize;
                var ch = character.height * vsize;

                var uvtl = new Vector2(cx, 1.0f - cy - ch);
                var uvbr = new Vector3(cx + cw, 1.0f - cy);

                uvs.Add(new Vector2(uvtl.x, uvbr.y));
                uvs.Add(new Vector2(uvbr.x, uvbr.y));
                uvs.Add(new Vector2(uvbr.x, uvtl.y));
                uvs.Add(new Vector2(uvtl.x, uvtl.y));

                offset.x += character.xadvance;
            }
            else
            {
                Debug.Log("Could not find character " + characterCode + " (" + (int)characterCode + ")!");
            }
        }
    }
}

#endif