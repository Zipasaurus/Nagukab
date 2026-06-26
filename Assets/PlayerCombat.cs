using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Left Weapon (Left Click)")]
    public GameObject leftWeaponObject;
    public float leftClickDamage = 10f;

    [Header("Right Weapon (Right Click)")]
    public GameObject rightWeaponObject;
    public float rightClickDamage = 20f;

    [Header("Overall Timing Settings")]
    [Tooltip("The total length of the attack animation.")]
    public float attackDuration = 0.6f; 
    
    [Tooltip("How long you must wait after an attack before you can attack again.")]
    public float attackCooldown = 0.6f; 

    [Header("Hitbox Timing Settings")]
    [Tooltip("How long AFTER clicking before the weapon's hitbox actually turns on (the wind-up time).")]
    public float hitboxActivationDelay = 0.2f;

    private BoxCollider leftCollider;
    private WeaponHitbox leftHitbox;
    private BoxCollider rightCollider;
    private WeaponHitbox rightHitbox;
    
    private Energy energy; 
    
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        energy = GetComponent<Energy>();

        if (leftWeaponObject != null) 
        {
            leftCollider = leftWeaponObject.GetComponent<BoxCollider>();
            leftHitbox = leftWeaponObject.GetComponent<WeaponHitbox>();
            if(leftCollider != null) leftCollider.enabled = false;
        } 

        if (rightWeaponObject != null) 
        {
            rightCollider = rightWeaponObject.GetComponent<BoxCollider>();
            rightHitbox = rightWeaponObject.GetComponent<WeaponHitbox>();
            if(rightCollider != null) rightCollider.enabled = false;
        } 
    }

    void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && !isAttacking && cooldownTimer <= 0)
        {
            StartAttack("LeftAttack", leftClickDamage, leftCollider, leftHitbox);
        }
        else if (Input.GetMouseButtonDown(1) && !isAttacking && cooldownTimer <= 0)
        {
            StartAttack("RightAttack", rightClickDamage, rightCollider, rightHitbox);
        }
    }

    void StartAttack(string attackName, float damage, BoxCollider col, WeaponHitbox hitbox)
    {
        if (col == null || hitbox == null) {
            Debug.LogError("Weapon components missing for " + attackName);
            return;
        }

        if (energy != null)
        {
            float cost = (attackName == "LeftAttack") ? energy.leftAttackCost : energy.rightAttackCost;
            if (!energy.UseEnergy(cost)) return; 
        }

        isAttacking = true; 
        
        // Set the damage and play the animation IMMEDIATELY
        hitbox.SetDamage(damage);
        animator.SetBool(attackName, true);
        
        // NOTE: We do NOT turn the collider on here anymore! 
        // The Coroutine below will handle the delay.
        
        StartCoroutine(EndAttackSequence(attackName, col, hitbox));
    }

    // ==========================================
    // --- THE DELAY LOGIC ---
    // ==========================================
    IEnumerator EndAttackSequence(string attackName, BoxCollider col, WeaponHitbox hitbox)
    {
        // 1. WAIT FOR THE WIND-UP DELAY
        // The animation is playing, but the weapon cannot deal damage yet.
        yield return new WaitForSeconds(hitboxActivationDelay);

        // 2. TURN ON THE HITBOX
        // The sword has reached the enemy!
        col.enabled = true;
        hitbox.ResetHit(); // Allow it to register a hit

        // 3. WAIT FOR THE REMAINING ATTACK TIME
        // (Total duration minus the delay we just waited)
        float remainingTime = attackDuration - hitboxActivationDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        // 4. TURN EVERYTHING OFF
        col.enabled = false;
        animator.SetBool(attackName, false);

        isAttacking = false;
        cooldownTimer = attackCooldown;
    }
}