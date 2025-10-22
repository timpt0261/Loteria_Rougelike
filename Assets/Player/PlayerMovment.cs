using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Components
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private CapsuleCollider capsuleCollider;
    #endregion

    #region Camera Settings
    [Header("Camera")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 80f;
    #endregion

    #region Ground Detection
    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundedLayers;
    [SerializeField] private float groundedOffset = 0.95f;
    [SerializeField] private float groundedRadius = 0.28f;

    private bool isGrounded;
    private Vector3 groundCheckPosition;

    public bool IsGrounded => isGrounded;
    #endregion

    #region Movement Stats
    [Header("Movement Stats")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpHeight = 5f;
    [SerializeField] private float dodgeDistance = 3f;
    #endregion

    #region Crouch Settings
    [Header("Crouch Settings")]
    [SerializeField] private float walkHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float capsuleWalkHeight = 2f;
    [SerializeField] private float capsuleCrouchHeight = 1f;
    [SerializeField] private Vector3 walkCenter = Vector3.zero;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, -0.5f, 0);
    [SerializeField] private float cameraTransitionSpeed = 3f;

    private float cameraVelocity;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rigidBody = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }
    #endregion

    #region Public Methods
    public void CheckGround()
    {
        groundCheckPosition = new Vector3(
            transform.position.x,
            transform.position.y - groundedOffset,
            transform.position.z
        );
        isGrounded = Physics.CheckSphere(groundCheckPosition, groundedRadius, groundedLayers);
    }

    public void Move(Vector2 moveDirection, CinemachineCamera mainCamera, bool isDodging, bool isSprinting)
    {
        if (isDodging) return;

        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        if (moveDirection == Vector2.zero) targetSpeed = 0f;

        // Update camera FOV based on sprint state
        float targetFOV = isSprinting ? sprintFOV : normalFOV;
        mainCamera.Lens.FieldOfView = Mathf.Lerp(
            mainCamera.Lens.FieldOfView,
            targetFOV,
            Time.fixedDeltaTime
        );

        // Calculate movement direction relative to camera
        Vector3 inputDirection = new Vector3(moveDirection.x, 0f, moveDirection.y);
        Vector3 targetDirection = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f) * inputDirection;

        // Apply movement
        rigidBody.MovePosition(
            rigidBody.position + targetDirection.normalized * targetSpeed * Time.fixedDeltaTime
        );
    }

    public void Jump(bool isJumping)
    {
        if (isGrounded && isJumping)
        {
            float jumpForce = Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
            rigidBody.AddForce(Vector3.up * jumpForce * 100 * Time.fixedDeltaTime, ForceMode.Impulse);
        }


    }

    public void Dodge(bool isDodging, bool isJumping, Vector2 moveDirection, Transform mainCameraTransform)
    {
        if (!isDodging) return;

        // Calculate dodge force
        float force = isJumping ? dodgeDistance / 2f : dodgeDistance;
        float dodgeForce = Mathf.Sqrt(force * -2 * Physics.gravity.y);

        // Calculate dodge direction relative to camera
        Vector3 inputDirection = new Vector3(moveDirection.x, 0f, moveDirection.y);
        Vector3 dodgeDirection = Quaternion.Euler(0f, mainCameraTransform.eulerAngles.y, 0f) * inputDirection;

        // If no input, dodge forward relative to camera
        if (moveDirection == Vector2.zero)
            dodgeDirection = mainCameraTransform.forward;
        
        // Apply dodge impulse
        rigidBody.linearDamping = 0f;
        rigidBody.AddForce(dodgeDirection * dodgeForce * Time.fixedDeltaTime, ForceMode.Impulse);

        Debug.Log($"Normalized Dodge Direction: {dodgeDirection * dodgeForce}");
    }

    public void Crouch(bool isCrouching, Transform playerCameraRoot)
    {
        // Update capsule collider dimensions
        capsuleCollider.height = isCrouching ? capsuleCrouchHeight : capsuleWalkHeight;
        capsuleCollider.center = isCrouching ? crouchCenter : walkCenter;

        // Smooth camera height transition
        float currentHeight = playerCameraRoot.localPosition.y;
        float targetCameraHeight = isCrouching ? crouchHeight : walkHeight;

        if (currentHeight == targetCameraHeight) return;

        float smoothedHeight = Mathf.SmoothDamp(
            currentHeight,
            targetCameraHeight,
            ref cameraVelocity,
            Time.fixedDeltaTime * cameraTransitionSpeed
        );

        playerCameraRoot.localPosition = new Vector3(
            playerCameraRoot.localPosition.x,
            smoothedHeight,
            playerCameraRoot.localPosition.z
        );
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPosition, groundedRadius);
    }
    #endregion
}