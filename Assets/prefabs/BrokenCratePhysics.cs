using UnityEngine;

public class BrokenCratePhysics : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("How to handle the colliders on this broken crate.")]
    public CollisionMode collisionMode = CollisionMode.DisableImmediately;
    
    [Tooltip("If using 'DisableAfterDelay', how many seconds to wait before disabling colliders.")]
    public float disableDelay = 0.5f;

    public enum CollisionMode
    {
        DisableImmediately,    // Colliders are removed instantly (player walks through)
        MakeTriggers,          // Colliders become triggers (player walks through, but can detect overlap)
        DisableAfterDelay,     // Colliders stay solid for a moment, then become passable
        KeepSolid              // Colliders stay solid forever (NOT recommended)
    }

    void Start()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        
        switch (collisionMode)
        {
            case CollisionMode.DisableImmediately:
                foreach (Collider col in colliders)
                {
                    col.enabled = false;
                }
                Debug.Log("📦 Broken crate colliders disabled immediately.");
                break;
                
            case CollisionMode.MakeTriggers:
                foreach (Collider col in colliders)
                {
                    col.isTrigger = true;
                }
                Debug.Log("📦 Broken crate colliders set to triggers.");
                break;
                
            case CollisionMode.DisableAfterDelay:
                Invoke("DisableColliders", disableDelay);
                Debug.Log($"📦 Broken crate colliders will disable in {disableDelay} seconds.");
                break;
                
            case CollisionMode.KeepSolid:
                Debug.Log("📦 Broken crate colliders remain solid.");
                break;
        }
    }

    void DisableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
}