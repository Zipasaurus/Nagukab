using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    private float damageAmount;
    private bool hasHit = false;

    // Called by the Player script when an attack starts
    public void SetDamage(float damage)
    {
        damageAmount = damage;
    }

    // Called by the Player script when an attack starts to allow it to hit again
    public void ResetHit()
    {
        hasHit = false;
    }

    // This triggers when the Box Collider touches another collider
    private void OnTriggerEnter(Collider other)
    {
        // --- THE FIX: IGNORE THE PLAYER ---
        if (other.CompareTag("Player"))
        {
            return; // This immediately stops the function. No damage applied, 'hasHit' stays false!
        }
        // ----------------------------------

        Debug.Log("🟢 WEAPON HIT SOMETHING: " + other.gameObject.name);

        if (hasHit) return;

        Health enemyHealth = other.GetComponentInParent<Health>(); 
        
        if (enemyHealth != null)
        {
            Debug.Log("🟢 FOUND HEALTH SCRIPT! Applying " + damageAmount + " damage.");
            enemyHealth.TakeDamage(damageAmount);
            hasHit = true; 
        }
        else
        {
            Debug.LogWarning("🔴 HIT " + other.gameObject.name + " BUT COULD NOT FIND HEALTH SCRIPT!");
        }
    }
    }