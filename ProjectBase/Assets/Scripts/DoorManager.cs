using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DoorManager : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 2f;
    public float forwardOffset = 0.5f;

    private bool _isNearDoor = false;

    private PlayerInputActions _controller;

    private void Awake()
    {
        _controller = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _controller.Player.Enable();
        _controller.Player.Door.performed += TryInteractWithDoor;
    }

    private void OnDisable()
    {
        _controller.Player.Door.performed -= TryInteractWithDoor;
        _controller.Player.Disable();
    }

    private void TryInteractWithDoor(InputAction.CallbackContext context)
    {
        Vector3 sphereCenter = transform.position + transform.forward * forwardOffset;
        Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, interactionRange);

        foreach (Collider hit in hitColliders)
        {

            if (hit.TryGetComponent(out DoorController door))
            {
                SpiderActive(1);
                Debug.Log("Interacting with door: " + hit.name);

                if (door.isLocked)
                {
                    door.BeginHoldUnlock();
                    StartCoroutine(HoldToUnlock(door));
                }
                else
                {
                    door.ToggleDoor();
                }
            }
        }
    }


    public void SpiderActive(int isNearDoor)
    {
        _isNearDoor = (isNearDoor == 1);
        Debug.Log(_isNearDoor ? "Spider is active near the door." : "Spider is not active near the door.");
    }

    public bool GetIsNear()
    {
        return _isNearDoor;
    }

    private IEnumerator HoldToUnlock(DoorController door)
    {
        while (door != null && door.isLocked && Keyboard.current.eKey.isPressed)
        {
            door.BeginHoldUnlock();
            yield return null;
        }

        door.EndHoldUnlock();
    }
}