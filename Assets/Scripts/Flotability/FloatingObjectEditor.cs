using UnityEditor;
using UnityEngine;

namespace OceanSystem.Editor
{
    [CustomEditor(typeof(FloatingObject))]
    public class FloatingObjectEditor : UnityEditor.Editor
    {
        private FloatingObject _floatingObject;

        private void OnEnable()
        {
            _floatingObject = (FloatingObject)target;
        }

        private void OnSceneGUI()
        {
            for (int i = 0; i < _floatingObject.FloatingPoints.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                var point = _floatingObject.transform.TransformPoint(_floatingObject.FloatingPoints[i]);

                Handles.color = Color.white;
                Handles.Label(point, "Position -" + i);
                
                Handles.color = Color.red;
                var xComponent = Handles.ScaleValueHandle(
                    point.x,
                    point + Vector3.right,
                    Quaternion.Euler(0, 90, 0),
                    2.5f,
                    Handles.ConeHandleCap,
                    1.0f);
                Handles.color = Color.blue;
                var zComponent = Handles.ScaleValueHandle(
                    point.z,
                    point + Vector3.forward,
                    Quaternion.identity,
                    2.5f,
                    Handles.ConeHandleCap,
                    1.0f);
                
                point = new Vector3(xComponent, 0.0f, zComponent);

                _floatingObject.FloatingPoints[i] = _floatingObject.transform.InverseTransformPoint(point);
            }
        }


    }
}

