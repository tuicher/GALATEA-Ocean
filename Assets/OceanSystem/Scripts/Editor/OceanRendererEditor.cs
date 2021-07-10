using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    [CustomEditor(typeof(OceanRenderer))]
    public class OceanRendererEditor : Editor
    {
        private SerializedProperty _reflectionsMode;
        private SerializedProperty _probe;
        private SerializedProperty _cubemap;
        private SerializedProperty _material;
        private SerializedProperty _colorsPreset;

        private SerializedProperty _viewer;
        private SerializedProperty _minMeshScale;
        private SerializedProperty _clipMapLevels;
        private SerializedProperty _vertexDensity;

        private void OnEnable()
        {
            FindProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour((OceanRenderer)target), typeof(OceanRenderer), false);
            GUI.enabled = true;

            EditorGUILayout.PropertyField(_material);
            EditorGUILayout.PropertyField(_colorsPreset);
            EditorGUILayout.PropertyField(_reflectionsMode);
            switch ((OceanRenderer.OceanReflectionsMode)_reflectionsMode.enumValueIndex)
            {
                case OceanRenderer.OceanReflectionsMode.RealtimeProbe:
                    EditorGUILayout.PropertyField(_probe);
                    break;
                case OceanRenderer.OceanReflectionsMode.Custom:
                    EditorGUILayout.PropertyField(_cubemap);
                    break;
                default:
                    break;
            }

            EditorGUILayout.PropertyField(_viewer);
            EditorGUILayout.PropertyField(_minMeshScale);
            EditorGUILayout.PropertyField(_clipMapLevels);
            EditorGUILayout.PropertyField(_vertexDensity);

            serializedObject.ApplyModifiedProperties();
        }

        private void FindProperties()
        {
            _reflectionsMode = serializedObject.FindProperty("_reflectionsMode");
            _probe = serializedObject.FindProperty("_probe");
            _cubemap = serializedObject.FindProperty("_cubemap");
            _material = serializedObject.FindProperty("_material");
            _colorsPreset = serializedObject.FindProperty("_colorsPreset");

            _viewer = serializedObject.FindProperty("_viewer");
            _minMeshScale = serializedObject.FindProperty("_minMeshScale");
            _clipMapLevels = serializedObject.FindProperty("_clipMapLevels");
            _vertexDensity = serializedObject.FindProperty("_vertexDensity");
        }
    }
}


