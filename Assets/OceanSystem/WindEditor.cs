#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace OceanSystem.Editor
{
    
    [CustomEditor(typeof(Wind))]
    public class WindEditor : UnityEditor.Editor
    {
        private Wind _windComponent;
        private readonly float _controllerHeigth = 100.0f;
        private readonly float _controllerRadius = 20.0f;
        private Vector3 _gizmoPosition;

        private void OnEnable()
        {
            _windComponent = (Wind)target;
            _gizmoPosition = _windComponent.transform.position + Vector3.up * _controllerHeigth;
        }

        private void OnSceneGUI()
        {
            var rot = Quaternion.AngleAxis((_windComponent.WindDirection - 180) % 360, Vector3.down);

            Handles.color = Color.white;
            Handles.DrawWireDisc(_gizmoPosition, Vector3.down, _controllerRadius);
            Handles.Label
            (
                _gizmoPosition + rot * (Vector3.right * (_controllerRadius + 5.0f)),
                _windComponent.WindDirection.ToString("0.0")
            );

            Handles.ConeHandleCap
            (
                0,
                _gizmoPosition + rot * (Vector3.right * _controllerRadius),
                rot * Quaternion.Euler(0.0f, 90.0f, 0.0f),
                5.0f,
                EventType.Repaint
            );

        }

        /*
        public override void OnInspectorGUI() 
        {
            base.OnInspectorGUI();
        }
        */
        
    }
    
}
#endif