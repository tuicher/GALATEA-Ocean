using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public class SailOrientationComponent : MonoBehaviour
    {
        [SerializeField, Range(-1.0f, 1.0f)] private float _sailOrientation = 0.0f;

        Transform _sailPivot;
        Wind _wind;
        SailAnimationController _sailAnimationController;

        private void Awake()
        {
            _sailPivot = transform;
            _wind = GameObject.Find("Ocean").GetComponent<Wind>();
            _sailAnimationController = GameObject.Find("Sails").GetComponent<SailAnimationController>();
        }

        private void Update()
        {
            _sailPivot.localRotation = Quaternion.Lerp( _sailPivot.localRotation, Quaternion.AngleAxis(_sailOrientation * 80.0f, Vector3.up), 2.0f * Time.deltaTime);
            
            var v = new Vector3(0.0f, 10.0f, 0.0f);
            var sailsDir = transform.TransformVector(Vector3.right);
            sailsDir.y = 0.0f;
            var windDir = Quaternion.AngleAxis((_wind.WindDirection - 180) % 360, Vector3.down) * Vector3.right;
            
            var dotResult = Vector3.Dot( windDir / windDir.magnitude, sailsDir / sailsDir.magnitude);
            //Debug.Log(windDir.normalized+ " " + sailsDir.normalized + " " + dotResult);
            _sailAnimationController._windDir = dotResult > 0 ? dotResult : 0.0f;
        }

        public void setSailOrientation(float value)
        {
            _sailOrientation = value;
        }

    }
}
