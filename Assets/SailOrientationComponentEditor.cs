#if UNITY_EDITOR
namespace OceanSystem.Editor
{
    using UnityEngine;
    using UnityEditor;
    [CustomEditor(typeof(SailOrientationComponent))]
    public class SailOrientationComponentEditor : Editor
    {
        private Wind _wind;
        private SailOrientationComponent _sails;

        private Transform SailsTransform => _sails.transform;

        private void OnEnable()
        {
            _sails = (SailOrientationComponent)target;
            _wind = GameObject.Find("Ocean").GetComponent<Wind>();
        }

        private void OnSceneGUI()
        {

           
            var v = new Vector3(0.0f, 10.0f, 0.0f);
            var sailsDir = SailsTransform.TransformVector(Vector3.right);
            var windDir = Quaternion.AngleAxis((_wind.WindDirection - 180) % 360, Vector3.down) * Vector3.right;
            Debug.Log(windDir.normalized+ " " + sailsDir.normalized);
            var dotResult = Vector3.Dot( windDir / sailsDir.magnitude, sailsDir / sailsDir.magnitude);
            
            if (dotResult > 0) 
                Handles.color = Color.green;
            else 
                Handles.color = Color.red;
            Handles.ConeHandleCap
            (
                0,
                v,
                SailsTransform.rotation * Quaternion.Euler(0.0f, 90.0f, 0.0f),
                10.0f * dotResult,
                EventType.Repaint
            );

            //Gizmos.DrawLine(v, v + SailsTransform.TransformDirection(SailsTransform.right) * 5.0f);
        }
    }
}

#endif