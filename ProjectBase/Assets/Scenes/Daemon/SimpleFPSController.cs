using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float lookSensitivity = 1f;
    public Transform cameraTarget; // The object being rotated for mouse look
    public Transform headbobHolder; // The object that gets headbob applied

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private PlayerInputActions inputActions;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch = 0f;

    [Header("Headbob")]
    public float bobFrequency = 10f;
    public float bobHeight = 0.05f;
    public float bobSideMovement = 0.03f;
    public float bobRotationAmount = 1.5f;

    private float bobTimer = 0f;
    private Vector3 originalHeadbobLocalPos;
    private Quaternion originalHeadbobLocalRot;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        originalHeadbobLocalPos = headbobHolder.localPosition;
        originalHeadbobLocalRot = headbobHolder.localRotation;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => TryJump();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleHeadbob();
    }

    void HandleLook()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleHeadbob()
    {
        Vector3 bobOffset = Vector3.zero;
        Quaternion bobTilt = Quaternion.identity;

        bool isMoving = controller.isGrounded && moveInput.magnitude > 0.1f;

        if (isMoving)
        {
            bobTimer += Time.deltaTime * bobFrequency;

            float bobX = Mathf.Sin(bobTimer) * bobSideMovement;
            float bobY = Mathf.Cos(bobTimer * 2f) * bobHeight;
            float bobRot = Mathf.Sin(bobTimer) * bobRotationAmount;

            bobOffset = new Vector3(bobX, bobY, 0f);
            bobTilt = Quaternion.Euler(bobRot, 0f, 0f);
        }
        else
        {
            bobTimer = 0f;
        }

        Vector3 targetPos = originalHeadbobLocalPos + bobOffset;
        Quaternion targetRot = originalHeadbobLocalRot * bobTilt;

        headbobHolder.localPosition = Vector3.Lerp(headbobHolder.localPosition, targetPos, Time.deltaTime * 10f);
        headbobHolder.localRotation = Quaternion.Slerp(headbobHolder.localRotation, targetRot, Time.deltaTime * 10f);
    }

    void TryJump()
    {
        if (isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
    }
}
