using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainmenu : MonoBehaviour
{
    [SerializeField] private int gamescene;
    public GameObject pausePanel;
    private bool isPaused = false;
    public GameObject SkillPanel;
    public AudioMixer audioMixer; // Drag your mixer into this in the Inspector
    public Slider VolumeSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        SceneManager.LoadScene(gamescene);

    }

    // Update is called once per frame
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is quit, is end, is exit yk what i mean homie? its all over you quit the game and shii");
    }

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

            float currentDb;
        if (audioMixer.GetFloat("MasterVolume", out currentDb))
        {
            // Convert dB back to linear (slider value)
            float linear = Mathf.Pow(10f, currentDb / 20f);
            VolumeSlider.SetValueWithoutNotify(linear);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && pausePanel != null)
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.Tab) && SkillPanel != null)
        {
            ToggleSkillPanel();
        }
    }

    public void TogglePause()

    {
        if (pausePanel != null) { return; }
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    //VolumeSlider:
    public void SetVolume(float volume)
    {
        // Converts linear slider (0.0001 to 1.0) to decibels (-80 to 0)

        float clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
        float dB = Mathf.Log10(clampedVolume) * 20f;
        //float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);
        //PlayerPrefs.SetFloat("MasterVolume", volume);
        //PlayerPrefs.Save();
    }

    // Call this from your UI buttons
    public void SetGraphicsQuality(int level)
    {
        // 0 = Low, 1 = Medium, 2 = High, etc. (based on order in Project Settings > Quality)
        QualitySettings.SetQualityLevel(level, true);
        Debug.Log("Graphics quality set to level: " + level);
    }

    public void ToggleGameobject(GameObject poopy)
    {
        if (poopy != null)
        {
            poopy.SetActive(!poopy.activeSelf);
        }
        }

    public void ToggleSkillPanel()

    {
        isPaused = !isPaused;
        SkillPanel.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}
