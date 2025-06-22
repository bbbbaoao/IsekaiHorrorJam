using UnityEngine;
using UnityEngine.Windows;
using UnityEngine.InputSystem;
public class DoorManager : MonoBehaviour
{
    [Header("Settings")]
   // public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 2f;

    private bool _isNearDoor= false;
    public float forwardOffset = 0.5f;

  //  private PlayerInput _input;
    [SerializeField]  private PlayerController _controller;
    //private DoorController _doorController;

    private void Update()
    {
        //    if (Input.GetKeyUp(interactionKey))
        //    {
        //        //ToggleDoor();
        //        TryInteractWithDoor();
        //    }
    }

    private void Start()
    {
        _controller._input.currentActionMap.door.performed += ctx => TryInteractWithDoor(ctx);
    }


    private void TryInteractWithDoor(InputAction.CallbackContext context)
    {
        // Position the sphere slightly in front of the player
        Vector3 sphereCenter = transform.position + transform.forward * forwardOffset;

        // Find all colliders in the sphere
        Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, interactionRange);

        foreach (Collider hit in hitColliders)
        {
                if (hit.CompareTag("Door"))
            {

                SpiderActive(1);
                Debug.LogError("Interacting with door: " + hit.name);
                hit.GetComponent<DoorController>().ToggleDoor();

            }
        }
    }
    public bool SpiderActive(int isNearDoor)
    {
       if(isNearDoor == 1)
        {
            _isNearDoor = true;
            Debug.Log("Spider is active near the door.");
        }
        //else
        //{
        //    _isNearDoor = false;
        //    Debug.Log("Spider is not active near the door.");
        //}
        return _isNearDoor;
    }

}