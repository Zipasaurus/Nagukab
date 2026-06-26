using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement2D_3D : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;

    [Header("Walking Delay")]
    public float walkGraceTime = 0.15f;

    [Header("NoSprint Zone Settings")]
    [Tooltip("How many seconds you must be completely outside the zone before you can sprint again. Prevents physics jitter.")]
    public float noSprintExitDelay = 0.2f; 

    [Header("Dash Settings")]
    [Tooltip("Drag your 'Dash to' object into this slot!")]
    public Transform dashTarget;
    
    [Tooltip("Exactly how many seconds the dash should take to reach the target.")]
    public float dashDuration = 0.5f; 
    
    [Tooltip("How long you must wait before dashing again.")]
    public float dashCooldown = 1.0f;
    
    [Tooltip("How fast you must double-tap Shift to trigger the dash.")]
    public float doubleTapWindow = 0.3f;

    [Header("Audio")]
    public AudioSource movementAudio;
    public AudioClip walkClip;

    [Header("State (Read Only)")]
    public bool IsWalking;
    public bool IsSprinting;

    // 8-Direction Movement Booleans
    public bool MoveW, MoveWD, MoveD, MoveSD, MoveS, MoveSA, MoveA, MoveWA;

    private Rigidbody rb;
    private Animator animator;
    private Collider playerCollider;
    private Energy energy; // <--- NEW: Reference to the Energy script

    private float horizontalInput;
    private float verticalInput;
    private float walkTimer;

    // --- STICKY ZONE VARIABLES ---
    private bool isOnNoSprintPlatform = false;
    private float noSprintExitTimer = 0f;
    
    // --- DASH VARIABLES ---
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashVelocity = Vector3.zero; 
    
    // Tracks the last time Shift was pressed
    private float lastShiftPress = -10f; 

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // --- REMOVE ALL RIGIDBODY CONSTRAINTS ---
        // This ensures the player is never frozen on any axis (X, Y, Z position or rotation)
        // ----------------------------------------

        animator = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<Collider>();
        energy = GetComponent<Energy>(); // <--- NEW: Find the Energy script

        if (movementAudio == null) movementAudio = GetComponent<AudioSource>();
        
        if (movementAudio != null && walkClip != null)
        {
            movementAudio.clip = walkClip;
            movementAudio.loop = true;
            movementAudio.playOnAwake = false; 
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); 
        verticalInput = Input.GetAxisRaw("Vertical");     
        
        // --- ENERGY LOCKOUT CHECK ---
        // If energy is 0 or less, block all inputs!
        bool isOutOfEnergy = energy != null && energy.currentEnergy <= 0f;
        
        if (isOutOfEnergy)
        {
            horizontalInput = 0f;
            verticalInput = 0f;
        }
        // ----------------------------

        Vector3 moveInput = new Vector3(horizontalInput, 0, verticalInput);

        if (moveInput != Vector3.zero && !isOutOfEnergy) walkTimer = walkGraceTime;
        else walkTimer -= Time.deltaTime;

        IsWalking = walkTimer > 0f && !isOutOfEnergy;

        // --- SPRINTING LOGIC ---
        bool isHoldingBox = animator.GetBool("IsHolding");
        bool rawIsInZone = false;
        
        if (playerCollider != null)
        {
            Vector3 boxSize = playerCollider.bounds.extents + Vector3.one * 0.1f;
            Collider[] overlappingColliders = Physics.OverlapBox(transform.position, boxSize);

            foreach (Collider col in overlappingColliders)
            {
                if (col.CompareTag("NoSprint")) { rawIsInZone = true; break; }
            }
        }

        if (rawIsInZone) { isOnNoSprintPlatform = true; noSprintExitTimer = 0f; }
        else
        {
            noSprintExitTimer += Time.deltaTime;
            if (noSprintExitTimer >= noSprintExitDelay) isOnNoSprintPlatform = false;
        }

        IsSprinting = IsWalking && Input.GetKey(KeyCode.LeftShift) && !isHoldingBox && !isOnNoSprintPlatform && !isOutOfEnergy;

        // --- ENERGY DRAIN FOR MOVEMENT ---
        if (energy != null && IsWalking)
        {
            if (IsSprinting)
                energy.DrainPerSecond(energy.sprintDrainPerSecond);
            else
                energy.DrainPerSecond(energy.walkDrainPerSecond);
        }
        // ---------------------------------

        // Reset 8-direction booleans
        MoveW = MoveWD = MoveD = MoveSD = MoveS = MoveSA = MoveA = MoveWA = false;

        if (IsWalking)
        {
            if (verticalInput > 0f && horizontalInput == 0f) MoveW = true;
            else if (verticalInput > 0f && horizontalInput > 0f) MoveWD = true;
            else if (verticalInput > 0f && horizontalInput < 0f) MoveWA = true;
            else if (verticalInput < 0f && horizontalInput == 0f) MoveS = true;
            else if (verticalInput < 0f && horizontalInput > 0f) MoveSD = true;
            else if (verticalInput < 0f && horizontalInput < 0f) MoveSA = true;
            else if (verticalInput == 0f && horizontalInput > 0f) MoveD = true;
            else if (verticalInput == 0f && horizontalInput < 0f) MoveA = true;
        }

        // --- AUDIO ---
        if (movementAudio != null && walkClip != null)
        {
            if (IsWalking && !movementAudio.isPlaying) movementAudio.Play();
            else if (!IsWalking && movementAudio.isPlaying) movementAudio.Stop();
        }

        // --- ANIMATOR ---
        animator.SetBool("Walking", IsWalking && !IsSprinting && !isDashing);
        animator.SetBool("Sprinting", IsSprinting && !isDashing);

        animator.SetBool("W", MoveW && !isDashing);
        animator.SetBool("WD", MoveWD && !isDashing);
        animator.SetBool("D", MoveD && !isDashing);
        animator.SetBool("SD", MoveSD && !isDashing);
        animator.SetBool("S", MoveS && !isDashing);
        animator.SetBool("SA", MoveSA && !isDashing);
        animator.SetBool("A", MoveA && !isDashing);
        animator.SetBool("WA", MoveWA && !isDashing);

        // ==========================================
        // --- DASH TIMERS ---
        // ==========================================
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) 
            {
                isDashing = false;
                animator.SetBool("Dash", false);
            }
        }

        // ==========================================
        // --- DOUBLE TAP SHIFT DETECTION ---
        // ==========================================
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // Added !isOutOfEnergy to prevent dashing on empty stamina
            if (Time.time - lastShiftPress < doubleTapWindow && dashCooldownTimer <= 0 && !isDashing && !isOutOfEnergy)
            {
                StartDash();
            }
            lastShiftPress = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // Apply the calculated dash velocity (preserve Y velocity so gravity still works!)
            rb.velocity = new Vector3(dashVelocity.x, rb.velocity.y, dashVelocity.z);

            // Safety check: If we reach the target early, stop dashing
            if (dashTarget != null && Vector3.Distance(transform.position, dashTarget.position) < 0.2f)
            {
                isDashing = false;
                animator.SetBool("Dash", false);
            }
        }
        else
        {
            // Normal movement
            float speed = IsSprinting ? sprintSpeed : walkSpeed;
            Vector3 moveInput = new Vector3(horizontalInput, 0, verticalInput);
            Vector3 moveDirection = moveInput.normalized;

            rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);
        }
    }

    // ==========================================
    // --- DASH CALCULATION METHOD ---
    // ==========================================
    void StartDash()
    {
        // Safety check to ensure you didn't forget to drag the object in
        if (dashTarget == null)
        {
            Debug.LogError("PlayerMovement: You forgot to assign the 'Dash Target' object in the Inspector!");
            return;
        }

        // --- ENERGY CHECK FOR DASH ---
        if (energy != null && !energy.UseEnergy(energy.dashCost))
        {
            return; // Not enough energy, cancel the dash
        }
        // -----------------------------

        // Calculate the direction to the target (ignore Y axis so we don't fly into the sky)
        Vector3 dir = dashTarget.position - transform.position;
        dir.y = 0; 
        float distance = dir.magnitude;

        // If we are already standing on the target, don't dash
        if (distance < 0.1f) return;

        dir.Normalize();

        // THE MAGIC MATH: Calculate the exact speed needed to cover the distance in the set duration
        float requiredSpeed = distance / dashDuration;

        // Store the final velocity vector
        dashVelocity = new Vector3(dir.x * requiredSpeed, 0, dir.z * requiredSpeed);

        // Activate the dash state
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        // Turn ON the Dash bool in your Animator!
        animator.SetBool("Dash", true);
    }
}