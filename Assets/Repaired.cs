using UnityEngine;

public class Repaired : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    
    [Tooltip("Drag the Health script component from this same object into here!")]
    public Health healthScript;

    [Header("Settings")]
    [Tooltip("The minimum health required for the door to be considered 'Fixed'.")]
    public float repairThreshold = 50f;

    private bool isFixed = false;

    void Start()
    {
        // Auto-find references if you forget to drag them in
        if (animator == null) animator = GetComponent<Animator>();
        if (healthScript == null) healthScript = GetComponent<Health>();
    }

    void Update()
    {
        if (healthScript == null || animator == null) return;

        // Check if the current health is at or above the repair threshold
        bool shouldBeFixed = healthScript.currentHealth >= repairThreshold;

        // Only update the Animator if the state actually changed (prevents spamming the Animator)
        if (shouldBeFixed != isFixed)
        {
            isFixed = shouldBeFixed;
            
            // This is the exact name of the Bool in your Animator!
            animator.SetBool("Door Fixed - Opens", isFixed);

            if (isFixed)
            {
                Debug.Log("🟢 DOOR FIXED! Bool set to TRUE.");
            }
            else
            {
                Debug.Log("🔴 DOOR BROKEN! Bool set to FALSE.");
            }
        }
    }
}