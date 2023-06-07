using UnityEngine;

[ExecuteAlways]
public class SailAnimationController : MonoBehaviour
{

    public enum SailStates { RollingUp, RollingDown, Up, Down, Inflating, Deflating}
    public Animator _animator;
    public SailStates _sailState;

    public bool isAnimating => _sailState == SailStates.RollingUp || _sailState == SailStates.RollingDown || _sailState == SailStates.Inflating || _sailState == SailStates.Deflating;

    [Range(0.0f,1.0f)] public float _windDir = 0.5f;
    [Range(0.0f,1.0f)] public float _animPercent;
    [Range(0.000001f,1.0f)] public float _windParam;
    [Range(0.000001f,1.0f)] public float _rollUpParam;


    void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    void Start()
    {
        // Initial State
        _animator.Play("Idle");
        _animator.Update(0.0f);
        //RollUp();
        //RollDown();
    }

    void Update()
    {
        AnimateSail();

        
    }

    Vector3 upValues = new Vector3(0.00001f, 1.0f, 1.0f);
    Vector3 downValues = new Vector3(0.2f, 0.6f, 0.3f);
    Vector3 inflatingValues = new Vector3(1.0f, 0.0f, 0.3f);
    
    float _iterator = 0.0f;
    float _speed = 0.2f;

    Vector3 ActualValues => inflatingValues + Vector3.forward * _windDir * 0.7f;

    void AnimateSail()
    {
        switch (_sailState)
        {
            case SailStates.Up:
                SetValues(upValues);
                break;
            case SailStates.Down:
                SetValues(ActualValues);
                break;
            case SailStates.RollingUp:
                PerformAnimation(ActualValues, downValues, SailStates.Deflating);
                break;
            case SailStates.RollingDown:
                PerformAnimation(upValues, downValues, SailStates.Inflating);
                break;
            case SailStates.Inflating:
                PerformAnimation(downValues, ActualValues, SailStates.Down);
                break;
            case SailStates.Deflating:
                PerformAnimation(downValues, upValues, SailStates.Up);
                break;
        }

        _animator.SetFloat("Wind", _windParam);
        _animator.SetFloat("RollUp", _rollUpParam);
        _animator.Play("Idle", 0, _animPercent);
    }

    public void RollUp()
    {
        _iterator = 0.0f;
        _sailState = SailStates.RollingUp;
    }

    public void RollDown()
    {
         _iterator = 0.0f;
        _sailState = SailStates.RollingDown;
    }

    void PerformAnimation( Vector3 from, Vector3 to, SailStates exitState)
    {
        var values = Vector3.Lerp(from, to, _iterator);

        SetValues(values);

        _iterator += _speed * Time.deltaTime;

        if (_iterator >= 1.0f)
        {
            _sailState = exitState;
            if(exitState == SailStates.Inflating || exitState == SailStates.Deflating)
                _iterator = 0.0f;
        }

    }

    void SetValues(Vector3 values)
    {
        _windParam = values.x;
        _rollUpParam = values.y;
        _animPercent = values.z;
    }
    
}
