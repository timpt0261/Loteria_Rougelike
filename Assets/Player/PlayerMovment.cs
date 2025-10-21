using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float sprintFOV = 80f;


    [SerializeField] private Rigidbody m_rigidBody;
    [SerializeField] private CapsuleCollider m_capsuleCollider;

    [Header("Jump and Gravity")]
    [SerializeField] private bool m_isGrounded;
    public bool IsGrounded => m_isGrounded;

    [SerializeField] private float m_groundedOffset = .95f;
    [SerializeField] private float m_groundedRadius = .28f;
    [SerializeField] private LayerMask m_groundedLayers;



    [Header("Crouch")]
    [SerializeField] private float _walkHeight = 2;
    [SerializeField] private float _crouchHeight = 1;

    [SerializeField] private float _capsuleWalkHeight = 2;
    [SerializeField] private float _capsuleCrouchHeight = 1;


    [SerializeField] private Vector3 _walkCenter = Vector3.zero;
    [SerializeField] private Vector3 _crouchCenter = new Vector3(0, -0.5f, 0);
    [SerializeField] private float _cameraTransitionSpeed = 3f;
    private float _cameraVelocity;



    [Header("Movement Stats")]
    [SerializeField] private float _JumpHeight = 5f;
    [SerializeField] private float _WalkSpeed = 3f;
    [SerializeField] private float _SprintSpeed = 10f;
    [SerializeField] private float _DodgeDistance = 3f;
    private Vector3 spherePosition;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        m_rigidBody = GetComponent<Rigidbody>();
        m_capsuleCollider = GetComponent<CapsuleCollider>();


    }

    public void CheckGround()
    {
        spherePosition = new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z);
        m_isGrounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundedLayers);
    }

    public void Move(Vector2 _moveDirection, CinemachineCamera mainCamera, bool _IsDodging, bool _isSprinting)
    {
        if (_IsDodging) return;
        float targetSpeed = _isSprinting ? _SprintSpeed : _WalkSpeed;
        if (_moveDirection == Vector2.zero) targetSpeed = 0;

        float targetFOV = _isSprinting ? sprintFOV : normalFOV;
        mainCamera.Lens.FieldOfView = Mathf.Lerp(a: mainCamera.Lens.FieldOfView, b: targetFOV, Time.fixedDeltaTime);

        Vector3 inputDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        Vector3 targetDirection = Quaternion.Euler(0.0f, mainCamera.transform.eulerAngles.y, 0.0f) * inputDirection;
        m_rigidBody.MovePosition(m_rigidBody.position + targetDirection.normalized * targetSpeed * Time.fixedDeltaTime);

    }

    public void Jump(bool _isJumping)
    {
        // if on the ground can jump
        // if in air can jump while stamina is still full 
        // while in air termin velocity is slower than forces tha pushes down 
        if (m_isGrounded && _isJumping)
        {
            m_rigidBody.AddForce(Vector3.up * _JumpHeight * 100 * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    public void Dodge(bool _IsDodging, bool _isJumping, Vector2 _moveDirection, Transform mainCameraTransform)
    {
        if (!_IsDodging) return;
        float force = _isJumping ? _DodgeDistance / 2f : _DodgeDistance;
        float dodgeForce = Mathf.Sqrt(force * -2 * Physics.gravity.y);
        Vector3 inputDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        Vector3 dodgeDirection = Quaternion.Euler(0.0f, mainCameraTransform.eulerAngles.y, 0.0f) * inputDirection;
        if (_moveDirection == Vector2.zero)
        {
            dodgeDirection = mainCameraTransform.transform.forward;
        }

        m_rigidBody.linearDamping = 0;

        // Apply dodge impulse
        // m_rigidBody.AddForce(new Vector3(0f, _JumpHeight / 2f, 0f), ForceMode.Impulse);
        Debug.Log($" Normalized Dodge Direction: {dodgeDirection * dodgeForce}");

        m_rigidBody.AddForce(dodgeDirection * dodgeForce * Time.fixedDeltaTime, ForceMode.Impulse);

        // Prevent continuous dodging
        _IsDodging = false;
    }



    public void Crouch(bool _isCrouching, Transform playerCameraRoot)
    {
        // if not grounded --> slam 

        m_capsuleCollider.height = _isCrouching ? _capsuleCrouchHeight : _capsuleWalkHeight;
        m_capsuleCollider.center = _isCrouching ? _crouchCenter : _walkCenter;

        // Smooth interpolation for camera position
        float currentHeight = playerCameraRoot.localPosition.y;
        float _targetCameraHeight = _isCrouching ? _crouchHeight : _walkHeight;
        if (currentHeight == _targetCameraHeight) return;
        float smoothedHeight = Mathf.SmoothDamp(currentHeight, _targetCameraHeight, ref _cameraVelocity, Time.fixedDeltaTime * _cameraTransitionSpeed);

        playerCameraRoot.localPosition = new Vector3(
            playerCameraRoot.localPosition.x,
            smoothedHeight,
            playerCameraRoot.localPosition.z

        );
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(spherePosition, m_groundedRadius);
    }

}
