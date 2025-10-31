using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class WeaponController : MonoBehaviour
{
    #region Events
    public UnityEvent OnWeaponFired;
    public UnityEvent<int, int> OnAmmoChanged; // currentAmmo, maxAmmo
    public UnityEvent OnReloadStarted;
    public UnityEvent OnReloadCompleted;
    public UnityEvent<string> OnAnimationRequested; // Request player body animation
    #endregion

    #region Weapon Data
    [Header("Weapon Data")]
    [SerializeField] protected WeaponData weaponData;

    public WeaponData Data => weaponData;
    #endregion

    #region Component References
    [Header("Component References")]
    public PlayerController MyController { get; set; }

    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected Animator weaponAnimator;
    [SerializeField] protected AudioSource audioSource;

    public Transform AttackPoint => attackPoint;
    #endregion

    #region Animation
    [Header("Animation Settings")]
    [SerializeField] protected float animationTransitionTime = 0.2f;

    protected string currentAnimationState;

    // Common animation state names
    protected const string ANIM_IDLE = "Idle";
    protected const string ANIM_FIRE = "Fire";
    protected const string ANIM_RELOAD = "Reload";
    #endregion

    #region Weapon State
    [Header("Weapon State")]
    protected bool isReady = true;
    protected bool isAttacking = false;
    protected bool isReloading = false;
    protected float lastAttackTime = 0f;

    public bool IsReady => isReady;
    public bool IsAttacking => isAttacking;
    public bool IsReloading => isReloading;
    public float CooldownRemaining => Mathf.Max(0f, (lastAttackTime + weaponData.fireRate) - Time.time);
    #endregion

    #region Ammo System
    [Header("Ammo Information")]
    protected int currentAmmo;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => weaponData.maxAmmo;
    public bool HasAmmo => currentAmmo > 0;
    public string AmmoPercentage => weaponData.maxAmmo > 0 ? $"{CurrentAmmo} / {MaxAmmo}" : $"{Mathf.Infinity}";
    #endregion

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        ValidateComponents();
    }

    protected virtual void Update()
    {
        UpdateCooldown();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize the weapon with owner reference and setup components.
    /// Called by PlayerCombat when weapon is equipped.
    /// </summary>
    public virtual void Initialize(PlayerController owner)
    {
        MyController = owner;

        ValidateComponents();
        InitializeAmmo();

        isReady = true;
        isAttacking = false;
        isReloading = false;

        OnEquip();
    }

    /// <summary>
    /// Validates and caches component references.
    /// </summary>
    protected virtual void ValidateComponents()
    {
        if (weaponData == null)
        {
            Debug.LogError($"WeaponData is not assigned on {gameObject.name}!");
            return;
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (weaponAnimator == null)
            weaponAnimator = GetComponentInChildren<Animator>();

        if (attackPoint == null)
        {
            // Try to find it by name
            Transform foundPoint = transform.Find("HitPoint");
            if (foundPoint != null)
                attackPoint = foundPoint;
            else
                attackPoint = transform; // Fallback to weapon root
        }
    }

    /// <summary>
    /// Initialize ammo to maximum capacity.
    /// </summary>
    protected virtual void InitializeAmmo()
    {
        currentAmmo = weaponData.maxAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, weaponData.maxAmmo);
    }
    #endregion

    #region Abstract Methods (Must Be Implemented)
    /// <summary>
    /// Primary attack implementation (shoot, swing, throw).
    /// </summary>
    public abstract void PrimaryAttack();

    /// <summary>
    /// Secondary attack implementation (ADS, block, cook grenade).
    /// </summary>
    public abstract void SecondaryAttack();

    /// <summary>
    /// Called when weapon is equipped.
    /// </summary>
    protected abstract void OnEquip();

    /// <summary>
    /// Called when weapon is unequipped.
    /// </summary>
    public abstract void OnUnequip();
    #endregion

    #region Attack Validation
    /// <summary>
    /// Check if weapon can currently attack.
    /// </summary>
    public virtual bool CanAttack()
    {
        if (weaponData == null) return false;
        if (!isReady) return false;
        if (isAttacking) return false;
        if (isReloading) return false;
        if (Time.time < lastAttackTime + weaponData.fireRate) return false;

        return true;
    }

    /// <summary>
    /// Check if weapon can reload.
    /// </summary>
    public virtual bool CanReload()
    {
        if (isReloading) return false;
        if (currentAmmo >= weaponData.maxAmmo) return false;
        if (isAttacking) return false;

        return true;
    }
    #endregion

    #region Ammo Management
    /// <summary>
    /// Consume one unit of ammo.
    /// </summary>
    protected virtual void ConsumeAmmo()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            OnAmmoChanged?.Invoke(currentAmmo, weaponData.maxAmmo);
        }
    }

    /// <summary>
    /// Reload the weapon to full capacity.
    /// </summary>
    public virtual void Reload()
    {
        if (!CanReload()) return;

        isReloading = true;
        isReady = false;

        OnReloadStarted?.Invoke();

        // Play reload animation
        if (weaponAnimator != null)
        {
            ChangeAnimationState(ANIM_RELOAD);
        }
        MyController.ChangeAnimationState(weaponData.ReloadPlayerAnimation);

        // Request player body animation
        OnAnimationRequested?.Invoke("Reload");

        // Schedule reload completion
        Invoke(nameof(CompleteReload), weaponData.reloadTime);
    }

    /// <summary>
    /// Complete the reload process.
    /// </summary>
    protected virtual void CompleteReload()
    {
        currentAmmo = weaponData.maxAmmo;
        isReloading = false;
        isReady = true;

        OnAmmoChanged?.Invoke(currentAmmo, weaponData.maxAmmo);
        OnReloadCompleted?.Invoke();
    }
    #endregion

    #region Cooldown System
    /// <summary>
    /// Start attack cooldown.
    /// </summary>
    protected virtual void StartCooldown()
    {
        lastAttackTime = Time.time;
        isReady = false;
        isAttacking = true;
    }

    /// <summary>
    /// Update cooldown state each frame.
    /// </summary>
    protected virtual void UpdateCooldown()
    {
        if (!isReady && !isReloading && Time.time >= lastAttackTime + weaponData.fireRate)
        {
            ResetCooldown();
        }
    }

    /// <summary>
    /// Reset cooldown and make weapon ready.
    /// </summary>
    protected virtual void ResetCooldown()
    {
        isReady = true;
        isAttacking = false;
    }
    #endregion

    #region Effect Management
    /// <summary>
    /// Play weapon fire visual effect.
    /// </summary>
    protected virtual void PlayFireEffect()
    {
        if (weaponData.fireEffect != null && attackPoint != null)
        {
            GameObject effect = Instantiate(weaponData.fireEffect, attackPoint.position, attackPoint.rotation, attackPoint);
            Destroy(effect, 2f);
        }
    }

    /// <summary>
    /// Play hit effect at specified position.
    /// </summary>
    protected virtual void PlayHitEffect(Vector3 hitPosition, Quaternion rotation)
    {
        if (weaponData.hitEffect != null)
        {
            GameObject hitVFX = Instantiate(weaponData.hitEffect, hitPosition, rotation);
            Destroy(hitVFX, 10f);
        }
    }

    /// <summary>
    /// Play weapon fire sound with pitch variation.
    /// </summary>
    protected virtual void PlayFireSound()
    {
        if (audioSource != null && weaponData.fireSound != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(weaponData.fireSound);
        }
    }

    /// <summary>
    /// Play a specific audio clip.
    /// </summary>
    protected virtual void PlaySound(AudioClip clip, float pitchVariation = 0.1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(1f - pitchVariation, 1f + pitchVariation);
            audioSource.PlayOneShot(clip);
        }
    }
    #endregion

    #region Damage System
    /// <summary>
    /// Calculate base damage from weapon data.
    /// </summary>
    protected virtual int CalculateDamage()
    {
        return weaponData.attackDamage;
    }

    /// <summary>
    /// Apply damage modifiers (headshot, distance, etc.).
    /// </summary>
    protected virtual int ApplyDamageModifiers(int baseDamage, bool isHeadshot = false, float distance = 0f)
    {
        float finalDamage = baseDamage;

        // Apply headshot multiplier
        if (isHeadshot)
        {
            finalDamage *= weaponData.headshotMultiplier;
        }

        // Can add distance falloff here for firearms

        return Mathf.RoundToInt(finalDamage);
    }

    /// <summary>
    /// Deal damage to target actor.
    /// </summary>
    protected virtual void DealDamage(Actor target, int damage, Vector3 hitPoint)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
            PlayHitEffect(hitPoint, Quaternion.identity);
        }
    }
    #endregion

    #region Animation Handling
    /// <summary>
    /// Change weapon model animation state with smooth transition.
    /// </summary>
    public virtual void SetAnimationState()
    {
        if (!isAttacking)
        {
            ChangeAnimationState(ANIM_IDLE);
            MyController.ChangeAnimationState(weaponData.IdlePlayerAnimation);
        }


    }
    protected virtual void ChangeAnimationState(string newAnimationState)
    {
        if (weaponAnimator == null) return;
        if (currentAnimationState == newAnimationState) return;

        currentAnimationState = newAnimationState;
        weaponAnimator.CrossFade(newAnimationState, animationTransitionTime);
    }

    /// <summary>
    /// Request player body animation change.
    /// </summary>
    protected virtual void RequestSinglePlayerAnimation(string animationName)
    {
        MyController.ChangeAnimationState(animationName);
    }

    protected virtual void RequestRandomPlayerAnimation(List<string> animationName)
    {
        int randomIndex = UnityEngine.Random.Range(0, animationName.Count);
        string attackAnimation = weaponData.AttackPlayerAnimation[randomIndex];
        MyController.ChangeAnimationState(attackAnimation);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Get the direction the weapon is aiming based on camera.
    /// </summary>
    protected virtual Vector3 GetAimDirection()
    {
        if (MyController != null && MyController.MainCinemachineCamera != null)
        {
            return MyController.MainCinemachineCamera.transform.forward;
        }

        return transform.forward;
    }

    /// <summary>
    /// Get the origin point for attacks (usually camera position for firearms).
    /// </summary>
    protected virtual Vector3 GetAimOrigin()
    {
        if (MyController != null && MyController.MainCinemachineCamera != null)
        {
            return MyController.MainCinemachineCamera.transform.position;
        }

        return attackPoint != null ? attackPoint.position : transform.position;
    }

    /// <summary>
    /// Check if target is within weapon range.
    /// </summary>
    protected virtual bool IsWithinRange(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(GetAimOrigin(), targetPosition);
        return distance <= weaponData.weaponRange;
    }
    #endregion

    #region Debug
    protected virtual void OnDrawGizmos()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, 0.1f);

            if (weaponData != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(attackPoint.position, attackPoint.forward * weaponData.weaponRange);
            }
        }
    }
    #endregion
}