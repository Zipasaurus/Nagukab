using UnityEngine;

public class DialogueZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [Tooltip("The unique ID for this zone. Must match the 'Required Zone ID' in the DialogueManager array.")]
    public int zoneID = 1;
    
    [Tooltip("Should this zone only trigger once?")]
    public bool triggerOnce = true;
    
    [Tooltip("What tag is required to trigger this zone?")]
    public string requiredTag = "Player";

    [Header("References")]
    [Tooltip("Drag the DialogueManager object here.")]
    public DialogueManager dialogueManager;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the correct tag entered
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
        
        // Check if it already triggered
        if (triggerOnce && hasTriggered) return;

        if (dialogueManager != null)
        {
            // THIS IS THE KEY: Tell the DialogueManager this zone ID has been entered.
            // This unlocks Elements 4 and 5!
            dialogueManager.OnZoneEntered(zoneID);
            hasTriggered = true;
            Debug.Log($"📍 Zone ID {zoneID} entered! Dialogue is now unlocked to proceed.");
        }
    }
}