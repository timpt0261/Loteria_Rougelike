using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour
{
    [Header("Player Component")]
    [SerializeField] private PlayerInputReader m_playerInput;
    [SerializeField] private PlayerMovement m_playerMovement;
    [SerializeField] private PlayerCombat m_playerCombat;

    [Header("Cinemachine")]
    [SerializeField] private Transform m_playerCameraRoot;
    private CinemachineCamera m_mainCamera;
    public CinemachineCamera MainCinemachineCamera => m_mainCamera;


    [Header("Animation")]

    [SerializeField] private Animator m_animator;
    public Animator Animator => m_animator;


    // Input Reader Actions
    private Vector2 _moveDirection;
    private bool _IsSprinting;
    private bool _IsCrouching;
    private bool _IsJumping;
    private bool _IsDodging;
    private bool _IsPrimaryActionPressed;
    private bool _IsSecondaryActionPressed;
    private bool _IsNextWeaponSelected;
    private bool _IsPreviousWeaponSelected;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_playerInput = GetComponent<PlayerInputReader>();
        m_playerCombat = GetComponent<PlayerCombat>();
        m_playerMovement = GetComponent<PlayerMovement>();

        m_mainCamera = GetComponentInChildren<CinemachineCamera>();
        m_animator = GetComponentInChildren<Animator>();


    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = m_playerInput.move;

        // jumping , sprinting, crouching, dodging
        _IsJumping = m_playerInput.jump;
        _IsSprinting = m_playerInput.sprint;
        _IsCrouching = m_playerInput.crouch;
        _IsDodging = m_playerInput.dodge;

        // primary and secondary action
        _IsPrimaryActionPressed = m_playerInput.primary_fire;
        _IsSecondaryActionPressed = m_playerInput.secondary_fire;

        // previous and next weapon selection
        _IsPreviousWeaponSelected = m_playerInput.previous;
        _IsNextWeaponSelected = m_playerInput.next;


        m_playerMovement.CheckGround();

    }



    void LateUpdate()
    {

    }


    void FixedUpdate()
    {
        this.m_playerMovement.Move(_moveDirection, m_mainCamera, _IsDodging, _IsSprinting);
        this.m_playerMovement.Crouch(_IsCrouching, m_playerCameraRoot);
        this.m_playerMovement.Jump(_IsJumping);
        this.m_playerMovement.Dodge(_IsDodging, _IsJumping, _moveDirection, m_mainCamera.transform);

    }
}
