using UnityEngine;

public class SoftFollowCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The object the camera will follow (usually the player)")]
    public Transform target;

    [Header("Camera Position")]
    [Tooltip("How far back the camera is from the target")]
    public float distance = 10f;
    
    [Tooltip("How high above the target the camera sits")]
    public float height = 5f;
    
    [Tooltip("Horizontal offset (left/right from target)")]
    public float horizontalOffset = 0f;

    [Header("Follow Settings")]
    [Tooltip("How smoothly the camera follows (lower = smoother/laggier, higher = snappier)")]
    [Range(0.1f, 20f)]
    public float smoothSpeed = 5f;

    [Tooltip("How smoothly the camera rotates to look at the target")]
    [Range(0.1f, 20f)]
    public float rotationSmoothSpeed = 10f;

    [Header("Advanced Settings")]
    [Tooltip("If true, the camera will rotate to match the target's rotation")]
    public bool followTargetRotation = false;
    
    [Tooltip("Additional rotation offset for the camera")]
    public Vector3 rotationOffset = Vector3.zero;

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("SoftFollowCamera: No target assigned!");
            return;
        }

        // Calculate the desired position behind and above the target
        Vector3 desiredPosition = CalculateDesiredPosition();

        // Smoothly move to the desired position
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            1f / smoothSpeed
        );

        // Always look at the target
        SmoothLookAtTarget();
    }

    private Vector3 CalculateDesiredPosition()
    {
        Vector3 targetPosition = target.position;

        // If following target rotation, calculate position relative to target's facing direction
        if (followTargetRotation)
        {
            Vector3 direction = -target.forward * distance;
            Vector3 up = Vector3.up * height;
            Vector3 right = target.right * horizontalOffset;
            
            return targetPosition + direction + up + right;
        }
        else
        {
            // Fixed world-space position (always behind and above)
            return targetPosition + new Vector3(horizontalOffset, height, -distance);
        }
    }

    private void SmoothLookAtTarget()
    {
        // Calculate the direction to the target
        Vector3 direction = target.position - transform.position;
        
        // Calculate the desired rotation
        Quaternion desiredRotation = Quaternion.LookRotation(direction);
        
        // Apply rotation offset
        desiredRotation *= Quaternion.Euler(rotationOffset);

        // Smoothly rotate to look at the target
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            desiredRotation, 
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    // Optional: Draw gizmos in the editor to visualize the camera setup
    void OnDrawGizmos()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(CalculateDesiredPosition(), 0.3f);
        }
    }
}