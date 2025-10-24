using System.Collections.Generic;
using UnityEngine;

public class MeleeController : WeaponController
{
    #region Melee-Specific State
    [Header("Melee Settings")]
    [SerializeField] private LayerMask attackLayers;
    [SerializeField] private bool showDebugGizmos = true;
    
    private bool isBlocking = false;
    private int currentComboCount = 0;
    private float lastComboTime = 0f;
    private const float comboResetTime = 1.5f; // Time window to continue combo
    private const int maxComboCount = 3;
    
    // Animation state names for combos
    private const string ANIM_ATTACK_1 = "Attack1";
    private const string ANIM_ATTACK_2 = "Attack2";
    private const string ANIM_ATTACK_3 = "Attack3";
    private const string ANIM_BLOCK = "Block";
    
    public bool IsBlocking => isBlocking;
    public int CurrentCombo => currentComboCount;
    #endregion

    #region Unity Lifecycle
    protected override void Update()
    {
        base.Update();
        
        // Reset combo if too much time has passed
        if (Time.time - lastComboTime > comboResetTime && currentComboCount > 0)
        {
            ResetCombo();
        }
    }
    #endregion

    #region Abstract Implementation
    protected override void OnEquip()
    {
        // Play equip animation
        ChangeAnimationState(ANIM_IDLE);
        RequestPlayerAnimation("EquipMelee");
        
        // Reset states
        isBlocking = false;
        ResetCombo();
        
        Debug.Log($"Equipped melee weapon: {weaponData.name}");
    }

    public override void OnUnequip()
    {
        // Stop blocking
        if (isBlocking)
        {
            StopBlocking();
        }
        
        // Cancel pending attacks
        CancelInvoke(nameof(ExecuteMeleeAttack));
        CancelInvoke(nameof(ResetCombo));
        
        Debug.Log($"Unequipped melee weapon: {weaponData.name}");
    }

    public override void PrimaryAttack()
    {
        // Validation checks
        if (!CanAttack()) return;
        
        // Can't attack while blocking
        if (isBlocking) return;
        
        // Start attack
        StartCooldown();
        
        // Update combo
        currentComboCount = (currentComboCount % maxComboCount) + 1;
        lastComboTime = Time.time;
        
        // Play appropriate combo animation
        string attackAnim = GetComboAnimation();
        ChangeAnimationState(attackAnim);
        RequestPlayerAnimation(attackAnim);
        
        // Play swing sound
        PlayFireSound();
        
        // Schedule the actual attack hitbox check (after windup)
        float attackDelay = weaponData.meleeSwingDuration * 0.4f; // Attack lands 40% through animation
        Invoke(nameof(ExecuteMeleeAttack), attackDelay);
        
        // Fire event
        // OnWeaponFired?.Invoke();
    }

    public override void SecondaryAttack()
    {
        // Toggle blocking if weapon supports it
        if (!weaponData.canBlock) return;
        
        if (isBlocking)
        {
            StopBlocking();
        }
        else
        {
            StartBlocking();
        }
    }
    #endregion

    #region Melee Attack Logic
    /// <summary>
    /// Execute the actual melee attack detection.
    /// Called after animation windup delay.
    /// </summary>
    private void ExecuteMeleeAttack()
    {
        Vector3 origin = GetAimOrigin();
        Vector3 direction = GetAimDirection();
        
        // Perform cone/sphere detection based on swing angle
        List<Actor> hitActors = DetectMeleeTargets(origin, direction);
        
        if (hitActors.Count > 0)
        {
            // Calculate damage with combo multiplier
            int baseDamage = CalculateDamage();
            float comboMultiplier = 1f + (currentComboCount - 1) * 0.15f; // +15% per combo level
            int finalDamage = Mathf.RoundToInt(baseDamage * comboMultiplier);
            
            // Apply damage to all hit targets
            foreach (Actor actor in hitActors)
            {
                // Apply knockback
                ApplyKnockback(actor, direction);
                
                // Deal damage
                Vector3 hitPoint = actor.transform.position;
                DealDamage(actor, finalDamage, hitPoint);
                
                Debug.Log($"Melee hit {actor.name} for {finalDamage} damage (Combo x{currentComboCount})");
            }
        }
    }

    /// <summary>
    /// Detect all targets within melee range using sphere overlap and angle check.
    /// </summary>
    private List<Actor> DetectMeleeTargets(Vector3 origin, Vector3 forward)
    {
        List<Actor> hitActors = new List<Actor>();
        
        // Sphere overlap to find potential targets
        Collider[] colliders = Physics.OverlapSphere(origin, weaponData.weaponRange, attackLayers);
        
        foreach (Collider col in colliders)
        {
            // Skip if this is the player
            if (col.transform.root == MyController.transform) continue;
            
            // Check if within swing angle
            Vector3 directionToTarget = (col.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(forward, directionToTarget);
            
            if (angleToTarget <= weaponData.meleeSwingAngle / 2f)
            {
                // Try to get Actor component
                Actor actor = col.GetComponentInParent<Actor>();
                
                if (actor != null && !hitActors.Contains(actor))
                {
                    hitActors.Add(actor);
                }
            }
        }
        
        return hitActors;
    }

    /// <summary>
    /// Apply knockback force to hit target.
    /// </summary>
    private void ApplyKnockback(Actor target, Vector3 attackDirection)
    {
        if (weaponData.knockbackForce <= 0f) return;
        
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 knockbackForce = attackDirection * weaponData.knockbackForce;
            targetRb.AddForce(knockbackForce, ForceMode.Impulse);
        }
    }
    #endregion

