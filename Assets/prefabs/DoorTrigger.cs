using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The unique ID for this door. Must match the 'Required Door ID' in the DialogueManager.")]
    public int doorID = 1;
    
    [Header("References")]
    [Tooltip("Drag the object that has the DialogueManager script here.")]
    public DialogueManager dialogueManager;

    private bool hasBeenTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure the thing walking through is the Player
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            if (dialogueManager != null)
            {
                // Tell the dialogue system THIS specific door was passed!
                dialogueManager.OnDoorPassed(doorID);
                hasBeenTriggered = true; // Prevent it from triggering again
                Debug.Log($"🚪 Door {doorID} trigger activated!");
            }
        }
    }
}