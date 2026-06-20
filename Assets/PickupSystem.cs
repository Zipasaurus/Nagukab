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
    public int animationLayerIndex = 1; 

    private bool isHolding = false;
    private bool isBusy = false; 
    private GameObject currentTarget;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isBusy)
        {
            if (!isHolding)
            {
                StartCoroutine(PickupRoutine());
            }
            else if (isHolding)
            {
                StartCoroutine(DropRoutine());
            }
        }
    }

    private IEnumerator PickupRoutine()
    {
        isBusy = true; 
        
        animator.SetBool("Pickup", true);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(animationLayerIndex).length);
        animator.SetBool("Pickup", false);

        if (currentTarget != null)
        {
            // --- 1. SAVE THE CUBE'S CURRENT ROTATION ---
            Quaternion originalRotation = currentTarget.transform.rotation;

            // 2. Parent the object to the hand
            currentTarget.transform.SetParent(holdPoint);

            // 3. Snap it to the hand's position
            currentTarget.transform.localPosition = Vector3.zero;

            // --- 4. RESTORE THE CUBE'S ORIGINAL ROTATION ---
            currentTarget.transform.rotation = originalRotation;
            // ---------------------------------------------

            // Disable Physics
            Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Disable Collider
            Collider col = currentTarget.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            
            isHolding = true;
            animator.SetBool("IsHolding", true); 
        }
        else
        {
            animator.SetBool("NoBox", true);
            yield return null; 
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(animationLayerIndex).length);
            animator.SetBool("NoBox", false);
        }

        yield return new WaitForSeconds(inputCooldown);
        isBusy = false;
    }

    private IEnumerator DropRoutine()
    {
        isBusy = true; 

        animator.SetBool("Drop", true);

        if (currentTarget != null)
        {
            // Unparent immediately (true = keep world position AND rotation)
            currentTarget.transform.SetParent(null, true); 
            
            // Re-enable Physics
            Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero; // Reset velocity so it doesn't inherit weird movement
            }

            // Re-enable Collider
            Collider col = currentTarget.GetComponent<Collider>();
            if (col != null) col.enabled = true;

            isHolding = false;
            animator.SetBool("IsHolding", false);

            currentTarget = null; 
        }

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(animationLayerIndex).length);
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
        // If we are already holding the object, IGNORE the exit trigger!
        if (!isHolding && other.gameObject == currentTarget)
        {
            currentTarget = null;
        }
    }
}