    #region Combo System
    /// <summary>
    /// Get the appropriate animation for current combo count.
    /// </summary>
    private string GetComboAnimation()
    {
        return currentComboCount switch
        {
            1 => ANIM_ATTACK_1,
            2 => ANIM_ATTACK_2,
            3 => ANIM_ATTACK_3,
            _ => ANIM_ATTACK_1
        };
    }

    /// <summary>
    /// Reset combo counter.
    /// </summary>
    private void ResetCombo()
    {
        currentComboCount = 0;
    }
    #endregion

    #region Blocking System
    /// <summary>
    /// Start blocking incoming attacks.
    /// </summary>
    private void StartBlocking()
    {
        if (!weaponData.canBlock) return;
        if (isAttacking) return; // Can't block while attacking
        
        isBlocking = true;
        
        // Play block animation
        ChangeAnimationState(ANIM_BLOCK);
        RequestPlayerAnimation("Block"); // change to player controller set animation to block
        
        Debug.Log("Started blocking");
    }

    /// <summary>
    /// Stop blocking.
    /// </summary>
    private void StopBlocking()
    {
        if (!isBlocking) return;
        
        isBlocking = false;
        
        // Return to idle
        ChangeAnimationState(ANIM_IDLE);
        RequestPlayerAnimation("BlockEnd");
        
        Debug.Log("Stopped blocking");
    }

    /// <summary>
    /// Called when blocking an incoming attack.
    /// Returns the damage reduction amount.
    /// </summary>
    public float GetBlockDamageReduction()
    {
        if (!isBlocking) return 0f;
        return weaponData.blockDamageReduction;
    }

    /// <summary>
    /// Check if currently blocking and facing the attack direction.
    /// </summary>
    public bool IsBlockingDirection(Vector3 attackDirection)
    {
        if (!isBlocking) return false;
        
        Vector3 forward = GetAimDirection();
        float angle = Vector3.Angle(forward, -attackDirection);
        
        // Block if attack is coming from front 120 degree arc
        return angle <= 60f;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Melee weapons don't use traditional ammo.
    /// </summary>
    public override bool CanAttack()
    {
        if (!base.IsReady) return false;
        if (isAttacking) return false;
        if (isReloading) return false;
        if (isBlocking) return false;
        if (weaponData == null) return false;
        
        // Check cooldown
        if (Time.time < lastAttackTime + weaponData.fireRate) return false;
        
        // Melee can check stamina here if you have a stamina system
        // if (playerStamina < weaponData.staminaCost) return false;
        
        return true;
    }

    /// <summary>
    /// Melee weapons don't reload in the traditional sense.
    /// Could be repurposed for sharpening or stamina recovery.
    /// </summary>
    public override void Reload()
    {
        // Melee weapons don't need to reload
        // Could play a "sharpen" animation or restore stamina
        Debug.Log("Melee weapons don't reload");
    }

    protected override void InitializeAmmo()
    {
        // Melee weapons don't use ammo
        currentAmmo = 0;
    }
    #endregion

    #region Debug Visualization
    protected override void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        base.OnDrawGizmos();
        
        if (weaponData != null && attackPoint != null)
        {
            Vector3 origin = attackPoint.position;
            Vector3 forward = attackPoint.forward;
            
            // Draw attack range sphere
            Gizmos.color = isBlocking ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(origin, weaponData.weaponRange);
            
            // Draw swing angle cone
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            float halfAngle = weaponData.meleeSwingAngle / 2f;
            
            // Draw cone edges
            Vector3 rightEdge = Quaternion.Euler(0, halfAngle, 0) * forward * weaponData.weaponRange;
            Vector3 leftEdge = Quaternion.Euler(0, -halfAngle, 0) * forward * weaponData.weaponRange;
            
            Gizmos.DrawLine(origin, origin + rightEdge);
            Gizmos.DrawLine(origin, origin + leftEdge);
            
            // Draw arc
            Vector3 previousPoint = origin + rightEdge;
            for (int i = 1; i <= 10; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / 10f);
                Vector3 point = origin + Quaternion.Euler(0, angle, 0) * forward * weaponData.weaponRange;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
            
            // Show combo count
            if (Application.isPlaying && currentComboCount > 0)
            {
                UnityEditor.Handles.Label(origin + Vector3.up, $"Combo: {currentComboCount}");
            }
        }
    }
    #endregion
}