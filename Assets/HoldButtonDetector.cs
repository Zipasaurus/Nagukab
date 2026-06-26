using UnityEngine;

public class HoldButtonDetector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How long an object must stay inside the collider to activate.")]
    public float holdTime = 0.5f;

    [Tooltip("Drag the Door GameObject here. It should have an Animator with a 'Door is unlocked' parameter.")]
    public GameObject doorTarget;

    [Tooltip("Leave empty to allow ANY object. Set to restrict (e.g., 'Player' or 'Weight').")]
    public string requiredTag = "";

    [Header("Button Animation")]
    [Tooltip("The Animator for the button itself. Leave empty to auto-find on this object.")]
    public Animator buttonAnimator;

    [Header("State (Read Only)")]
    public bool ObjectOnButton;

    private int objectsInsideCount = 0;
    private float entryTime = 0f;
    private bool isActivated = false;
    private Animator doorAnimator;

    void Awake()
    {
        // Find the Animator on the door target
        if (doorTarget != null)
            doorAnimator = doorTarget.GetComponent<Animator>();
            
        // --- NEW: Auto-find the button's own Animator ---
        if (buttonAnimator == null)
            buttonAnimator = GetComponent<Animator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        if (objectsInsideCount == 0)
        {
            entryTime = Time.time;
        }
        objectsInsideCount++;
    }

    void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        objectsInsideCount--;
        if (objectsInsideCount < 0) objectsInsideCount = 0;

        if (objectsInsideCount == 0)
        {
            isActivated = false;
            ObjectOnButton = false;

            // Tell the door to close
            if (doorAnimator != null)
                doorAnimator.SetBool("Door is unlocked", false);
                
            // --- NEW: Tell the button to pop back up ---
            if (buttonAnimator != null)
                buttonAnimator.SetBool("Pressed", false);
        }
    }

    void Update()
    {
        if (objectsInsideCount > 0 && !isActivated)
        {
            if (Time.time - entryTime >= holdTime)
            {
                isActivated = true;
                ObjectOnButton = true;

                // Tell the door to open
                if (doorAnimator != null)
                    doorAnimator.SetBool("Door is unlocked", true);
                    
                // --- NEW: Tell the button to press down ---
                if (buttonAnimator != null)
                    buttonAnimator.SetBool("Pressed", true);
            }
        }
    }
}