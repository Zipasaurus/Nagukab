using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag your TextMeshPro UI text object here.")]
    public TextMeshProUGUI dialogueText;

    [Header("Dialogue Settings")]
    [Tooltip("The lines of text to display.")]
    public string[] dialogues;
    
    [Tooltip("0 = Show immediately. 1, 2, 3, etc. = Wait until that specific Door ID is passed.")]
    public int[] requiredDoorID;
    
    [Tooltip("0 = Show immediately. 1, 2, 3, etc. = Wait until that specific Zone ID is entered.")]
    public int[] requiredZoneID;
    
    [Tooltip("Seconds to wait before auto-advancing. 0 = Waits for Spacebar.")]
    public float[] waitTimes;

    [Header("Typewriter Settings")]
    [Tooltip("How fast the text writes out (seconds between each letter).")]
    public float typingSpeed = 0.05f;
    public AudioSource typingAudio;
    public AudioClip typingSound;

    private int currentIndex = 0;
    private bool isWaitingForInput = false;
    private bool isWaitingForDoor = false;
    private bool isWaitingForZone = false;

    // Track which doors and zones have been triggered
    private Dictionary<int, bool> doorsPassed = new Dictionary<int, bool>();
    private Dictionary<int, bool> zonesTriggered = new Dictionary<int, bool>();
    
    // Typewriter variables
    private bool isTyping = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;
    private Coroutine waitCoroutine;

    void Start()
    {
        if (dialogueText == null) { Debug.LogError("DialogueManager: Missing TextMeshProUGUI!"); return; }
        if (dialogues.Length == 0) { Debug.LogWarning("DialogueManager: No dialogues assigned!"); return; }

        // Ensure all arrays match the dialogue count
        if (requiredDoorID.Length != dialogues.Length) System.Array.Resize(ref requiredDoorID, dialogues.Length);
        if (requiredZoneID.Length != dialogues.Length) System.Array.Resize(ref requiredZoneID, dialogues.Length);
        if (waitTimes.Length != dialogues.Length) System.Array.Resize(ref waitTimes, dialogues.Length);

        if (typingAudio == null) typingAudio = GetComponent<AudioSource>();
        ShowDialogue(currentIndex);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping) { SkipTyping(); }
            else if (isWaitingForInput) { AdvanceDialogue(); }
        }
    }

    void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = currentFullText;
        isTyping = false;
        isWaitingForInput = true;
        StartWaitTimerIfNeeded();
    }

    void AdvanceDialogue()
    {
        if (waitCoroutine != null) { StopCoroutine(waitCoroutine); waitCoroutine = null; }
        currentIndex++;

        if (currentIndex < dialogues.Length)
        {
            int reqDoor = requiredDoorID[currentIndex];
            int reqZone = requiredZoneID[currentIndex];
            
            // Check if this line requires a door we haven't passed yet
            if (reqDoor > 0 && !doorsPassed.ContainsKey(reqDoor))
            {
                isWaitingForInput = false;
                isWaitingForDoor = true;
                dialogueText.text = "";
                Debug.Log($"⏳ Pausing: Waiting for Door ID {reqDoor}...");
            }
            // Check if this line requires a zone we haven't entered yet
            else if (reqZone > 0 && !zonesTriggered.ContainsKey(reqZone))
            {
                isWaitingForInput = false;
                isWaitingForZone = true;
                dialogueText.text = "";
                Debug.Log($"⏳ Pausing: Waiting for Zone ID {reqZone} to be entered...");
            }
            else
            {
                ShowDialogue(currentIndex);
            }
        }
        else
        {
            dialogueText.text = "";
            isWaitingForInput = false;
            Debug.Log("✅ Dialogue sequence finished.");
        }
    }

    void ShowDialogue(int index)
    {
        currentIndex = index;
        currentFullText = dialogues[index];
        dialogueText.text = "";
        isWaitingForInput = false;
        isTyping = true;
        
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(currentFullText));
    }

    IEnumerator TypeText(string fullText)
    {
        foreach (char letter in fullText.ToCharArray())
        {
            dialogueText.text += letter;
            if (typingAudio != null && typingSound != null && !char.IsWhiteSpace(letter))
                typingAudio.PlayOneShot(typingSound);
            
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        isWaitingForInput = true;
        StartWaitTimerIfNeeded();
    }

    void StartWaitTimerIfNeeded()
    {
        if (waitTimes[currentIndex] > 0f)
        {
            if (waitCoroutine != null) StopCoroutine(waitCoroutine);
            waitCoroutine = StartCoroutine(WaitAndAdvance(waitTimes[currentIndex]));
        }
    }

    IEnumerator WaitAndAdvance(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        waitCoroutine = null;
        AdvanceDialogue();
    }

    // ==========================================
    // --- EXTERNAL TRIGGER METHODS ---
    // ==========================================
    
    /// <summary>
    /// Called by DialogueZone when a zone is entered
    /// </summary>
    public void OnZoneEntered(int zoneID)
    {
        zonesTriggered[zoneID] = true;
        Debug.Log($"📍 Zone ID {zoneID} has been entered!");
        
        // If we're currently paused waiting for THIS exact zone, resume now
        if (isWaitingForZone && requiredZoneID[currentIndex] == zoneID)
        {
            isWaitingForZone = false;
            ShowDialogue(currentIndex);
        }
    }

    /// <summary>
    /// Called by DoorTrigger when a specific door is passed.
    /// </summary>
    public void OnDoorPassed(int doorID)
    {
        doorsPassed[doorID] = true;
        Debug.Log($"🚪 Door ID {doorID} marked as passed!");
        
        if (isWaitingForDoor && requiredDoorID[currentIndex] == doorID)
        {
            isWaitingForDoor = false;
            ShowDialogue(currentIndex);
        }
    }
        /// <summary>
    /// Called by DialogueZone to directly play a specific dialogue line
    /// </summary>
    public void PlayDialogueAtIndex(int index)
    {
        if (index < 0 || index >= dialogues.Length)
        {
            Debug.LogError($"Invalid dialogue index {index}!");
            return;
        }
        
        // Cancel any existing wait timer
        if (waitCoroutine != null) { StopCoroutine(waitCoroutine); waitCoroutine = null; }
        
        // Reset waiting states
        isWaitingForDoor = false;
        isWaitingForZone = false;
        
        // Play the dialogue directly!
        ShowDialogue(index);
        Debug.Log($"💬 Directly playing dialogue index {index}: {dialogues[index]}");
    }



}