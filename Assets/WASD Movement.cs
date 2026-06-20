using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement2D_3D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Walking Delay")]
    public float walkGraceTime = 0.15f;

    [Header("State (Read Only)")]
    public bool IsWalking;
    public bool IsSprinting;

    // 8-Direction Movement Booleans
    public bool MoveW, MoveWD, MoveD, MoveSD, MoveS, MoveSA, MoveA, MoveWA;

    private Rigidbody rb;
    private Animator animator;

    private float horizontalInput;
    private float verticalInput;
    private float walkTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Get both Horizontal (A/D) and Vertical (W/S) input
        horizontalInput = Input.GetAxisRaw("Horizontal"); 
        verticalInput = Input.GetAxisRaw("Vertical");     

        Vector3 moveInput = new Vector3(horizontalInput, 0, verticalInput);

        // Walking grace buffer
        if (moveInput != Vector3.zero)
            walkTimer = walkGraceTime;
        else
            walkTimer -= Time.deltaTime;

        IsWalking = walkTimer > 0f;

        // --- SPRINTING DISABLED WHEN HOLDING ---
        bool isHoldingBox = animator.GetBool("IsHolding");
        IsSprinting = IsWalking && Input.GetKey(KeyCode.LeftShift) && !isHoldingBox;
        // ---------------------------------------

        // Reset all 8-direction booleans
        MoveW = MoveWD = MoveD = MoveSD = MoveS = MoveSA = MoveA = MoveWA = false;

        // Set the correct direction boolean based on input combination
        if (IsWalking)
        {
            // UP directions
            if (verticalInput > 0f && horizontalInput == 0f) MoveW = true;
            else if (verticalInput > 0f && horizontalInput > 0f) MoveWD = true;
            else if (verticalInput > 0f && horizontalInput < 0f) MoveWA = true;

            // DOWN directions
            else if (verticalInput < 0f && horizontalInput == 0f) MoveS = true;
            else if (verticalInput < 0f && horizontalInput > 0f) MoveSD = true;
            else if (verticalInput < 0f && horizontalInput < 0f) MoveSA = true;

            // LEFT/RIGHT directions
            else if (verticalInput == 0f && horizontalInput > 0f) MoveD = true;
            else if (verticalInput == 0f && horizontalInput < 0f) MoveA = true;
        }

        // --- ANIMATOR PARAMETERS ---
        animator.SetBool("Walking", IsWalking && !IsSprinting);
        animator.SetBool("Sprinting", IsSprinting);

        // 8-Direction Booleans
        animator.SetBool("W", MoveW);
        animator.SetBool("WD", MoveWD);
        animator.SetBool("D", MoveD);
        animator.SetBool("SD", MoveSD);
        animator.SetBool("S", MoveS);
        animator.SetBool("SA", MoveSA);
        animator.SetBool("A", MoveA);
        animator.SetBool("WA", MoveWA);
    }

    void FixedUpdate()
    {
        float speed = IsSprinting ? sprintSpeed : walkSpeed;
        
        // Combine inputs and normalize so diagonals aren't faster
        Vector3 moveInput = new Vector3(horizontalInput, 0, verticalInput);
        Vector3 moveDirection = moveInput.normalized;

        // Apply velocity to X and Z axes
        rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);
    }
}