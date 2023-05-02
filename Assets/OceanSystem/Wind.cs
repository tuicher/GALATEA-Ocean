using UnityEngine;
namespace OceanSystem
{
    public class Wind : MonoBehaviour
    {
        [SerializeField, Range(0, 360)] private float _windDirection;
        [SerializeField] private Vector3 _direction;
        Transform _windIndicator;
        public float WindDirection => _windDirection;

        OceanSimulation _oceanSimulation;

        public void Awake()
        {
            _oceanSimulation = GameObject.Find("Ocean").GetComponent<OceanSimulation>();

            _windIndicator = GameObject.Find("FlagPivotPoint").transform;
        }
        public void Update()
        {
            _oceanSimulation._localWindDirection = _windDirection;
            _oceanSimulation._swellDirection = _windDirection;
            _windIndicator.rotation = Quaternion.AngleAxis((_windDirection - 270) % 360, Vector3.down);
        }
    }
}
