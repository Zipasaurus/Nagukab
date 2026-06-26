using UnityEngine;

// THIS CLASS NAME MUST MATCH THE FILE NAME EXACTLY
public class Energy : MonoBehaviour
{
    [Header("Energy Settings")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    
    [Header("Regeneration Settings")]
    [Tooltip("How much energy regenerates per second.")]
    public float regenRate = 15f;
    [Tooltip("How long you must wait after using energy before it starts regenerating.")]
    public float regenDelay = 1.0f;

    [Header("Movement Settings")]
    [Tooltip("Energy drained per second while walking.")]
    public float walkDrainPerSecond = 2f;
    [Tooltip("Energy drained per second while sprinting.")]
    public float sprintDrainPerSecond = 8f;
    [Tooltip("If true, player cannot sprint when energy is 0.")]
    public bool preventSprintWhenEmpty = true;
    [Tooltip("If true, player cannot walk at all when energy is 0.")]
    public bool preventWalkWhenEmpty = false;

    [Header("Action Costs")]
    public float dashCost = 20f;
    public float leftAttackCost = 10f;
    public float rightAttackCost = 15f;

    [Header("Death Settings")]
    [Tooltip("If checked, the player will die and respawn when energy reaches 0.")]
    public bool dieWhenEmpty = true;

    private float regenTimer = 0f;
    private bool hasDiedFromEnergy = false; // Prevents triggering respawn every frame

    void Start()
    {
        currentEnergy = maxEnergy;
    }

    void Update()
    {
        // ==========================================
        // --- 1. DEATH CHECK (RAN OUT OF ENERGY) ---
        // ==========================================
        if (dieWhenEmpty && currentEnergy <= 0f && !hasDiedFromEnergy)
        {
            hasDiedFromEnergy = true;
            Debug.Log("🔋 Player ran out of energy! Respawning...");
            
            // Tell the Respawn Manager to reset the player and the world!
            if (RespawnManager.Instance != null)
            {
                RespawnManager.Instance.RespawnPlayer();
            }
            return; // Stop processing regeneration this frame
        }

        // Reset the death flag if energy is restored (e.g., after respawning)
        if (currentEnergy > 0f && hasDiedFromEnergy)
        {
            hasDiedFromEnergy = false;
        }

        // ==========================================
        // --- 2. REGENERATION LOGIC ---
        // ==========================================
        // Don't regenerate if the player is dead/empty
        if (currentEnergy < maxEnergy && currentEnergy > 0f)
        {
            regenTimer -= Time.deltaTime;
            if (regenTimer <= 0f)
            {
                currentEnergy += regenRate * Time.deltaTime;
                currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
            }
        }
    }

    // ==========================================
    // --- PUBLIC METHODS FOR OTHER SCRIPTS ---
    // ==========================================

    /// <summary>
    /// Attempts to use a set amount of energy instantly. Returns true if successful, false if not enough energy.
    /// </summary>
    public bool UseEnergy(float amount)
    {
        if (currentEnergy < amount) 
        {
            Debug.Log("⚠️ NOT ENOUGH ENERGY! Need " + amount + ", have " + currentEnergy);
            return false;
        }

        currentEnergy -= amount;
        regenTimer = regenDelay; // Reset regen timer
        return true;
    }

    /// <summary>
    /// Drains energy continuously per second. Call this from movement scripts.
    /// </summary>
    public void DrainPerSecond(float amount)
    {
        currentEnergy -= amount * Time.deltaTime;
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);
        regenTimer = regenDelay; // Keep resetting regen while draining
    }

    /// <summary>
    /// Resets energy to max and clears the death flag (used by the RespawnManager).
    /// </summary>
    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        hasDiedFromEnergy = false;
    }
}