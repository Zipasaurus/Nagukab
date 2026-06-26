using UnityEngine;

public class DoorHiddenRooms : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Health script on this door to check if it's fully repaired. Leave empty to auto-find.")]
    public Health doorHealth;

    [Header("Hidden Rooms Settings")]
    [Tooltip("Drag all the objects (roofs, walls, etc.) that should be hidden when the door is fixed.")]
    public GameObject[] objectsToHide;

    // Track if the door has ever been fixed
    private bool hasBeenFixed = false;

    void Awake()
    {
        // Auto-find Health if not assigned in the inspector
        if (doorHealth == null)
            doorHealth = GetComponent<Health>();
    }

    void Update()
    {
        // Safety check
        if (doorHealth == null || objectsToHide == null || objectsToHide.Length == 0) return;

        // Check if the door is currently at full health
        bool isDoorFixed = (doorHealth.currentHealth >= doorHealth.maxHealth);

        // If the door has ever been fixed, mark it as permanently fixed
        if (isDoorFixed && !hasBeenFixed)
        {
            hasBeenFixed = true;
            Debug.Log("🚪 Door has been repaired! Hidden room will stay sealed.");
        }

        // LOGIC:
        // If the door has NEVER been fixed, show the roof (broken state)
        // If the door has EVER been fixed, hide the roof permanently (even if damaged again)
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
            {
                obj.SetActive(!hasBeenFixed); // Show only if door has never been fixed
            }
        }
    }
}