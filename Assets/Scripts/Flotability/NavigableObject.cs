using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OceanSystem
{
    [RequireComponent(typeof(FloatingObject))]
    public class NavigableObject : MonoBehaviour
    {
        [SerializeField] private FloatingObject _floatingObject;

        [SerializeField, Range(-1, 1)] private float direction;
        [SerializeField, Range(0, 1)] private float maxTurnSpeed;

        [SerializeField, Range(-1, 1)] private float throttle;
        [SerializeField, Range(0, 5)] private float maxSpeed;

        private void OnEnable()
        {
            _floatingObject = gameObject.GetComponent<FloatingObject>();
        }

        private void Update()
        {
            var delta = Time.deltaTime;
            _floatingObject.MoveFwd(throttle * maxSpeed * delta);
            _floatingObject.Rotate(direction * maxTurnSpeed * delta);
        }

    }
}
