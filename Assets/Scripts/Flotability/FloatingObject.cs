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

        [SerializeField] private List<Vector3> _floatingPoints;

        private float gizmoScale = 0.5f;
        [SerializeField, Range(0,1)] private float _displacementStiffness;
        [SerializeField, Range(0, 1)] private float _rotationStiffness;

        public List<Vector3> FloatingPoints => _floatingPoints;

        private void Awake()
        {
            var ocean = GameObject.Find("Ocean");
            _oceanSimulation = ocean.GetComponent<OceanSimulation>();
            _position = transform.position;
        }

        private void Update()
        {
            if (_oceanSimulation.Setup())
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

                transform.position = _position +  new Vector3( avgDisplacemet.x * _displacementStiffness, avgHeight / _floatingPoints.Count, avgDisplacemet.z * _displacementStiffness);
                transform.up = Vector3.Lerp(transform.up, avgUp, _rotationStiffness);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < _floatingPoints.Count; i++)
            {
                var point = transform.TransformPoint(_floatingPoints[i]);
                Gizmos.DrawSphere(point, gizmoScale);
            }
        }
    }
}