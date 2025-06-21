using UnityEngine;
public class DoorManager : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public float interactionRange = 2f;

    private bool _isNearDoor;
    public float forwardOffset = 0.5f;


    //private DoorController _doorController;

    private void Update()
    {
        if (Input.GetKeyUp(interactionKey))
        {
            //ToggleDoor();
            TryInteractWithDoor();
        }
    }

    private void TryInteractWithDoor()
    {
        // Position the sphere slightly in front of the player
        Vector3 sphereCenter = transform.position + transform.forward * forwardOffset;

        // Find all colliders in the sphere
        Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, interactionRange);

        foreach (Collider hit in hitColliders)
        {
                if (hit.CompareTag("Door"))
            {
                hit.GetComponent<DoorController>().ToggleDoor();
            }
        }
    }

}