#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AeternumGames.ShapeEditor
{
    /// <summary>
    /// Represents a mesh target that, when selected, can receive meshes created by the 2D Shape Editor.
    /// </summary>
    [CustomEditor(typeof(ShapeEditorTarget))]
    public class ShapeEditorTargetInspector : Editor
    {
        private SerializedProperty spTargetMode => serializedObject.FindProperty(nameof(ShapeEditorTarget.targetMode));
        private SerializedProperty spPolygonDoubleSided => serializedObject.FindProperty(nameof(ShapeEditorTarget.polygonDoubleSided));
        private SerializedProperty spFixedExtrudeDistance => serializedObject.FindProperty(nameof(ShapeEditorTarget.fixedExtrudeDistance));
        private SerializedProperty spSplineExtrudePrecision => serializedObject.FindProperty(nameof(ShapeEditorTarget.splineExtrudePrecision));
        private SerializedProperty spRevolveExtrudePrecision => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveExtrudePrecision));
        private SerializedProperty spRevolveExtrudeDegrees => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveExtrudeDegrees));
        private SerializedProperty spRevolveExtrudeRadius => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveExtrudeRadius));
        private SerializedProperty spRevolveExtrudeHeight => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveExtrudeHeight));
        private SerializedProperty spRevolveExtrudeSloped => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveExtrudeSloped));
        private SerializedProperty spLinearStaircasePrecision => serializedObject.FindProperty(nameof(ShapeEditorTarget.linearStaircasePrecision));
        private SerializedProperty spLinearStaircaseDistance => serializedObject.FindProperty(nameof(ShapeEditorTarget.linearStaircaseDistance));
        private SerializedProperty spLinearStaircaseHeight => serializedObject.FindProperty(nameof(ShapeEditorTarget.linearStaircaseHeight));
        private SerializedProperty spLinearStaircaseSloped => serializedObject.FindProperty(nameof(ShapeEditorTarget.linearStaircaseSloped));
        private SerializedProperty spScaledExtrudeDistance => serializedObject.FindProperty(nameof(ShapeEditorTarget.scaledExtrudeDistance));
        private SerializedProperty spScaledExtrudeFrontScale => serializedObject.FindProperty(nameof(ShapeEditorTarget.scaledExtrudeFrontScale));
        private SerializedProperty spScaledExtrudeBackScale => serializedObject.FindProperty(nameof(ShapeEditorTarget.scaledExtrudeBackScale));
        private SerializedProperty spScaledExtrudeOffset => serializedObject.FindProperty(nameof(ShapeEditorTarget.scaledExtrudeOffset));
        private SerializedProperty spRevolveChoppedPrecision => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveChoppedPrecision));
        private SerializedProperty spRevolveChoppedDegrees => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveChoppedDegrees));
        private SerializedProperty spRevolveChoppedDistance => serializedObject.FindProperty(nameof(ShapeEditorTarget.revolveChoppedDistance));

        public override void OnInspectorGUI()
        {
            ShapeEditorMenu_OnGUI();

            var shapeEditorTarget = (ShapeEditorTarget)target;

            bool rebuild = GUILayout.Button("Rebuild");

            EditorGUILayout.PropertyField(spTargetMode);

            switch ((ShapeEditorTargetMode)spTargetMode.enumValueIndex)
            {
                case ShapeEditorTargetMode.Polygon:
                    Polygon_OnGUI();
                    break;

                case ShapeEditorTargetMode.FixedExtrude:
                    FixedExtrude_OnGUI();
                    break;

                case ShapeEditorTargetMode.SplineExtrude:
                    SplineExtrude_OnGUI();
                    break;

                case ShapeEditorTargetMode.RevolveExtrude:
                    RevolveExtrude_OnGUI();
                    break;

                case ShapeEditorTargetMode.LinearStaircase:
                    LinearStaircase_OnGUI();
                    break;

                case ShapeEditorTargetMode.ScaledExtrude:
                    ScaledExtrude_OnGUI();
                    break;

                case ShapeEditorTargetMode.RevolveChopped:
                    RevolveChopped_OnGUI();
                    break;
            }

            if (serializedObject.ApplyModifiedProperties() || rebuild)
            {
                shapeEditorTarget.Rebuild();
            }
        }

        private void Polygon_OnGUI()
        {
            EditorGUILayout.PropertyField(spPolygonDoubleSided, new GUIContent("Double Sided"));
        }

        private void FixedExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spFixedExtrudeDistance, new GUIContent("Distance"));
        }

        private void SplineExtrude_OnGUI()
        {
            EditorGUILayout.PropertyField(spSplineExtrudePrecision, new GUIContent("Precision"));
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
            var shapeEditorTarget = (ShapeEditorTarget)target;

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