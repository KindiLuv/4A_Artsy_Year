using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{

    #region Variables

    [SerializeField] private float _playerspeed = 5f;
    public float PlayerSpeed => _playerspeed;

    [SerializeField] private float _gravityValue = -9.81f;
    [SerializeField] private float _controllerDeadzone = .1f;
    [SerializeField] private float _gamepadRotateSmoothing = 1000f;

    [SerializeField] private bool _isGamepad; //Mostly a visualizer

    private CharacterController _controller;

    private Vector2 _movement;
    private Vector2 _aim;

    private Vector3 _playerVelocity;

    private float _dashValue = 1f;

    private bool _actionLocked;
    private bool _dashLocked;

    private PlayerControls _playerControls;
    private PlayerInput _playerInput;
    [SerializeField] private TrailRenderer _tr = null;
    [SerializeField] private PlayerInteract _playerInteract = null;
    [SerializeField] private GameObject _playerCamera = null;

    #endregion

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerControls = new PlayerControls();
        _playerInput = GetComponent<PlayerInput>();
        //_tr = GetComponent<TrailRenderer>();

    }

    private void OnEnable()
    {
        _playerControls.Enable();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            Destroy(_playerCamera);
            Destroy(_playerInput);
            enabled = false;
            return;
        }
        _playerControls.controls.Dash.performed += HandleDash;
        _playerControls.controls.BasicAttack.performed += HandleBasicAttack;
        _playerControls.controls.Spell1.performed += HandleSpell1;
        _playerControls.controls.Spell2.performed += HandleSpell2;
        _playerControls.controls.Ultimate.performed += HandleUltimate;
        _playerControls.controls.Interact.performed += HandleInteract;
        _playerControls.controls.OpenMenu.performed += HandleOpenMenu;
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    private void Update()
    {        
        if (_actionLocked) return;
        HandleMovement();
        HandleInputAim();
        HandleRotation();
        if (_dashLocked) return;
        OnDeviceChange(_playerInput);
        HandleInputMovement();

    }

    void HandleInputMovement()
    {
        _movement = _playerControls.controls.Movement.ReadValue<Vector2>();
    }

    void HandleInputAim()
    {
        _aim = _playerControls.controls.Aim.ReadValue<Vector2>();
    }
    void HandleMovement()
    {
        Vector3 move = new Vector3(_movement.x, 0, _movement.y);
        _controller.Move(move * (Time.deltaTime * _playerspeed * _dashValue));

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

    private void HandleDash(InputAction.CallbackContext obj)
    {
        StartCoroutine(DashAction());
    }

    private void HandleBasicAttack(InputAction.CallbackContext obj)
    {
        Debug.Log("basic attack");
        _playerInteract.Interact();
    }

    private void HandleSpell1(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void HandleSpell2(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void HandleUltimate(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void HandleInteract(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void HandleOpenMenu(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    IEnumerator DashAction()
    {
        if (_dashLocked) yield break;
        _dashLocked = true;
        _tr.emitting = true;
        _dashValue = 3f;
        yield return new WaitForSeconds(0.3f);
        _dashValue = 0.7f;
        _tr.emitting = false;
        yield return new WaitForSeconds(0.2f);
        _dashValue = 1f;
        _dashLocked = false;
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

