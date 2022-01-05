#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Represents a mesh target that, when selected, can receive meshes created by the 2D Shape Editor.
    /// </summary>
    [CustomEditor(typeof(RealtimeCSGTarget))]
    public class RealtimeCSGTargetInspector : Editor
    {
        private SerializedProperty spTargetMode => serializedObject.FindProperty("targetMode");
        private SerializedProperty spFixedExtrudeDistance => serializedObject.FindProperty("fixedExtrudeDistance");

        public override void OnInspectorGUI()
        {
            ShapeEditorMenu_OnGUI();

            var shapeEditorTarget = (RealtimeCSGTarget)target;

            bool rebuild = GUILayout.Button("Rebuild");

            EditorGUILayout.PropertyField(spTargetMode);

            switch ((RealtimeCSGTargetMode)spTargetMode.enumValueIndex)
            {
                case RealtimeCSGTargetMode.FixedExtrude:
                    FixedExtrude_OnGUI();
                    break;
            }

            if (serializedObject.ApplyModifiedProperties() || rebuild)
            {
                shapeEditorTarget.Rebuild();
            }
        }

        private void FixedExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spFixedExtrudeDistance);
        }

        private void ShapeEditorMenu_OnGUI()
        {
            var shapeEditorTarget = (RealtimeCSGTarget)target;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUIStyle createBrushStyle = ShapeEditorResources.toolbarButtonStyle;

            if (GUILayout.Button(new GUIContent(" Shape Editor", ShapeEditorResources.Instance.shapeEditorIcon, "Show 2D Shape Editor"), createBrushStyle))
            {
                // display the 2d shape editor.
                ShapeEditorWindow.Init();
            }

            if (GUILayout.Button(new GUIContent(" Restore Project", ShapeEditorResources.Instance.shapeEditorRestore, "Load Embedded Project Into 2D Shape Editor"), createBrushStyle))
            {
                // display the 2d shape editor.
                ShapeEditorWindow window = ShapeEditorWindow.InitAndGetHandle();
                // load a copy of the embedded project into the editor.
                window.OpenProject(shapeEditorTarget.project.Clone());
            }
            GUILayout.EndHorizontal();
        }
    }
}

#endif