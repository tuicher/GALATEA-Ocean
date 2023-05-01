using UnityEngine;

public class CloudSimulation : MonoBehaviour
{
    private Transform _container;
    [SerializeField] public float _cloudScale = 1.0f;
    [SerializeField] public Vector3 _cloudOffset = Vector3.zero;
    [SerializeField] public float _densityThreshold = 0.5f;
    [SerializeField] public float _densityMultiplier = 1.0f;
    [SerializeField] public float _detailMultiplier = 1.0f;
    [SerializeField] public float _containerEdgeFadeDistance = 100.0f;
    [SerializeField, Range(1,64)] public int _numSteps = 8;
    [SerializeField] private Vector3 _dir;
    [SerializeField] private float _speed = 1.0f;
    [SerializeField] public Vector3 _detailNoiseWeight = Vector3.one;
    [SerializeField] public Vector4 _volumeNoiseWeight = Vector4.one;
    [SerializeField] public Vector2 _heightOffset = Vector2.zero;
    [SerializeField] public float _lightAbsortionSun = 1.0f;
    [SerializeField] public float _lightAbsortionCloud = 1.0f;
    [SerializeField, Range(0,1)] public float _darknessThreshold = 1.0f;
    [SerializeField, Range(0,1)] public float _fwdScattering = 0.0f;
    [SerializeField, Range(0,1)] public float _bckScattering = 0.0f;
    [SerializeField, Range(0,1)] public float _bareBrightness = 0.0f;
    [SerializeField, Range(0,1)] public float _phaseFactor = 0.0f;
    [SerializeField] public float _precalcNoiseStrenght = 0.0f;
    [SerializeField] public Texture2D _precalcNoise;
    public Vector3 MinBounds
    {
        get
        {
            if (_container == null)
            {
                _container = transform;
            }

            return _container.position - _container.localScale / 2f;
        }
    }
    public Vector3 MaxBounds
    {
        get
        {
            if (_container == null)
            {
                _container = transform;
            }

            return _container.position + _container.localScale / 2f;
        }
    }

    private void Update()
    {
        _cloudOffset += _dir.normalized * _speed * Time.deltaTime;
    }
}
