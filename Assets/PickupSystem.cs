using System.Collections;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    [Header("References")]
    public Transform holdPoint;
    public Animator animator;

    [Header("Settings")]
    public float inputCooldown = 0.1f;

    [Tooltip("If your Pickup animation is on the UpperBody layer, this should be 1. If it's on the Base Layer, this should be 0.")]
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

        Debug.Log("--- PICKUP STARTED ---");
        Debug.Log("Current Target is: " + (currentTarget != null ? currentTarget.name : "NULL (Nothing)"));

        // 1. Trigger Pickup
        animator.SetBool("Pickup", true);

        // 2. Wait for animation (Using the correct layer index now!)
        float animLength = animator.GetCurrentAnimatorStateInfo(animationLayerIndex).length;
        Debug.Log("Waiting for Pickup animation. Length: " + animLength);
        yield return new WaitForSeconds(animLength);

        // 3. Reset Pickup boolean
        animator.SetBool("Pickup", false);

        // 4. Check for object
        if (currentTarget != null)
        {
            Debug.Log("SUCCESS: Grabbed " + currentTarget.name);
            currentTarget.transform.SetParent(holdPoint);
            currentTarget.transform.localPosition = Vector3.zero;
            currentTarget.transform.localRotation = Quaternion.identity;

            Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            isHolding = true;
        }
        else
        {
            // --- FAILURE: No object detected! ---
            Debug.Log("FAILURE: No object in range. Setting NoBox to TRUE.");

            animator.SetBool("NoBox", true);

            // Wait one frame for Animator to register
            yield return null;

            float noBoxLength = animator.GetCurrentAnimatorStateInfo(animationLayerIndex).length;
            Debug.Log("Waiting for NoBox animation. Length: " + noBoxLength);
            yield return new WaitForSeconds(noBoxLength);

            Debug.Log("NoBox animation finished. Setting NoBox to FALSE.");
            animator.SetBool("NoBox", false);
        }

        yield return new WaitForSeconds(inputCooldown);
        isBusy = false;
        Debug.Log("--- PICKUP ROUTINE ENDED ---");
    }

    private IEnumerator DropRoutine()
    {
        isBusy = true;
        animator.SetBool("Drop", true);

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(animationLayerIndex).length);
        animator.SetBool("Drop", false);

        if (currentTarget != null)
        {
            currentTarget.transform.SetParent(null);
            Rigidbody rb = currentTarget.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            isHolding = false;
            currentTarget = null;
        }

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
        if (other.gameObject == currentTarget)
        {
            currentTarget = null;
        }
    }
}