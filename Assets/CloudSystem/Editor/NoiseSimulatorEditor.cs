using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NoiseSimulator))]
public class NoiseSimulatorEditor : Editor 
{
    NoiseSimulator _noiseComponent;
    Editor _noiseEditor;
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();

        DrawParams( _noiseComponent.ActiveParams, ref _noiseEditor);

        if(GUILayout.Button ("Save Textures"))
            _noiseComponent.SaveTextures();
    }

    void DrawParams(Object obj, ref Editor editor)
    {
        if (obj == null)
            return;
        
         var foldout = EditorGUILayout.InspectorTitlebar (true, obj);
            using (var check = new EditorGUI.ChangeCheckScope ()) {
                if (foldout) {
                    CreateCachedEditor (obj, null, ref editor);
                    editor.OnInspectorGUI ();
                }
                if (check.changed) {
                    _noiseComponent._simulate = true;
                }
            }
    }
    void OnEnable()
    {
        _noiseComponent = (NoiseSimulator)target;
    }
}
