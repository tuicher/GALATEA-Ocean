using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AlphaCameraController : MonoBehaviour
{
    private PlayerActions _inputManager;
    private Camera _mainCam;
    [SerializeField] private float _speed;
    [SerializeField] private float _speedBonus = 2.5f;
    private Vector2 _moveInput;
    private float _sprintSpeed;
    [SerializeField, Range(0, 5)] private float _sensibilityX;
    [SerializeField, Range(0, 5)] private float _sensibilityY;
    private Vector2 _rotation = Vector2.zero;
    private float _verticalMovement = 0.0f;
    private Vector2 _mouseLook;
    
    void Awake()
    {
        _mainCam =  Camera.main;

        _inputManager = new PlayerActions();
        Cursor.lockState = CursorLockMode.Locked;

        _inputManager.FreeView.Sprint.started += ctx => StartSprint();
        _inputManager.FreeView.Sprint.canceled += ctx => StopSprint();

        _inputManager.FreeView.Look.performed += ctx => _mouseLook = ctx.ReadValue<Vector2>();

        _inputManager.FreeView.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();

        _inputManager.FreeView.Vertical.performed += ctx => _verticalMovement = ctx.ReadValue<float>();
    }

    void OnEnable() 
    {
        _inputManager.Enable();
        
    }

    void OnDisable()
    {
        _inputManager.Disable();   
    }

    void Update()
    {
        Move();
        Look();
    }

    void StartSprint()
    {
        _speed *= _speedBonus;
    }

    void StopSprint()
    {
        _speed /= _speedBonus;
    }

    void Move()
    {
        var movement = _moveInput * _speed * Time.deltaTime;
        var vMovement = _verticalMovement * _speed * Time.deltaTime;

        transform.Translate(new Vector3(movement.x, 0, movement.y), Space.Self);
        transform.Translate(new Vector3(0, vMovement, 0), Space.World);
    }

    void Look()
    {
        _rotation.x += _mouseLook.x * _sensibilityX * Time.deltaTime;
        _rotation.y -= _mouseLook.y * _sensibilityY * Time.deltaTime;
        _rotation.y = Mathf.Clamp(_rotation.y, -90f, 90f);

        transform.localRotation = Quaternion.Euler(_rotation.y, _rotation.x, 0f);
    }
}
