using System.Collections;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint; 
    public Animator animator;

    [Header("Settings")]
    public float inputCooldown = 0.1f;
    
    [Tooltip("Set to 1 if your Pickup/Drop animations are on the UpperBody layer. Set to 0 if they are on the Base Layer.")]
    public int animationLayerIndex = 0; 

    private bool isHolding = false;
    private bool isBusy = false; 
    private GameObject currentTarget;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isBusy)
        {
            if (!isHolding) StartCoroutine(PickupRoutine());
            else if (isHolding) StartCoroutine(DropRoutine());
        }
    }

    private IEnumerator PickupRoutine()
    {
        isBusy = true; 
        
        animator.SetBool("Pickup", true);
        yield return SafeWaitForAnimation(animationLayerIndex);
        animator.SetBool("Pickup", false);

        if (currentTarget != null)
        {
            Quaternion originalRotation = currentTarget.transform.rotation;
            currentTarget.transform.SetParent(holdPoint);
            currentTarget.transform.localPosition = Vector3.zero;
            currentTarget.transform.rotation = originalRotation;

            Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = currentTarget.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            isHolding = true;
            animator.SetBool("IsHolding", true); 
        }
        else
        {
            animator.SetBool("NoBox", true);
            yield return SafeWaitForAnimation(animationLayerIndex);
            animator.SetBool("NoBox", false);
        }

        yield return new WaitForSeconds(inputCooldown);
        isBusy = false;
    }

    private IEnumerator DropRoutine()
    {
        isBusy = true; 
        animator.SetBool("Drop", true);
        Debug.Log("🟢 DROP STARTED: Bool is now TRUE"); // DETECTIVE LOG

        if (currentTarget != null)
        {
            currentTarget.transform.SetParent(null, true); 
            
            Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero; 
            }

            Collider col = currentTarget.GetComponent<Collider>();
            if (col != null) col.enabled = true;
        }

        isHolding = false;
        animator.SetBool("IsHolding", false);
        currentTarget = null; 

        // WAIT FOR ANIMATION (Now with a hard cap so it never freezes!)
        yield return SafeWaitForAnimation(animationLayerIndex);
        
        Debug.Log("🔴 DROP FINISHED: Bool is now FALSE"); // DETECTIVE LOG
        animator.SetBool("Drop", false);

        yield return new WaitForSeconds(inputCooldown);
        isBusy = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isHolding && other.CompareTag("Pickupable"))
        {
            currentTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isHolding && other.gameObject == currentTarget)
        {
            currentTarget = null;
        }
    }

    // ==========================================
    // --- BULLETPROOF ANIMATION WAIT (V2) ---
    // ==========================================
    private IEnumerator SafeWaitForAnimation(int layer)
    {
        if (animator == null) yield break;
        if (layer >= animator.layerCount) layer = 0;

        // Wait 1 frame for the transition to start
        yield return null;

        float length = 0.5f; // Default fallback
        
        try 
        {
            length = animator.GetCurrentAnimatorStateInfo(layer).length;
        } 
        catch 
        {
            Debug.LogWarning("PickupSystem: Could not read animation length. Using 0.5s fallback.");
        }

        // --- THE FIX: HARD CAP THE WAIT TIME ---
        // If Unity returns 0, a negative number, or Infinity, we force it to 0.5 seconds.
        // It will NEVER wait more than 1 second, preventing the script from freezing!
        if (length <= 0f || length > 1f) 
        {
            length = 0.5f; 
        }

        yield return new WaitForSeconds(length);
    }
}