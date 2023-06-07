using UnityEngine;
namespace OceanSystem
{
    public class FlagOrientationComponent : MonoBehaviour
    {
        Transform _windIndicator;
        Transform _hull;
        Quaternion _initialRotation;
        Wind _wind;
        public float _angle = 0.0f;

        void Awake()
        {
            _windIndicator = transform;
            _wind = GameObject.Find("Ocean").GetComponent<Wind>();
            _hull = GameObject.Find("Boat").transform;
            _initialRotation = _windIndicator.rotation;
        }

        void Update()
        {
            _angle = (_wind.WindDirection + _hull.rotation.eulerAngles.y - 270) % 360;
            _windIndicator.localRotation = Quaternion.AngleAxis((_wind.WindDirection + _hull.rotation.eulerAngles.y - 270) % 360, Vector3.down);
        }
    }
}
