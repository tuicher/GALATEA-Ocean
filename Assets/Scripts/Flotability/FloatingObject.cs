using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OceanSystem
{
    public class FloatingObject : MonoBehaviour
    {
        [SerializeField] bool _simulateFlotation;
        private OceanSimulation _oceanSimulation;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Quaternion _rotation;
        [SerializeField] private List<Vector3> _floatingPoints;
        [SerializeField, Range(0, 1)] private float _displacementStiffness;
        [SerializeField, Range(0, 1)] private float _rotationStiffness;

        // Properties
        public List<Vector3> FloatingPoints => _floatingPoints;

        // Tooling Paramns
        private float gizmoScale = 0.5f;

        private void Awake()
        {
            var ocean = GameObject.Find("Ocean");
            _oceanSimulation = ocean.GetComponent<OceanSimulation>();

            _position = transform.position;
            _rotation = transform.rotation;
        }

        private void Update()
        {
            if (_simulateFlotation && _oceanSimulation.Setup())
            {
                // Initialize variables
                var avgHeight = 0.0f;
                var avgDisplacemet = Vector3.zero;
                var avgUp = Vector3.zero;

                for (int i = 0; i < _floatingPoints.Count; i++)
                {
                    var pos = transform.TransformPoint(_floatingPoints[i]);

                    avgHeight += _oceanSimulation.Collision.GetWaterHeight(pos);
                    avgDisplacemet += _oceanSimulation.Collision.GetWaterDisplacement(pos);

                    var p1 = pos;
                    var p2 = p1 + Vector3.forward * 2.5f;
                    var p3 = p1 + Vector3.right * 2.5f;

                    p1.y = _oceanSimulation.Collision.GetWaterHeight(p1);
                    p2.y = _oceanSimulation.Collision.GetWaterHeight(p2);
                    p3.y = _oceanSimulation.Collision.GetWaterHeight(p3);

                    avgUp += Vector3.Cross(p2 - p1, p3 - p1).normalized;
                }

                avgHeight /= _floatingPoints.Count;
                avgDisplacemet /= _floatingPoints.Count;
                avgUp /= _floatingPoints.Count;

                transform.position = _position + new Vector3(avgDisplacemet.x * _displacementStiffness, avgHeight, avgDisplacemet.z * _displacementStiffness);

                //transform.rotation = _rotation * Quaternion.Euler(avgUp.x * _rotationStiffness, 0, avgUp.z * _rotationStiffness);

                /*
                var fromTo = Quaternion.FromToRotation(Vector3.up, Vector3.Lerp(transform.up, avgUp, _rotationStiffness));
                fromTo.y = 0;
                transform.rotation = _rotation * fromTo;
                */

                var fromTo = Quaternion.FromToRotation(Vector3.up, avgUp);
                transform.rotation = Quaternion.Slerp( _rotation, _rotation * fromTo, _rotationStiffness);
            
            }
        }

        public void MoveFwd(float amount)
        {
            Move( Vector3.right, amount);
        }
        private void Move(Vector3 direction, float amount = 1.0f)
        {
            _position += transform.TransformVector(direction) * amount;
        }

        public void Rotate(float amount)
        {
            _rotation.y += amount;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < _floatingPoints.Count; i++)
            {
                var point = transform.TransformPoint(_floatingPoints[i]);
                Gizmos.DrawSphere(point, gizmoScale);
            }
        }
    }
}