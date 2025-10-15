using UnityEngine;
using UnityEngine.Assertions.Must;

public class PlayerCombat : MonoBehaviour
{
	[SerializeField] private PlayerMovement m_player_movement;
	[SerializeField] private PlayerInputReader m_player_input;

	// Animation Handling
	[SerializeField] private Animator m_player_animator;
	[SerializeField] private AudioSource m_audio_source;

	private const string idle_animation = "Idle";
	private const string melee_attack_01 = "Melee Attack 1";
	private const string melee_attack_02 = "Melee Attack 2";
	private const string melee_block_00 = "Melee Block";

	private string currentAnimationState;


	[Header("Melee")]
	[SerializeField] private float attackDistance = 3f;
	[SerializeField] private float attackDelay = 0.4f;
	[SerializeField] private float attackSpeed = 2f;
	[SerializeField] private int attackDamage = 10;
	[SerializeField] private LayerMask attackLayer;

	[SerializeField] private GameObject hitEffect;
	[SerializeField] private AudioClip swordSwingSFX;
	[SerializeField] private AudioClip hitSFX;

	private bool isAttacking = false;
	private bool readyToAttack = true;

	private int attackCount;

	// Input Handling
	private bool primary;
	private bool secondary;

	public void Start()
	{
		m_player_input = GetComponent<PlayerInputReader>();
		m_player_movement = GetComponent<PlayerMovement>();
		m_player_animator = GetComponent<Animator>();
		m_audio_source = GetComponent<AudioSource>();
	}
	void Update()
	{
		primary = m_player_input.primary_fire;
		secondary = m_player_input.secondary_fire;

		if (secondary) { Attack(); }

		SetAnimations();
	}

	public void Attack()
	{
		if (!readyToAttack || isAttacking) return;

		readyToAttack = false;
		isAttacking = true;

		Invoke(nameof(ResetAttack), attackSpeed);
		Invoke(nameof(AttackRayCast), attackDelay);

		m_audio_source.pitch = Random.Range(0.9f, 1.1f);
		m_audio_source.PlayOneShot(swordSwingSFX);

		if (attackCount == 0)
		{
			ChangeAnimationState(melee_attack_01);
			attackCount++;
		}
		else
		{
			ChangeAnimationState(melee_attack_02);
			attackCount = 0;
		}

	}

	private void ResetAttack()
	{
		isAttacking = false;
		readyToAttack = true;
	}

	private void AttackRayCast()
	{
		var main_camera = m_player_movement.CinemachineCamera;
		if (Physics.Raycast(main_camera.transform.position, main_camera.transform.forward, out RaycastHit hitInfo, attackDistance, attackLayer))
		{
			HitTarget(hitInfo.point);

			// handle actor getting hit
			if (hitInfo.transform.TryGetComponent<Actor>(out Actor actor))
			{
				actor.TakeDamage(attackDamage);
			}
		}
	}

	void HitTarget(Vector3 pos)
	{
		m_audio_source.pitch = 1;
		m_audio_source.PlayOneShot(hitSFX);

		GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
		Destroy(GO, 20);
	}

	private void SetAnimations()
	{
		if (isAttacking) return;

		ChangeAnimationState(idle_animation);

	}

	public void ChangeAnimationState(string newState)
	{
		// STOP THE SAME ANIMATION FROM INTERRUPTING WITH ITSELF //
		if (currentAnimationState == newState) return;

		// PLAY THE ANIMATION //
		currentAnimationState = newState;
		m_player_animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, attackDistance);
	}

}