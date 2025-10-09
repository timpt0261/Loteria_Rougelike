using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private PlayerCamera m_playerCamera;
    [SerializeField] private PlayerInputReader m_playerInput;
    [SerializeField] private Rigidbody m_rigidBody;
    [SerializeField] private CapsuleCollider m_capsuleCollider;

    [Header("Jump and Gravity")]

    [SerializeField] private bool Grounded;
    [SerializeField] private float m_groundedOffset = .95f;
    [SerializeField] private float m_groundedRadius = .28f;
    [SerializeField] private LayerMask m_groundedLayers;

    [Header("Crouch")]

    private int _walkHeight = 2;
    private Vector3 _walkCenter = Vector3.zero;
    private int _crouchHeight = 1;

    private Vector3 _crouchCenter = new Vector3(0, -0.5f, 0);

    private Vector2 _moveDirection;
    private Vector2 _lookDirection;
    private bool _isSprinting;
    private bool _isCrouching;
    private bool _isJumping;

    [SerializeField] private float _JumpHeight = 5f;
    [SerializeField] private float _WalkSpeed = 3f;
    [SerializeField] private float _SprintSpeed = 10f;
    [SerializeField] private float _RotateSpeed = 0.2f;


    private const float _rotation_threshold = 0.01f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.lockState = CursorLockMode.Locked;

    }

    // Update is called once per frame
    void Update()
    {
        _moveDirection = m_playerInput.move;
        _lookDirection = m_playerInput.look;

        _isJumping = m_playerInput.jump;
        _isSprinting = m_playerInput.sprint;
        _isCrouching = m_playerInput.crouch;

        CheckGround();
        Crouch();


    }
    void FixedUpdate()
    {
        Rotate();
        Move();
        Jump();



    }

    private void Move()
    {
        float targetSpeed = _isSprinting ? _SprintSpeed : _WalkSpeed;
        if (_moveDirection == Vector2.zero) targetSpeed = 0;
        Vector3 inputDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y).normalized;
        Vector3 targetDirection = Quaternion.Euler(0.0f, m_playerCamera.GetCinemachineCamera().transform.eulerAngles.y, 0.0f) * inputDirection;
        m_rigidBody.MovePosition(m_rigidBody.position + targetDirection.normalized * targetSpeed * Time.fixedDeltaTime);
    }

    private void Rotate()
    {

        float targetYaw = _lookDirection.x * _RotateSpeed * Time.deltaTime;
        float targetPitch = _lookDirection.y * _RotateSpeed * Time.deltaTime;

        Vector3 inputRotation = new Vector3(targetPitch, targetYaw, 0f);
        Quaternion targetRotation = Quaternion.Euler(inputRotation);
        m_rigidBody.MoveRotation(targetRotation);

    }

    private void Jump()
    {
        // if on the ground can jump
        if (Grounded && _isJumping)
        {
            m_rigidBody.AddForce(Vector3.up * _JumpHeight, ForceMode.Impulse);
        }
    }

    private void CheckGround()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, m_groundedRadius, m_groundedLayers);
    }

    private void Crouch()
    {
        m_capsuleCollider.height = _isCrouching ? _crouchHeight : _walkHeight;
        m_capsuleCollider.center = _isCrouching ? _crouchCenter : _walkCenter;


    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y - m_groundedOffset, transform.position.z), m_groundedRadius);
    }
}
