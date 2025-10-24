using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponType { Melee, Bullet, Piercing, Explosive }

[CreateAssetMenu(menuName = "Scriptable Objects/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Player Animation")]

    public string IdlePlayerAnimation;
    public List<string> AttackPlayerAnimation;
    public string BlockPlayerAnimation;
    public string ReloadPlayerAnimation;

    [Header("Weapon")]
    public WeaponType type;
    public bool automatic = true;
    public int maxAmmo = 4;
    public float fireRate = 0.5f;
    public int attackDamage = 1;
    public float reloadTime = 0.5f;
    public float headshotMultiplier = 1.5f;
    public float weaponRange = 20f;

    [Header("Muzzle")]
    public GameObject fireEffect;
    public AudioClip fireSound;

    [Header("Raycast")]
    public GameObject hitEffect;

    [Header("Melee Settings")]
    [Tooltip("Angle of the melee attack cone")]
    public float meleeSwingAngle = 90f;
    [Tooltip("Duration of melee attack animation")]
    public float meleeSwingDuration = 0.3f;
    [Tooltip("Can this melee weapon block incoming attacks?")]
    public bool canBlock = false;
    [Tooltip("Damage reduction percentage when blocking (0-1)")]
    public float blockDamageReduction = 0.5f;
    [Tooltip("Stamina cost per melee attack")]
    public float staminaCost = 10f;
    [Tooltip("Knockback force applied to hit enemies")]
    public float knockbackForce = 5f;

    [Header("Explosive Settings")]
    [Tooltip("Explosion radius in units")]
    public float explosionRadius = 5f;
    [Tooltip("Damage falloff over distance (curve from center to edge)")]
    public AnimationCurve explosionDamageFalloff = AnimationCurve.Linear(0, 1, 1, 0.3f);
    [Tooltip("Force applied to rigidbodies in explosion radius")]
    public float explosionForce = 700f;
    [Tooltip("Upward modifier for explosion force")]
    public float explosionUpwardModifier = 1f;
    [Tooltip("Time before projectile explodes (0 = on impact)")]
    public float fuseTime = 0f;
    [Tooltip("Does this explosive stick to surfaces?")]
    public bool isSticky = false;
    [Tooltip("Explosion effect prefab")]
    public GameObject explosionEffect;
    [Tooltip("Explosion sound")]
    public AudioClip explosionSound;
    [Tooltip("Can damage the player who fired it?")]
    public bool canSelfDamage = true;
    [Tooltip("Arc trajectory for thrown/launched explosives")]
    public float projectileArc = 0.5f;

    [Header("Prefab")]
    public WeaponController weaponPrefab;
}