using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth; 

    [Header("Initial State Settings")]
    [Tooltip("If checked, the object will start at 0 HP (broken) when the game begins.")]
    public bool startBroken = false;

    [Header("Destruction Settings")]
    [Tooltip("If checked, the object will reach 0 HP and 'die', but will NOT be deleted.")]
    public bool cantBeDestroyed = false; 

    [Header("Invulnerability Settings")]
    [Tooltip("If checked, once this object is fully repaired (reaches max health), it can NEVER take damage again.")]
    public bool becomeInvulnerableWhenRepaired = false;

    [Header("Loot Settings")]
    [Tooltip("If checked, this object will drop a battery when killed by the player.")]
    public bool dropsBattery = false; 
    public GameObject batteryPrefab;
    public Vector3 batterySpawnOffset = new Vector3(0, 1.5f, 0);

    [Header("Broken Crate Settings")]
    public bool spawnsBrokenCrate = false; 
    public GameObject brokenCratePrefab;
    public Vector3 brokenCrateSpawnOffset = Vector3.zero;
    public float brokenCrateDestroyDelay = 3f;

    [Header("Debug Settings")]
    public bool debugMode = false;

    private Vector3 startPosition;
    private Quaternion startRotation;
    
    // --- Invulnerability tracking ---
    private bool hasBeenBroken = false;
    private bool isNowInvulnerable = false;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (startBroken)
        {
            currentHealth = 0f;
            hasBeenBroken = true; 
        }
        else
        {
            currentHealth = maxHealth;
            hasBeenBroken = false; 
        }
    }

    public void ResetHealth()
    {
        // When the player respawns, reset the door back to its starting state!
        isNowInvulnerable = false;
        hasBeenBroken = startBroken;

        if (startBroken) currentHealth = 0f;
        else currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        // ==========================================
        // --- 1. CHECK INVULNERABILITY ---
        // ==========================================
        if (isNowInvulnerable) 
        {
            // It's permanently fixed! Ignore all damage and healing.
            return; 
        }

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); 

        // Track if it has ever been damaged
        if (currentHealth < maxHealth) 
        {
            hasBeenBroken = true;
        }

        Debug.Log(gameObject.name + " took " + amount + " damage! HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // ==========================================
        // --- 2. CHECK IF IT JUST GOT REPAIRED ---
        // ==========================================
        if (hasBeenBroken && currentHealth >= maxHealth && becomeInvulnerableWhenRepaired)
        {
            isNowInvulnerable = true;
            Debug.Log("🛡️ " + gameObject.name + " is fully repaired and now INVULNERABLE!");
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " reached 0 HP!");
        
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("💀 PLAYER DIED! Respawning...");
            if (RespawnManager.Instance != null) RespawnManager.Instance.RespawnPlayer();
            return; 
        }
        
        if (cantBeDestroyed) return; 

        if (debugMode)
        {
            Debug.Log("🔄 DEBUG MODE: Respawning crate at original position!");
            GameObject prefabToRespawn = null;
            #if UNITY_EDITOR
            prefabToRespawn = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
            #endif
            
            if (prefabToRespawn == null)
            {
                transform.position = startPosition;
                transform.rotation = startRotation;
                currentHealth = maxHealth;
                return; 
            }
            else
            {
                Instantiate(prefabToRespawn, startPosition, startRotation);
                Destroy(gameObject);
                return;
            }
        }

        RespawnableObject respawnObj = GetComponent<RespawnableObject>();

        if (spawnsBrokenCrate && brokenCratePrefab != null)
        {
            Vector3 spawnPosition = transform.TransformPoint(brokenCrateSpawnOffset);
            GameObject brokenCrate = Instantiate(brokenCratePrefab, spawnPosition, transform.rotation);
            if (respawnObj != null) respawnObj.RegisterDebris(brokenCrate);
            if (brokenCrateDestroyDelay > 0f) Destroy(brokenCrate, brokenCrateDestroyDelay);
        }

        if (dropsBattery && batteryPrefab != null)
        {
            Vector3 spawnPosition = transform.TransformPoint(batterySpawnOffset);
            Instantiate(batteryPrefab, spawnPosition, Quaternion.identity);
        }

        if (respawnObj != null) respawnObj.OnObjectDestroyed(); 
        else Destroy(gameObject); 
    }
}