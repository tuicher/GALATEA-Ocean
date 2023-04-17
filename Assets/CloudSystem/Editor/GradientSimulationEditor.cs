using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GradientSimulation))]
public class GradientSimulationEditor : Editor
{
    GradientSimulation _noiseComponent;
    Editor _noiseEditor;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (_noiseComponent._simplexNoiseParams != null)
            DrawParams(_noiseComponent._simplexNoiseParams, ref _noiseEditor);
        
    }

    void DrawParams(Object obj, ref Editor editor)
    {
        if (obj == null)
            return;
        
         var foldout = EditorGUILayout.InspectorTitlebar (true, obj);
            using (var check = new EditorGUI.ChangeCheckScope ()) 
            {
                if (foldout) {
                    CreateCachedEditor (obj, null, ref editor);
                    editor.OnInspectorGUI ();
                }
            }
    }

    void OnEnable()
    {
        _noiseComponent = (GradientSimulation)target;
    }
}