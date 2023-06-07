using UnityEngine;

public class HUD_Manager : MonoBehaviour
{
    [SerializeField] RectTransform _wheel;
    [SerializeField] RectTransform _sails;
    [SerializeField] RectTransform _windIndicator;
    OceanSystem.Wind _wind;
    Transform _hull;


    [SerializeField] GameObject _boat;
    OceanSystem.SailOrientationComponent _sailOrientationComponent;
    OceanSystem.NavigableObject _navigableObject;
    SailAnimationController _sailAnimationController;
    readonly float _wheelFactor = -105f;
    readonly float _sailsFactor = -80f;
    private int _sailPos = 0;
    private int _wheelPos = 0;
    public float _wheelRotarion = 0.0f;
    public float _sailsRotation = 0.0f;
    private float _wheelRotationVelocity;
    private float _sailsRotationVelocity;

    void Awake()
    {
        _boat = GameObject.Find("Boat");
        _sailOrientationComponent = _boat.GetComponentInChildren<OceanSystem.SailOrientationComponent>();
        _sailAnimationController = _boat.GetComponentInChildren<SailAnimationController>();
        _wind = GameObject.Find("Ocean").GetComponent<OceanSystem.Wind>();
        _hull = _boat.transform;
        _navigableObject = _boat.GetComponent<OceanSystem.NavigableObject>();

    }
    private void Update()
    {
        _wheelRotarion = Mathf.SmoothDamp(_wheelRotarion, _wheelPos * 0.2f, ref _wheelRotationVelocity, Time.deltaTime * 2.0f);
        Quaternion wheelRotation = Quaternion.Euler(0, 0, _wheelRotarion * _wheelFactor);
        _wheel.rotation = Quaternion.Lerp(_wheel.rotation, wheelRotation, Time.deltaTime * 2.0f);
        _navigableObject.direction = _wheelRotarion;

        _sailsRotation = Mathf.SmoothDamp(_sailsRotation, _sailPos * 0.1f, ref _sailsRotationVelocity, Time.deltaTime * 2.0f);
        Quaternion sailsRotation = Quaternion.Euler(0, 0, _sailsRotation * _sailsFactor);
        _sails.rotation = Quaternion.Lerp(_sails.rotation, sailsRotation, Time.deltaTime * 2.0f);
        _sailOrientationComponent.setSailOrientation(_sailsRotation);

        Quaternion windRotation = Quaternion.Euler(0, 0, (_wind.WindDirection + _hull.rotation.eulerAngles.y + 270) % 360);
        _windIndicator.rotation = Quaternion.Lerp(_windIndicator.rotation, windRotation, Time.deltaTime * 2.0f);
    }
    public void TurnWheel(int value)
    {
        if (_wheelPos + value >= -5 && _wheelPos + value <= 5)
            _wheelPos += value;
    }

    public void TurnSails(int value)
    {
        if (_sailPos + value >= -10 && _sailPos + value <= 10)
            _sailPos += value;

    }


    public void SetSails(bool raise)
    {
        if (_sailAnimationController.isAnimating)
            return;

        if (raise)
            _sailAnimationController.RollUp();

        else
            _sailAnimationController.RollDown();

    }
}
