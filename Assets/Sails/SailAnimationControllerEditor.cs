using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SailAnimationController))]
public class SailAnimationControllerEditor : Editor
{ 
    SailAnimationController _sailAnimationController;
    Editor _sailEditor;

     public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Roll Up"))
            _sailAnimationController.RollUp();
        if (GUILayout.Button("Roll Down"))
            _sailAnimationController.RollDown();
    }

    // Update is called once per frame
    void OnEnable()
    {
        _sailAnimationController = (SailAnimationController)target;
    }
}
