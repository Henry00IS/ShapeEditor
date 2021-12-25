﻿#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        internal void OnNewProject()
        {
            RegisterUndo("New Project");
            project = new Project();
        }

        internal void OnOpenProject()
        {
            try
            {
                string path = EditorUtility.OpenFilePanel("Load 2D Shape Editor Project", "", "s2d,sabre2d");
                if (path.Length != 0)
                {
                    // try parsing the project file.
                    var text = System.IO.File.ReadAllText(path);
                    var proj = JsonUtility.FromJson<ProjectUnknown>(text);

                    switch (proj.version)
                    {
                        case 1:
                            RegisterUndo("Load Project");
                            project = JsonUtility.FromJson<ProjectV1>(text).ToV2();
                            break;

                        case 2:
                            RegisterUndo("Load Project");
                            project = JsonUtility.FromJson<Project>(text);
                            break;

                        default:
                            if (EditorUtility.DisplayDialog("2D Shape Editor", "Unsupported project version! Would you like to try loading it anyway?", "Yes", "No"))
                            {
                                RegisterUndo("Load Project");
                                project = JsonUtility.FromJson<Project>(text);
                            }
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("2D Shape Editor", "An exception occured while loading the project:\r\n" + ex.Message, "Ohno!");
            }
        }

        internal void OnSaveProject()
        {
            try
            {
                string path = EditorUtility.SaveFilePanel("Save 2D Shape Editor Project", "", "Project", "s2d");
                if (path.Length != 0)
                {
                    string json = JsonUtility.ToJson(project);
                    System.IO.File.WriteAllText(path, json);
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("2D Shape Editor", "An exception occured while saving the project:\r\n" + ex.Message, "Ohno!");
            }
        }
    }
}

#endif