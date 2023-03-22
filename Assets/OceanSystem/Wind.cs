using UnityEngine;
namespace OceanSystem
{
    public class Wind : MonoBehaviour
    {
        [SerializeField, Range(0, 360)] private float _windDirection;
        [SerializeField] private Vector3 _direction;
        public float WindDirection => _windDirection;

        OceanSimulation _oceanSimulation;

        public void Awake()
        {
            _oceanSimulation = GameObject.Find("Ocean").GetComponent<OceanSimulation>();
        }
        public void Update()
        {
            _oceanSimulation._localWindDirection = _windDirection;
            _oceanSimulation._swellDirection = _windDirection;
        }
    }
}
