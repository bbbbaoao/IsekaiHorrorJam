using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator[] _childAnimators;

    private void Awake()
    {
        _childAnimators = GetComponentsInChildren<Animator>();
    }

    public void ToggleDoor()
    {
        foreach (Animator anim in _childAnimators)
        {
            bool isOpen = anim.GetBool("DoorOpen");
            anim.SetBool("DoorOpen", !isOpen);
        }
    }
}