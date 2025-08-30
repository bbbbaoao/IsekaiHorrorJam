using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueUI;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public AudioSource audioSource;

    private DialogueLine[] currentLines;
    private int currentIndex;
    private bool isDialoguePlaying = false;
    private bool canAdvance = false;

    public static DialogueManager Instance;

    //Dialogue Typewriter effect
    public float textSpeed = 0.02f; // adjust this to change typing speed
    public bool isTyping;
    private Coroutine typingCoroutine;


    public DialogueData YokaiIntro;

    private void Awake()
    {
        Instance = this;
        DialogueManager.Instance.StartDialogue(YokaiIntro);
    }

    public void StartDialogue(DialogueData dialogueData)
    {
        dialogueUI.SetActive(true);
        speakerText.text = dialogueData.speakerName;
        currentLines = dialogueData.lines;
        currentIndex = 0;
        isDialoguePlaying = true;
        StartCoroutine(ShowLine());
    }


    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        canAdvance = true;
    }

    private IEnumerator ShowLine()
    {
        canAdvance = false;

        DialogueLine line = currentLines[currentIndex];

        // Play voiceline if available
        if (line.voiceLine != null)
        {
            audioSource.clip = line.voiceLine;
            audioSource.Play();
        }

        // Start typewriter effect
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(line.text));

        yield return null;
    }

    private void Update()
    {
        if (!isDialoguePlaying || !Input.GetMouseButtonDown(0)) return;

        if (isTyping)
        {
            // Finish the current line immediately
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLines[currentIndex].text;
            isTyping = false;
            canAdvance = true;
        }
        else if (canAdvance)
        {
            currentIndex++;

            if (currentIndex < currentLines.Length)
            {
                StartCoroutine(ShowLine());
            }
            else
            {
                EndDialogue();
            }
        }
    }

    void EndDialogue()
    {
        isDialoguePlaying = false;
        dialogueUI.SetActive(false);
    }
}