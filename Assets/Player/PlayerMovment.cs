using Unity.Cinemachine;
using UnityEngine;


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputReader m_playerInput;
    [SerializeField] private Rigidbody m_rigidBody;
    [SerializeField] private CapsuleCollider m_capsuleCollider;

    [Header("Camera")]

    [SerializeField] private Transform m_playerCameraRoot;
    [SerializeField] private CinemachineCamera m_playerCamera;

    public CinemachineCamera CinemachineCamera => m_playerCamera;
    private const float _normalFOV = 60;
    private const float _sprintFOV = 80;

    private const float _targetPitchRange = 180;
    private const float _targetYawRange = 90;

    [Header("Jump and Gravity")]

    [SerializeField] private bool _isGrounded;
    [SerializeField] private float m_groundedOffset = .95f;
    [SerializeField] private float m_groundedRadius = .28f;
    [SerializeField] private LayerMask m_groundedLayers;

    private float Stamina = 100f;

    [Header("Crouch")]
    private const int _capsuleWalkHeight = 2;
    private const int _capsuleCrouchHeight = 1;

    [SerializeField] private float _walkHeight;
    [SerializeField] private float _crouchHeight;

    private Vector3 _walkCenter = Vector3.zero;
    private Vector3 _crouchCenter = new Vector3(0, -0.5f, 0);
    [SerializeField] private float _cameraVelocity;
    [SerializeField] private float _cameraTransitionSpeed = 3f;

    // inputs
    private Vector2 _moveDirection;
    private Vector2 _lookDirection;
    private bool _isSprinting;
    private bool _isCrouching;
    private bool _isJumping;

    private bool _IsDodging;

    [Header("Variables")]
    [SerializeField] private float _JumpHeight = 5f;
    [SerializeField] private float _WalkSpeed = 3f;
    [SerializeField] private float _SprintSpeed = 10f;
    [SerializeField] private float _DodgeDistance = 3f;
    private Vector3 spherePosition;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _walkHeight = m_playerCameraRoot.transform.position.y;
        _crouchHeight = _walkHeight / 2f;


    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = m_playerInput.move;
        _lookDirection = m_playerInput.look;

        _isJumping = m_playerInput.jump;
        _isSprinting = m_playerInput.sprint;
        _isCrouching = m_playerInput.crouch;
        _IsDodging = m_playerInput.dodge;

        CheckGround();
    }


    void FixedUpdate()
    {
        // Debug.Log($"current velocity {m_rigidBody.linearVelocity}");

        Move();
        Crouch();
        Jump();
        Dodge();
    }

    private void Move()
    {
        if (_IsDodging) return;
        float targetSpeed = _isSprinting ? _SprintSpeed : _WalkSpeed;
        if (_moveDirection == Vector2.zero) targetSpeed = 0;

        float targetFOV = _isSprinting ? _sprintFOV : _normalFOV;
        m_playerCamera.Lens.FieldOfView = Mathf.Lerp(a: m_playerCamera.Lens.FieldOfView, b: targetFOV, Time.fixedDeltaTime);

        Vector3 inputDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        Vector3 targetDirection = Quaternion.Euler(0.0f, m_playerCamera.transform.eulerAngles.y, 0.0f) * inputDirection;
        m_rigidBody.MovePosition(m_rigidBody.position + targetDirection.normalized * targetSpeed * Time.fixedDeltaTime);

    }


    private void Jump()
    {
        // if on the ground can jump
        // if in air can jump while stamina is still full 
        // while in air termin velocity is slower than forces tha pushes down 
        if (_isGrounded && _isJumping)
        {
            m_rigidBody.AddForce(Vector3.up * _JumpHeight, ForceMode.Impulse);
        }
    }

    private void Dodge()
    {
        if (!_IsDodging) return;
        float force = _isJumping ? _DodgeDistance / 2f : _DodgeDistance;
        float dodgeForce = Mathf.Sqrt(force * -2 * Physics.gravity.y);
        Vector3 inputDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y);
        Vector3 dodgeDirection = Quaternion.Euler(0.0f, m_playerCamera.transform.eulerAngles.y, 0.0f) * inputDirection;
        if (_moveDirection == Vector2.zero)
        {
            dodgeDirection = m_playerCamera.transform.forward;
        }

        m_rigidBody.linearDamping = 0;

        // Apply dodge impulse
        // m_rigidBody.AddForce(new Vector3(0f, _JumpHeight / 2f, 0f), ForceMode.Impulse);
        Debug.Log($" Normalized Dodge Direction: {dodgeDirection * dodgeForce}");

        m_rigidBody.AddForce(dodgeDirection * dodgeForce * Time.fixedDeltaTime, ForceMode.Impulse);

        // Prevent continuous dodging
        _IsDodging = false;
    }

    private void CheckGround()
    {
        spherePosition = new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z);
        _isGrounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundedLayers);
    }

    private void Crouch()
    {
        // if not grounded --> slam 

        m_capsuleCollider.height = _isCrouching ? _capsuleCrouchHeight : _capsuleWalkHeight;
        m_capsuleCollider.center = _isGrounded && _isCrouching ? _crouchCenter : _walkCenter;

        // Smooth interpolation for camera position
        float currentHeight = m_playerCameraRoot.localPosition.y;
        float _targetCameraHeight = _isCrouching ? _crouchHeight : _walkHeight;
        float smoothedHeight = Mathf.SmoothDamp(currentHeight, _targetCameraHeight, ref _cameraVelocity, Time.fixedDeltaTime * _cameraTransitionSpeed);

        m_playerCameraRoot.localPosition = new Vector3(
            m_playerCameraRoot.localPosition.x,
            smoothedHeight,
            m_playerCameraRoot.localPosition.z

        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

    }
}
