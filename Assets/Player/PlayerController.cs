using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Components
    [Header("Player Components")]
    [SerializeField] private PlayerInputReader playerInput;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    #endregion

    #region Camera
    [Header("Camera")]
    [SerializeField] private Transform playerCameraRoot;
    private CinemachineCamera mainCamera;

    public CinemachineCamera MainCinemachineCamera => mainCamera;
    public Transform CameraRoot => playerCameraRoot;
    #endregion

    #region Animation
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationTransitionTime = 0.2f;

    private string currentAnimationState;

    public Animator Animator => animator;
    #endregion

    #region Weapon System
    [Header("Weapon System")]
    [SerializeField] private int maxWeaponSlots = 3;

    private int currentWeaponIndex = 0;
    #endregion

    #region Cached Input Values
    private Vector2 moveDirection;
    private bool isSprinting;
    private bool isCrouching;
    private bool isJumping;
    private bool isDodging;
    private bool isPrimaryActionPressed;
    private bool isSecondaryActionPressed;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeCursor();
        InitializeComponents();
    }

    private void Update()
    {
        CacheInputValues();
        HandleWeaponSwitching();



        playerMovement.CheckGround();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }
    #endregion

    #region Initialization
    private void InitializeCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void InitializeComponents()
    {
        // Get components if not assigned in inspector
        if (playerInput == null) playerInput = GetComponent<PlayerInputReader>();
        if (playerCombat == null) playerCombat = GetComponent<PlayerCombat>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (mainCamera == null) mainCamera = GetComponentInChildren<CinemachineCamera>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
    #endregion

    #region Input Handling
    private void CacheInputValues()
    {
        moveDirection = playerInput.move;

        isJumping = playerInput.jump;
        isSprinting = playerInput.sprint;
        isCrouching = playerInput.crouch;
        isDodging = playerInput.dodge;

        isPrimaryActionPressed = playerInput.primaryFire;
        isSecondaryActionPressed = playerInput.secondaryFire;
    }

    private void HandleWeaponSwitching()
    {
        // Use consume methods to prevent rapid switching
        if (playerInput.ConsumePreviousInput())
        {
            SwitchWeapon(-1);
        }

        if (playerInput.ConsumeNextInput())
        {
            SwitchWeapon(1);
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        playerMovement.Move(moveDirection, mainCamera, isDodging, isSprinting);
        playerMovement.Crouch(isCrouching, playerCameraRoot);
        playerMovement.Jump(isJumping);
        playerMovement.Dodge(isDodging, isJumping, moveDirection, mainCamera.transform);
    }
    #endregion

    #region Weapon System
    private void SwitchWeapon(int direction)
    {
        currentWeaponIndex += direction;

        // Wrap around using proper modulo for negative numbers
        if (currentWeaponIndex < 0)
        {
            currentWeaponIndex = maxWeaponSlots - 1;
        }
        else if (currentWeaponIndex >= maxWeaponSlots)
        {
            currentWeaponIndex = 0;
        }

        Debug.Log($"Switched to weapon slot: {currentWeaponIndex}");

        playerCombat.SwitchWeapon(currentWeaponIndex);

        // TODO: Notify PlayerCombat to equip the weapon at currentWeaponIndex
        // playerCombat.EquipWeaponAtIndex(currentWeaponIndex);
    }

    public int GetCurrentWeaponIndex() => currentWeaponIndex;
    #endregion

    #region Animation Handling
    /// <summary>
    /// Changes the current animation state with smooth transition.
    /// Prevents the same animation from interrupting itself.
    /// </summary>
    /// <param name="newAnimationState">Name of the animation state to play</param>
    public void ChangeAnimationState(string newAnimationState)
    {
        if (currentAnimationState == newAnimationState) return;

        currentAnimationState = newAnimationState;
        animator.CrossFade(newAnimationState, animationTransitionTime);
    }

    /// <summary>
    /// Changes animation state with custom transition time.
    /// </summary>
    public void ChangeAnimationState(string newAnimationState, float transitionTime)
    {
        if (currentAnimationState == newAnimationState) return;

        currentAnimationState = newAnimationState;
        animator.CrossFade(newAnimationState, transitionTime);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Get the forward direction of the camera (useful for aiming).
    /// </summary>
    public Vector3 CameraForward => mainCamera != null ? mainCamera.transform.forward : transform.forward;

    /// <summary>
    /// Get the camera's Y rotation (useful for character rotation).
    /// </summary>
    public float CameraYRotation => mainCamera != null ? mainCamera.transform.eulerAngles.y : 0f;
    #endregion
}