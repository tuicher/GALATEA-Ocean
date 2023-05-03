using UnityEngine;
namespace OceanSystem
{
    public class FlagOrientationComponent : MonoBehaviour
    {
        Transform _windIndicator;
        Wind _wind;

        void Awake()
        {
            _windIndicator = transform;
            _wind = GameObject.Find("Ocean").GetComponent<Wind>();
        }

        void Update()
        {
            _windIndicator.localRotation = Quaternion.AngleAxis((_wind.WindDirection - 270) % 360, Vector3.down);
        }
    }
}
