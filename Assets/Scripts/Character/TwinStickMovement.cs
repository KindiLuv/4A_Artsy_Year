using System;
using UnityEditor.Build;
using UnityEditor.Hardware;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TwinStickMovement : MonoBehaviour
{
    #region Variables

    [SerializeField] private float _playerspeed = 5f;
    public float PlayerSpeed => _playerspeed;

    [SerializeField] private float _gravityValue = -9.81f;
    [SerializeField] private float _controllerDeadzone = .1f;
    [SerializeField] private float _gamepadRotateSmoothing =  1000f;

    [SerializeField] private bool _isGamepad; //Mostly a visualizer

    private CharacterController _controller;

    private Vector2 _movement;
    private Vector2 _aim;

    private Vector3 _playerVelocity;

    private PlayerControls _playerControls;
    private PlayerInput _playerInput;

    #endregion

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerControls = new PlayerControls();
        _playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    private void Update()
    {
        OnDeviceChange(_playerInput);
        HandleInput();
        HandleMovement();
        HandleRotation();
    }

    void HandleInput()
    {
        _movement = _playerControls.controls.Movement.ReadValue<Vector2>();
        _aim = _playerControls.controls.Aim.ReadValue<Vector2>();
    }
    void HandleMovement()
    {
        Vector3 move = new Vector3(_movement.x, 0, _movement.y);
        _controller.Move(move * (Time.deltaTime * _playerspeed));

        _playerVelocity.y += _gravityValue * Time.deltaTime;
        _controller.Move(_playerVelocity * Time.deltaTime);
    }
    void HandleRotation()
    {
        if (_isGamepad)
        {
            if (Mathf.Abs(_aim.x) > _controllerDeadzone || Mathf.Abs(_aim.y) > _controllerDeadzone)
            {
                Vector3 playerDirection = Vector3.right * _aim.x + Vector3.forward * _aim.y;
                if (playerDirection.sqrMagnitude > .0f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation,
                        _gamepadRotateSmoothing * Time.deltaTime);
                }
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(_aim);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                LookAt(point);
            }
        }
    }

    void LookAt(Vector3 target)
    {
        Vector3 heightCorrectedPoint = new Vector3(target.x, transform.position.y, target.z);
        transform.LookAt(heightCorrectedPoint);
    }
    
    public void OnDeviceChange(PlayerInput pi)
    {
        _isGamepad = pi.currentControlScheme.Equals("Controller");
    }
}