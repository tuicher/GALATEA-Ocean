using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    public class SailOrientationComponent : MonoBehaviour
    {
        [Range(-1.0f, 1.0f)] public float _sailOrientation = 0.0f;

        Transform _sailPivot;
        Wind _wind;


        private void Awake()
        {
            _sailPivot = transform;
            _wind = GameObject.Find("Ocean").GetComponent<Wind>();
        }

        private void Update()
        {
            _sailPivot.localRotation = Quaternion.AngleAxis(_sailOrientation * 80.0f, Vector3.up);


        }

    }
}
