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
        private SerializedProperty spTargetMode => serializedObject.FindProperty(nameof(RealtimeCSGTarget.targetMode));
        private SerializedProperty spFixedExtrudeDistance => serializedObject.FindProperty(nameof(RealtimeCSGTarget.fixedExtrudeDistance));
        private SerializedProperty spSplineExtrudePrecision => serializedObject.FindProperty(nameof(RealtimeCSGTarget.splineExtrudePrecision));
        private SerializedProperty spRevolveExtrudePrecision => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveExtrudePrecision));
        private SerializedProperty spRevolveExtrudeDegrees => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveExtrudeDegrees));
        private SerializedProperty spRevolveExtrudeRadius => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveExtrudeRadius));
        private SerializedProperty spRevolveExtrudeHeight => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveExtrudeHeight));
        private SerializedProperty spRevolveExtrudeSloped => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveExtrudeSloped));
        private SerializedProperty spLinearStaircasePrecision => serializedObject.FindProperty(nameof(RealtimeCSGTarget.linearStaircasePrecision));
        private SerializedProperty spLinearStaircaseDistance => serializedObject.FindProperty(nameof(RealtimeCSGTarget.linearStaircaseDistance));
        private SerializedProperty spLinearStaircaseHeight => serializedObject.FindProperty(nameof(RealtimeCSGTarget.linearStaircaseHeight));
        private SerializedProperty spLinearStaircaseSloped => serializedObject.FindProperty(nameof(RealtimeCSGTarget.linearStaircaseSloped));
        private SerializedProperty spScaledExtrudeDistance => serializedObject.FindProperty(nameof(RealtimeCSGTarget.scaledExtrudeDistance));
        private SerializedProperty spScaledExtrudeFrontScale => serializedObject.FindProperty(nameof(RealtimeCSGTarget.scaledExtrudeFrontScale));
        private SerializedProperty spScaledExtrudeBackScale => serializedObject.FindProperty(nameof(RealtimeCSGTarget.scaledExtrudeBackScale));
        private SerializedProperty spScaledExtrudeOffset => serializedObject.FindProperty(nameof(RealtimeCSGTarget.scaledExtrudeOffset));
        private SerializedProperty spRevolveChoppedPrecision => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveChoppedPrecision));
        private SerializedProperty spRevolveChoppedDegrees => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveChoppedDegrees));
        private SerializedProperty spRevolveChoppedDistance => serializedObject.FindProperty(nameof(RealtimeCSGTarget.revolveChoppedDistance));

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

                case RealtimeCSGTargetMode.SplineExtrude:
                    SplineExtrude_OnGUI();
                    break;

                case RealtimeCSGTargetMode.RevolveExtrude:
                    RevolveExtrude_OnGUI();
                    break;

                case RealtimeCSGTargetMode.LinearStaircase:
                    LinearStaircase_OnGUI();
                    break;

                case RealtimeCSGTargetMode.ScaledExtrude:
                    ScaledExtrude_OnGUI();
                    break;

                case RealtimeCSGTargetMode.RevolveChopped:
                    RevolveChopped_OnGUI();
                    break;
            }

            // rebuild the target upon detecting an undo or redo event.
            rebuild = rebuild || Event.current.HasPerformedUndoRedo();

            if (serializedObject.ApplyModifiedProperties() || rebuild)
            {
                shapeEditorTarget.Rebuild();
            }
        }

        private void FixedExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spFixedExtrudeDistance);
        }

        private void SplineExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spSplineExtrudePrecision);
        }

        private void RevolveExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spRevolveExtrudePrecision, new GUIContent("Precision"));
            EditorGUILayout.PropertyField(spRevolveExtrudeDegrees, new GUIContent("Degrees"));
            EditorGUILayout.PropertyField(spRevolveExtrudeRadius, new GUIContent("Radius"));
            EditorGUILayout.PropertyField(spRevolveExtrudeHeight, new GUIContent("Spiral Height"));
            EditorGUILayout.PropertyField(spRevolveExtrudeSloped, new GUIContent("Spiral Sloped"));
        }

        private void LinearStaircase_OnGUI()
        {
            // sloped linear staircases always use a precision of 1.
            if (spLinearStaircaseSloped.boolValue)
                EditorGUILayout.LabelField("Precision (= 1)");
            else
                EditorGUILayout.PropertyField(spLinearStaircasePrecision, new GUIContent("Precision"));
            EditorGUILayout.PropertyField(spLinearStaircaseDistance, new GUIContent("Distance"));
            EditorGUILayout.PropertyField(spLinearStaircaseHeight, new GUIContent("Height"));
            EditorGUILayout.PropertyField(spLinearStaircaseSloped, new GUIContent("Sloped"));
        }

        private void ScaledExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spScaledExtrudeDistance, new GUIContent("Distance"));
            EditorGUILayout.PropertyField(spScaledExtrudeFrontScale, new GUIContent("Front Scale"));
            EditorGUILayout.PropertyField(spScaledExtrudeBackScale, new GUIContent("Back Scale"));
            EditorGUIUtility.wideMode = true;
            EditorGUILayout.PropertyField(spScaledExtrudeOffset, new GUIContent("Back Offset"));
            EditorGUIUtility.wideMode = false;
        }

        private void RevolveChopped_OnGUI()
        {
            EditorGUILayout.PropertyField(spRevolveChoppedPrecision, new GUIContent("Precision"));
            EditorGUILayout.PropertyField(spRevolveChoppedDegrees, new GUIContent("Degrees"));
            EditorGUILayout.PropertyField(spRevolveChoppedDistance, new GUIContent("Distance"));
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