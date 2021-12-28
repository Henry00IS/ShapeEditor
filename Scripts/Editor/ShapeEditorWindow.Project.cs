#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    public partial class ShapeEditorWindow
    {
        /// <summary>Pushes the current project onto the undo stack and creates a new project.</summary>
        private void NewProject()
        {
            RegisterUndo("New Project");
            project = new Project();
        }

        /// <summary>
        /// Attempts to open the project at the specified file path. May prompt the user.
        /// </summary>
        /// <param name="path">The file path of the project file.</param>
        private void OpenProject(string path)
        {
            try
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
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("2D Shape Editor", "An exception occured while loading the project:\r\n" + ex.Message, "Ohno!");
            }
        }

        /// <summary>Pushes the current project onto the undo stack and opens the specified project immediately.</summary>
        /// <param name="project">The project to be loaded.</param>
        internal void OpenProject(Project project)
        {
            RegisterUndo("Load Project");
            this.project = project;
        }

        /// <summary>Saves the current project to the specified file path.</summary>
        /// <param name="path">The file path to save the project to.</param>
        /// <exception cref="System.IO.IOException"></exception>
        private void SaveProject(string path)
        {
            string json = JsonUtility.ToJson(project);
            System.IO.File.WriteAllText(path, json);
        }
    }
}

#endif