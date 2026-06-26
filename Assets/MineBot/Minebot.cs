using UnityEngine;

public class MineBot : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    
    [Header("Audio Sources (Add 3 AudioSources to this object)")]
    public AudioSource drivingSound;
    public AudioSource proximitySound;
    public AudioSource explosionSound;
    
    [Header("Explosion VFX")]
    public GameObject explosionVFXPrefab;

    [Header("Sight & Movement Settings")]
    public float sightRange = 15f;
    public float driveSpeed = 5f;

    [Header("Proximity Beep Settings")]
    public float maxBeepDistance = 15f;
    public float minBeepDistance = 2f;
    public float minPitch = 1f;
    public float maxPitch = 3f;

    [Header("Attack & Explosion Settings")]
    public float attackDistance = 2.5f;
    public float attackDamage = 30f;
    public float explosionDestroyDelay = 1.5f;
    
    [Tooltip("The radius of the explosion blast.")]
    public float explosionRadius = 5f;

    [Header("Jump & Air Explosion Settings")]
    public float jumpUpSpeed = 10f;
    public float jumpDuration = 1.5f;

    [Header("Visual Settings")]
    public Vector3 rotationOffset = Vector3.zero;

    private bool isChasing = false;
    private bool isJumping = false;
    private bool hasExploded = false;
    private float jumpTimer = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null || hasExploded) return;

        float distance = Vector3.Distance(transform.position, player.position);
        
        // ==========================================
        // --- BULLETPROOF LINE OF SIGHT ---
        // ==========================================
        Vector3 targetCenter = player.position + Vector3.up * 1f;
        Vector3 direction = (targetCenter - transform.position).normalized;

        bool hitSomething = Physics.Raycast(transform.position, direction, out RaycastHit hit, sightRange);

        // 1. INITIAL SPOTTING
        if (!isChasing && !isJumping)
        {
            if (distance <= sightRange && hitSomething && hit.collider.CompareTag("Player"))
            {
                StartChasing();
            }
        }

        // 2. LOSING SIGHT (While Chasing)
        else if (isChasing && !isJumping)
        {
            if (hitSomething && !hit.collider.CompareTag("Player") && hit.collider.gameObject != gameObject)
            {
                Debug.Log("🔴 MineBot hit a wall: " + hit.collider.gameObject.name + ". Losing sight!");
                isChasing = false;
                if (animator != null) animator.SetBool("Drive", false);
                if (drivingSound != null) drivingSound.Stop();
                if (proximitySound != null) proximitySound.Stop();
                return; 
            }
            
            if (!hitSomething)
            {
                Debug.Log("🔴 MineBot can't see anything in range. Losing sight!");
                isChasing = false;
                if (animator != null) animator.SetBool("Drive", false);
                if (drivingSound != null) drivingSound.Stop();
                if (proximitySound != null) proximitySound.Stop();
                return;
            }
        }

        // ==========================================
        // 3. CHASING & PROXIMITY BEEP
        // ==========================================
        if (isChasing)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, driveSpeed * Time.deltaTime);
            
            Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookTarget);
            transform.Rotate(rotationOffset);

            float proximityPercent = Mathf.InverseLerp(maxBeepDistance, minBeepDistance, distance);
            
            if (proximitySound != null)
            {
                if (!proximitySound.isPlaying) proximitySound.Play();
                proximitySound.pitch = Mathf.Lerp(minPitch, maxPitch, proximityPercent);
            }

            if (distance <= attackDistance)
            {
                StartJump();
            }
        }

        // ==========================================
        // 4. JUMPING & FLYING UPWARDS
        // ==========================================
        if (isJumping)
        {
            transform.position += Vector3.up * jumpUpSpeed * Time.deltaTime;

            Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
            transform.LookAt(lookTarget);
            transform.Rotate(rotationOffset);

            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0)
            {
                TriggerExplosion();
            }
        }
    }

    void StartChasing()
    {
        isChasing = true;
        if (animator != null) animator.SetBool("Drive", true);
        if (drivingSound != null) drivingSound.Play();
    }

    void StartJump()
    {
        isChasing = false;
        isJumping = true;
        jumpTimer = jumpDuration;

        if (drivingSound != null) drivingSound.Stop();
        if (proximitySound != null) proximitySound.Stop();

        if (animator != null) animator.SetBool("Jump", true);
    }

    void TriggerExplosion()
    {
        hasExploded = true;
        isJumping = false;

        if (animator != null) 
        {
            animator.SetBool("Drive", false);
            animator.SetBool("Jump", false);
        }

        if (explosionSound != null) explosionSound.Play();

        // ==========================================
        // --- SIMPLE DISTANCE CHECK TO PLAYER ---
        // ==========================================
        if (player != null)
        {
            // Calculate distance to the player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            Debug.Log($"💥 Explosion! Distance to player: {distanceToPlayer:F2} (Radius: {explosionRadius})");
            
            // Only deal damage if the player is within the explosion radius
            if (distanceToPlayer <= explosionRadius)
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"💥 Player hit by explosion!");
                }
            }
            else
            {
                Debug.Log("💨 Player is outside the explosion radius! Safe.");
            }
        }

        if (explosionVFXPrefab != null)
        {
            Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
        }

        // ==========================================
        // --- RESPAWN SYSTEM INTEGRATION ---
        // ==========================================
        RespawnableObject respawnObj = GetComponent<RespawnableObject>();
        
        if (respawnObj != null)
        {
            respawnObj.OnObjectDestroyed(); 
        }
        else
        {
            Destroy(gameObject, explosionDestroyDelay);
        }
    }
}