using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    // initial cursor state
    [SerializeField] private CursorLockMode _cursorMode = CursorLockMode.None;
    // make character look in Camera direction instead of MoveDirection
    [SerializeField] private bool _lookInCameraDirection;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _rotationSmoothTime = 0.05f; // Smoothing factor

    private float _xRotation = 0f;
    private float _targetYaw;
    private float _targetPitch;
    private float _currentYaw;
    private float _currentPitch;
    private float _yawVelocity;
    private float _pitchVelocity;
    private Camera _camera;
    Vector2 _mousePos;
    Transform _projectileSpawn;
    Vector3 moveInput;
    private CharacterMovement _characterMovement;
    private Vector2 _moveInput;
    private Vector2 _rangeInput;
    private Vector2 aimDirection;
    public bool Reversed;
    private Vector2 _lookInput;
    public PlayerInput _input;

    private void Awake()
    {
        _characterMovement = GetComponent<CharacterMovement>();

        if (_characterMovement == null) { Debug.LogError("Add a Character Movement script to a character in the scene."); return; }

        _camera = Camera.main;
        //_input = GetComponent<PlayerInput>();
        // Lock State:
        _input = new PlayerInput();
        _input.currentActionMap.Enable();
        // None = Free to move around. This is what we want during gameplay.
        // Locked = Locked to center of screen.
        Cursor.lockState = _cursorMode;
        _input.currentActionMap.OnMove.performed += ctx => OnMove(ctx);
        _input.currentActionMap.OnMove.canceled += ctx => OnMove(ctx);
        _input.currentActionMap.OnJump.performed += ctx => OnJump(ctx);
        _input.currentActionMap.OnDash.performed += ctx => OnDash(ctx);
        _input.currentActionMap.Look.performed += ctx => OnLook(ctx);
        // Initialize rotation targets
        _targetYaw = transform.eulerAngles.y;
        _targetPitch = 0f;
        _currentYaw = _targetYaw;
        _currentPitch = _targetPitch;
    }

    private void Start()
    {
        // GlobalObjects.Player = this;
    }

    private void Update()
    {
        // Find correct right/forward directions based on main camera rotation
        
        Vector3 up =  Vector3.up;
        Vector3 right = _camera.transform.right;
        Vector3 forward = Vector3.Cross(right, up);
        moveInput =  forward *Mathf.Clamp ( _moveInput.y,-45,45) + right *-Mathf.Clamp( _moveInput.x,-45,45);
        // Send player input to character movement
        _characterMovement.SetMoveInput(moveInput);
    
        _input.currentActionMap.Sprint.performed += ctx => _characterMovement.IsRunning = true;
        _input.currentActionMap.Sprint.canceled += ctx => _characterMovement.IsRunning = false;
        // No, you do not spawn projectiles here.
        // Call animations, which then call the projectile.
        // And use the new input system code as below for move and jump.

        //if (Input.GetMouseButtonDown(0))
        //{
        //    _projectileSpawn = transform.Find("ProjectileSpawn"); 
        //    Spell.Fire(_projectileSpawn);
        //}
    }
    private void CameraLook()
    {
        // Smoothly interpolate yaw and pitch
        _targetYaw += _lookInput.x * _mouseSensitivity;
        _targetPitch -= _lookInput.y * _mouseSensitivity;
        _targetPitch = Mathf.Clamp(_targetPitch, -90f, 90f);


        float lerpSpeed = 1f / _rotationSmoothTime * Time.deltaTime; // Higher _rotationSmoothTime = slower smoothing
        _currentYaw = Mathf.LerpAngle(_currentYaw, _targetYaw, lerpSpeed);
        _currentPitch = Mathf.Lerp(_currentPitch, _targetPitch, lerpSpeed);
        // Apply rotation
        transform.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
        _camera.transform.localRotation = Quaternion.Euler(_currentPitch, 0f, 0f);

        // Choose a distance in front of the camera, e.g., 10 units
        float distance = 10f;
        Vector3 worldPoint = _camera.transform.position + _camera.transform.forward * distance;
        _mousePos = new Vector2(worldPoint.x, worldPoint.y); // or use worldPoint directly if needed

        //Debug.DrawLine(transform.position, _mousePos, Color.blue);
        _characterMovement.SetBodyDirection(moveInput);

        //_characterMovement.SetLookDirectionToCursor(); // TODO: May no longer be needed if we are using WASD.

        if (_lookInCameraDirection)
        {
            _characterMovement.SetBodyDirection(_camera.transform.forward);
        }
       
       
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _characterMovement.Jump();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        //if (_characterMovement.Stamina > 30f)
        //    _characterAnimations.Dash();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
        if (_lookInput != Vector2.zero)
        {
            CameraLook();

        }
    }
}
