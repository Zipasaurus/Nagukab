using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Doorunlockaudio : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Animator with the 'Door is unlocked' bool.")]
    public Animator doorAnimator;
    
    [Tooltip("The Health script on this door to check if it's fully repaired.")]
    public Health doorHealth; 

    private AudioSource audioSource;

    void Awake()
    {
        // Auto-find references if left empty
        if (doorAnimator == null) doorAnimator = GetComponentInChildren<Animator>();
        if (doorHealth == null) doorHealth = GetComponent<Health>();

        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        
        // Force these settings via code so it behaves consistently
        if (audioSource != null)
        {
            audioSource.loop = true; 
            audioSource.playOnAwake = false; // Prevents it from playing on game start
        }
    }

    void Update()
    {
        if (doorAnimator == null || audioSource == null) return;

        // ==========================================
        // 1. CHECK IF THE DOOR IS FULLY FIXED
        // ==========================================
        bool isDoorFixed = (doorHealth != null && doorHealth.currentHealth >= doorHealth.maxHealth);

        if (isDoorFixed)
        {
            // The door is repaired! Stop the sound and disable the AudioSource component entirely.
            if (audioSource.enabled)
            {
                audioSource.Stop();
                audioSource.enabled = false; 
            }
            return; // Skip the rest of the update loop
        }
        else
        {
            // If it's broken, make sure the AudioSource is enabled so it CAN play
            if (!audioSource.enabled)
            {
                audioSource.enabled = true;
            }
        }

        // ==========================================
        // 2. PLAY/STOP BASED ON ANIMATOR
        // ==========================================
        bool isUnlocked = doorAnimator.GetBool("Door is unlocked");

        if (isUnlocked)
        {
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
    }
}