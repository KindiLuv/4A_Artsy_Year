using Assets.Scripts.NetCode;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.VFX;
using Utilities;
using System.Linq;

public class PlayerController : Character
{

    #region Variables

    [SerializeField] private float _playerspeed = 5f;
    public float PlayerSpeed => _playerspeed;

    [SerializeField] private float _gravityValue = -9.81f;
    [SerializeField] private float _controllerDeadzone = .1f;
    [SerializeField] private float _gamepadRotateSmoothing = 1000f;

    [SerializeField] private bool _isGamepad; //Mostly a visualizer

    private CharacterController _controller;
    private Player _player;

    private Vector2 _movement;
    private Vector2 _aim;

    private Vector3 _playerVelocity;

    private float _dashValue = 1f;

    private bool _dashLocked;
    private bool _onAttackHandel = false;
    private float _attackRate = 0.0f;
    private Vector3 _impulseForce = Vector3.zero;
    private float clampImpulseSpeed = 25;
    private float timeToZeroImpulse = 20;

    private PlayerControls _playerControls;
    [SerializeField] private VisualEffect _tr = null;
    [SerializeField] private PlayerInteract _playerInteract = null;
    [SerializeField] private GameObject _playerCamera = null;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        team = Team.Player;
        _controller = GetComponent<CharacterController>();
        _playerControls = InputManager.PlayerInput;
        _player = GetComponent<Player>();
    }

    protected void Start()
    {
        if (GameNetworkManager.IsOffline || IsLocalPlayer)
        {
            _playerControls.controls.Dash.performed += HandleDash;
            _playerControls.controls.BasicAttack.started += HandleBasicAttack;
            _playerControls.controls.BasicAttack.canceled += HandleCancelBasicAttack;
            _playerControls.controls.Spell1.performed += HandleSpell1;
            _playerControls.controls.Spell2.performed += HandleSpell2;
            _playerControls.controls.Ultimate.performed += HandleUltimate;
            _playerControls.controls.Interact.performed += HandleInteract;
            _playerControls.controls.OpenMenu.performed += HandleOpenMenu;
        }
    }

    private void OnDestroy()
    {

        _playerControls.controls.Dash.performed -= HandleDash;
        _playerControls.controls.BasicAttack.started -= HandleBasicAttack;
        _playerControls.controls.BasicAttack.canceled -= HandleCancelBasicAttack;
        _playerControls.controls.Spell1.performed -= HandleSpell1;
        _playerControls.controls.Spell2.performed -= HandleSpell2;
        _playerControls.controls.Ultimate.performed -= HandleUltimate;
        _playerControls.controls.Interact.performed -= HandleInteract;
        _playerControls.controls.OpenMenu.performed -= HandleOpenMenu;
    }

    public override void Teleportation(Vector3 positionTarget)
    {
        _controller.enabled = false;
        transform.position = positionTarget;
        _controller.enabled = true;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsLocalPlayer)
        {
            Destroy(_playerCamera);
            return;
        }
    }

    private void Update()
    {
        if (GameNetworkManager.IsOffline || IsServer)
        {
            HealIndicator();
        }
        if (_actionLocked || (!GameNetworkManager.IsOffline && !IsLocalPlayer)) return;
        HandleMovement();
        HandleInputAim();
        HandleRotation();
        HandleAttack();
        if (_dashLocked) return;
        //OnDeviceChange(_playerInput);
        HandleInputMovement();
    }

    void HandleAttack()
    {
        _attackRate -= Time.deltaTime;
        if (_impulseForce.x > 0.5f) { _impulseForce.x -= Time.deltaTime * timeToZeroImpulse; }

        if (_impulseForce.z > 0.5f) { _impulseForce.z -= Time.deltaTime * timeToZeroImpulse; }

        if (_impulseForce.x < -0.5f) { _impulseForce.x += Time.deltaTime * timeToZeroImpulse; }

        if (_impulseForce.z < -0.5f) { _impulseForce.z += Time.deltaTime * timeToZeroImpulse; }

        if (_impulseForce.magnitude < 0.8f) { _impulseForce = Vector3.zero; }

        if (_onAttackHandel && _attackRate <= 0.0f)
        {
            if ((GameNetworkManager.IsOffline || IsLocalPlayer) && _player.HasWeapon())
            {
                _player.BasicAttack(transform.position, transform.rotation, (float)NetworkManager.Singleton.LocalTime.Time);
                _impulseForce += transform.forward * _player.Weapon.impulseForce;
                _impulseForce.x = Mathf.Clamp(_impulseForce.x, -clampImpulseSpeed, clampImpulseSpeed);
                _impulseForce.y = Mathf.Clamp(_impulseForce.y, -clampImpulseSpeed, clampImpulseSpeed);
                _impulseForce.z = Mathf.Clamp(_impulseForce.z, -clampImpulseSpeed, clampImpulseSpeed);
                _attackRate = _player.Weapon.spawnProjectileRate;
                if (!_player.Weapon.autoWeapon)
                {
                    _onAttackHandel = false;
                }
            }
        }
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
        _controller.Move(move * (Time.deltaTime * _playerspeed * _dashValue) + (_impulseForce * Time.deltaTime));

        _playerVelocity.y = _gravityValue;
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
        if(_movement.magnitude < 0.1f || !_controller.isGrounded)
        {
            return;
        }
        if(GameNetworkManager.IsOffline || IsLocalPlayer)
        {
            DashServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        StartCoroutine(DashAction());
    }

    private void HandleBasicAttack(InputAction.CallbackContext obj)
    {
        _onAttackHandel = true;
    }

    private void HandleCancelBasicAttack(InputAction.CallbackContext obj)
    {
        _onAttackHandel = false;
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
        _playerInteract.Interact();
    }

    private void HandleOpenMenu(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    [ServerRpc]
    public void DashServerRpc(ulong localClientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(x => x != localClientId).ToList() }
        };
        DashClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void DashClientRpc(ClientRpcParams clientRpcParams = default)
    {
        StartCoroutine(DashAction());
    }

    IEnumerator DashAction()
    {
        if (_dashLocked) yield break;
        _dashLocked = true;
        _tr.SetFloat("ParticlesRate", 256.0f);
        _dashValue = 3f;
        _gravityValue = 0f;
        yield return new WaitForSeconds(0.3f);
        _dashValue = 0.7f;
        _gravityValue = -9.81f;
        _tr.SetFloat("ParticlesRate", 0.0f);
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

