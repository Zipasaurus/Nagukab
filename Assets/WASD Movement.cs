using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement2D_3D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Walking Delay")]
    public float walkGraceTime = 0.15f;

    [Header("Visual / Metarig")]
    public Transform visualRoot;
    public bool enableFlip = true;

    [Header("Walk Rotations")]
    public Vector3 walkFacingRight = Vector3.zero;
    public Vector3 walkFacingLeft = new Vector3(0f, 180f, 0f);

    [Header("Sprint Rotations")]
    public Vector3 sprintFacingRight = Vector3.zero;
    public Vector3 sprintFacingLeft = new Vector3(0f, 180f, 0f);

    [Header("State (Read Only)")]
    public bool IsWalking;
    public bool IsSprinting;

    public bool WalkLeft;
    public bool WalkRight;

    public bool SprintLeft;
    public bool SprintRight;

    private Rigidbody rb;
    private Animator animator;

    private float horizontalInput;
    private float walkTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

      

        if (visualRoot == null)
            Debug.LogWarning("Visual Root (Metarig) not assigned on " + gameObject.name);
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Walking grace buffer
        if (horizontalInput != 0)
            walkTimer = walkGraceTime;
        else
            walkTimer -= Time.deltaTime;

        IsWalking = walkTimer > 0f;
        IsSprinting = IsWalking && Input.GetKey(KeyCode.LeftShift);

        // Reset all direction states
        WalkLeft = WalkRight = SprintLeft = SprintRight = false;

        if (IsSprinting)
        {
            SprintLeft  = horizontalInput < 0f;
            SprintRight = horizontalInput > 0f;
        }
        else if (IsWalking)
        {
            WalkLeft  = horizontalInput < 0f;
            WalkRight = horizontalInput > 0f;
        }

        // Animator parameters
        animator.SetBool("Walking", IsWalking && !IsSprinting);
        animator.SetBool("Sprinting", IsSprinting);

        animator.SetBool("WalkLeft", WalkLeft);
        animator.SetBool("WalkRight", WalkRight);
        animator.SetBool("SprintLeft", SprintLeft);
        animator.SetBool("SprintRight", SprintRight);

        HandleVisualRotation();
    }

    void FixedUpdate()
    {
        float speed = IsSprinting ? sprintSpeed : walkSpeed;
        rb.velocity = new Vector3(horizontalInput * speed, rb.velocity.y, 0f);
    }

    void HandleVisualRotation()
    {
        if (!enableFlip || visualRoot == null || horizontalInput == 0)
            return;

        bool facingRight = horizontalInput > 0f;

        if (IsSprinting)
        {
            visualRoot.localRotation = Quaternion.Euler(
                facingRight ? sprintFacingRight : sprintFacingLeft
            );
        }
        else if (IsWalking)
        {
            visualRoot.localRotation = Quaternion.Euler(
                facingRight ? walkFacingRight : walkFacingLeft
            );
        }
    }
}