using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class ProloguePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "GameScene";
    public Slider skipSlider;
    public float holdTime = 2f; // Time in seconds to fill the slider

    //skipAnimation
    public CanvasGroup skipUIGroup;
    public float fadeInDelay = 5f;
    public float fadeDuration = 1f;
    /// </summary>
    /// 

    private float holdTimer = 0f;
    private bool skipping = false;
    private bool isFading = false;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();

        // Start fade-in coroutine
        skipUIGroup.alpha = 0f;
        skipSlider.value = 0f;
        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        yield return new WaitForSeconds(fadeInDelay);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            skipUIGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        skipUIGroup.alpha = 1f;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            holdTimer += Time.deltaTime;
            skipSlider.value = holdTimer / holdTime;

            if (holdTimer >= holdTime && !skipping)
            {
                skipping = true;
                SceneManager.LoadScene(nextSceneName);
            }
        }
        else
        {
            // Reset slider if player releases the key
            holdTimer -= Time.deltaTime * 2f; // Optional: fades out instead of instant reset
            holdTimer = Mathf.Clamp(holdTimer, 0f, holdTime);
            skipSlider.value = holdTimer / holdTime;
        }
    }

        void OnVideoFinished(VideoPlayer vp)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
