using System.Linq;
using UnityEngine;

public class FirearmController : WeaponController
{
    #region Firearm-Specific State
    [Header("Firearm Settings")]
    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private bool showDebugRays = true;

    private bool isAiming = false;

    public bool IsAiming => isAiming;
    #endregion

    #region Abstract Implementation
    protected override void OnEquip()
    {
        // Play equip animation
        ChangeAnimationState(ANIM_IDLE);
        RequestSinglePlayerAnimation(weaponData.IdlePlayerAnimation);

        Debug.Log($"Equipped firearm: {weaponData.name}");
    }

    public override void OnUnequip()
    {
        // Stop any ongoing actions
        isAiming = false;

        // Cancel pending reloads
        CancelInvoke(nameof(CompleteReload));

        Debug.Log($"Unequipped firearm: {weaponData.name}");
    }

    public override void PrimaryAttack()
    {
        // Validation checks
        if (!CanAttack()) return;

        // Check ammo and auto-reload if empty
        if (!HasAmmo)
        {
            Reload();
            return;
        }

        // Execute attack
        StartCooldown();
        ConsumeAmmo();

        // Fire based on weapon type
        switch (weaponData.type)
        {
            case WeaponType.Bullet:
                FireSingleShot();
                break;
            case WeaponType.Piercing:
                FirePiercingShot();
                break;
            default:
                Debug.LogWarning($"Firearm type {weaponData.type} not supported in FirearmController");
                break;
        }

        // Effects and animations
        PlayFireEffect();
        PlayFireSound();
        ChangeAnimationState(ANIM_FIRE);
        RequestSinglePlayerAnimation(weaponData.AttackPlayerAnimation.First());
        int randomIndex = Random.Range(0, weaponData.AttackPlayerAnimation.Count);
        string attackAnimation = weaponData.AttackPlayerAnimation[randomIndex];
        MyController.ChangeAnimationState(attackAnimation);

        // Fire event
        // OnWeaponFired?.Invoke();
    }

    public override void SecondaryAttack()
    {
        // // Toggle aim down sights
        // isAiming = !isAiming;

        // if (isAiming)
        // {
        //     RequestPlayerAnimation("AimStart");
        // }
        // else
        // {
        //     RequestPlayerAnimation("AimEnd");
        // }

        // Can add FOV changes, accuracy bonuses, etc. here
    }
    #endregion

    #region Firearm Attack Logic
    /// <summary>
    /// Fire a single bullet raycast (standard gun behavior).
    /// </summary>
    private void FireSingleShot()
    {
        Vector3 origin = GetAimOrigin();
        Vector3 direction = GetAimDirection();

        // Apply spread if not aiming
        if (!isAiming)
        {
            direction = ApplySpread(direction);
        }

        // Perform raycast
        if (Physics.Raycast(origin, direction, out RaycastHit hit, weaponData.weaponRange, hitLayers))
        {
            ProcessHit(hit);

            // Debug visualization
            if (showDebugRays)
            {
                Debug.DrawRay(origin, direction * hit.distance, Color.red, 1f);
            }
        }
        else
        {
            // Miss - still show debug ray
            if (showDebugRays)
            {
                Debug.DrawRay(origin, direction * weaponData.weaponRange, Color.yellow, 1f);
            }
        }
    }

    /// <summary>
    /// Fire a piercing shot that hits all targets in line (railgun, sniper).
    /// </summary>
    private void FirePiercingShot()
    {
        Vector3 origin = GetAimOrigin();
        Vector3 direction = GetAimDirection();

        // Get all hits along the ray
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, weaponData.weaponRange, hitLayers);

        if (hits.Length > 0)
        {
            // Sort by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // Process each hit
            foreach (RaycastHit hit in hits)
            {
                ProcessHit(hit);
            }

            // Debug visualization
            if (showDebugRays)
            {
                Debug.DrawRay(origin, direction * weaponData.weaponRange, Color.cyan, 1f);
            }
        }
        else
        {
            if (showDebugRays)
            {
                Debug.DrawRay(origin, direction * weaponData.weaponRange, Color.yellow, 1f);
            }
        }
    }

    /// <summary>
    /// Process a raycast hit - check for actors and apply damage.
    /// </summary>
    private void ProcessHit(RaycastHit hit)
    {
        // Spawn hit effect
        PlayHitEffect(hit.point, Quaternion.LookRotation(hit.normal));

        // Check if we hit an actor
        Actor actor = hit.transform.GetComponentInParent<Actor>();

        if (actor != null)
        {
            // Check for headshot
            bool isHeadshot = hit.transform.CompareTag("Head");

            // Calculate damage
            int baseDamage = CalculateDamage();
            int finalDamage = ApplyDamageModifiers(baseDamage, isHeadshot, hit.distance);

            // Apply damage
            DealDamage(actor, finalDamage, hit.point);

            // Log for debugging
            if (isHeadshot)
            {
                Debug.Log($"HEADSHOT! Dealt {finalDamage} damage to {actor.name}");
            }
        }
    }
    #endregion

    #region Accuracy & Spread
    /// <summary>
    /// Apply weapon spread/inaccuracy to the aim direction.
    /// </summary>
    private Vector3 ApplySpread(Vector3 direction)
    {
        // Simple cone spread - can be expanded with spread patterns
        float spread = isAiming ? 0.5f : 2f; // Less spread when aiming

        Vector3 spreadOffset = new Vector3(
            Random.Range(-spread, spread),
            Random.Range(-spread, spread),
            0f
        );

        return (direction + spreadOffset).normalized;
    }
    #endregion

    #region Automatic Fire Support
    /// <summary>
    /// Check if weapon should fire automatically.
    /// Call this in Update() if you want to handle automatic fire here.
    /// </summary>
    public bool ShouldFireAutomatically()
    {
        return weaponData.automatic && CanAttack() && HasAmmo;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Override damage calculation to add distance-based falloff.
    /// </summary>
    protected override int ApplyDamageModifiers(int baseDamage, bool isHeadshot = false, float distance = 0f)
    {
        float finalDamage = baseDamage;

        // Apply headshot multiplier
        if (isHeadshot)
        {
            finalDamage *= weaponData.headshotMultiplier;
        }

        // Apply distance falloff (optional - can be removed for hit-scan weapons)
        // Damage reduces to 50% at max range
        if (distance > 0f && weaponData.weaponRange > 0f)
        {
            float distanceFactor = 1f - (distance / weaponData.weaponRange) * 0.5f;
            finalDamage *= Mathf.Clamp01(distanceFactor);
        }

        return Mathf.RoundToInt(finalDamage);
    }

    /// <summary>
    /// Override to add weapon-specific checks.
    /// </summary>
    public override bool CanAttack()
    {
        if (!base.CanAttack()) return false;

        // Firearms need ammo to shoot
        if (!HasAmmo && weaponData.type != WeaponType.Melee) return false;

        return true;
    }
    #endregion

    #region Debug Visualization
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        // Draw weapon range
        if (weaponData != null && attackPoint != null)
        {
            Gizmos.color = isAiming ? Color.green : Color.yellow;
            Vector3 origin = attackPoint.position;
            Vector3 direction = attackPoint.forward;

            Gizmos.DrawRay(origin, direction * weaponData.weaponRange);
            Gizmos.DrawWireSphere(origin + direction * weaponData.weaponRange, 0.2f);
        }
    }
    #endregion
}