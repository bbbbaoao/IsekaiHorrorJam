using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip[] musicClips;        // Main music tracks
    public AudioClip[] transitionClips;   // Optional transition sounds (same index as musicClips)

    [Header("Settings")]
    public float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Plays a music clip directly with fade, no transition.
    /// </summary>
    public void PlayDirect(int musicIndex)
    {
        if (!IsValidIndex(musicIndex, musicClips)) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToClip(musicClips[musicIndex]));
    }

    /// <summary>
    /// Plays a music clip with a transition clip before it.
    /// </summary>
    public void PlayWithTransition(int musicIndex)
    {
        if (!IsValidIndex(musicIndex, musicClips)) return;

        AudioClip musicClip = musicClips[musicIndex];
        AudioClip transitionClip = IsValidIndex(musicIndex, transitionClips) ? transitionClips[musicIndex] : null;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToTransitionThenClip(transitionClip, musicClip));
    }

    /// <summary>
    /// Fades out current music and plays the new music directly.
    /// </summary>
    private IEnumerator FadeToClip(AudioClip newClip)
    {
        yield return FadeOut();

        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();

        yield return FadeIn();
    }

    /// <summary>
    /// Fades out current music, plays a transition clip, then plays the new music.
    /// </summary>
    private IEnumerator FadeToTransitionThenClip(AudioClip transitionClip, AudioClip nextClip)
    {
        yield return FadeOut();

        if (transitionClip != null)
        {
            audioSource.clip = transitionClip;
            audioSource.loop = false;
            audioSource.volume = 1f;
            audioSource.Play();

            yield return new WaitForSeconds(transitionClip.length);
        }

        audioSource.clip = nextClip;
        audioSource.loop = true;
        audioSource.Play();

        yield return FadeIn();
    }

    /// <summary>
    /// Fade out audio volume to 0.
    /// </summary>
    private IEnumerator FadeOut()
    {
        float startVol = audioSource.volume;
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
    }

    /// <summary>
    /// Fade in audio volume to 1.
    /// </summary>
    private IEnumerator FadeIn()
    {
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        audioSource.volume = 1f;
    }

    /// <summary>
    /// Checks if index is valid for a clip array.
    /// </summary>
    private bool IsValidIndex(int index, AudioClip[] array)
    {
        if (index < 0 || index >= array.Length || array[index] == null)
        {
            Debug.LogWarning($"AudioManager: Invalid clip index [{index}] or null clip.");
            return false;
        }
        return true;
    }
}