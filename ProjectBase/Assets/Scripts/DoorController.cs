using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DoorController : MonoBehaviour
{
    private Animator[] _childAnimators;
    private AudioSource _audioSource;

    [Header("Lock Settings")]
    public bool isLocked = false;
    public float unlockDuration = 3f;

    private float unlockProgress = 0f;
    private bool isUnlocking = false;
    private bool isPlayerHolding = false;

    [Header("UI")]
    public GameObject doorUI; // assign or auto-find
    public Image[] radialProgressImages; // assign both images in inspector
    public GameObject CurseVfx;

    private void Awake()
    {
        _childAnimators = GetComponentsInChildren<Animator>();
        Debug.Log("Found animators: " + _childAnimators.Length);
        _audioSource = GetComponent<AudioSource>();

        if (doorUI == null)
            doorUI = transform.Find("DoorUI")?.gameObject;

        if (doorUI != null)
        {
            radialProgressImages = doorUI.GetComponentsInChildren<Image>();
            doorUI.SetActive(false);
        }

        if (isLocked) { if (CurseVfx != null) { CurseVfx.SetActive(true); } }
    }

    public void ToggleDoor()
    {
        if (isLocked)
        {
            if (!isUnlocking)
                StartCoroutine(UnlockDoorCoroutine());

            return;
        }

        Debug.Log("Door Toggled");
        foreach (Animator anim in _childAnimators)
        {
            bool isOpen = anim.GetBool("DoorOpen");
            anim.SetBool("DoorOpen", !isOpen);
            CurseVfx.SetActive(false );
        }

        _audioSource?.Play();
    }

    private IEnumerator UnlockDoorCoroutine()
    {
        isUnlocking = true;
        unlockProgress = 0f;
        isPlayerHolding = true;

        if (doorUI != null) doorUI.SetActive(true);

        while (unlockProgress < unlockDuration)
        {
            // Wait until player is holding the key
            while (!isPlayerHolding)
            {
                yield return null;
            }

            unlockProgress += Time.deltaTime;

            float percent = Mathf.Clamp01(unlockProgress / unlockDuration);
            foreach (var img in radialProgressImages)
            {
                if (img != null) img.fillAmount = percent;
            }

            yield return null;
        }

        isLocked = false;
        isUnlocking = false;
        if (doorUI != null) doorUI.SetActive(false);

        Debug.Log("Door unlocked!");
        ToggleDoor(); // auto-open after unlocking
    }

    // Call these from DoorManager or InputSystem callbacks
    public void BeginHoldUnlock()
    {
        isPlayerHolding = true;

        if (!isUnlocking)
            StartCoroutine(UnlockDoorCoroutine());
    }

    public void EndHoldUnlock()
    {
        isPlayerHolding = false;
    }
}