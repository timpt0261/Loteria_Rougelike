using System;
using Mono.Cecil.Cil;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private Transform m_playerCameraRoot;
    [SerializeField] private CinemachineCamera m_playerCamera;
    [SerializeField] private PlayerInputReader m_playerInput;
    [SerializeField] private Rigidbody m_rigidBody;
    [SerializeField] private CapsuleCollider m_capsuleCollider;

    [Header("Jump and Gravity")]

    [SerializeField] private bool Grounded;
    [SerializeField] private float m_groundedOffset = .95f;
    [SerializeField] private float m_groundedRadius = .28f;
    [SerializeField] private LayerMask m_groundedLayers;

    [Header("Crouch")]

    private const int _walkHeight = 2;
    private Vector3 _walkCenter = Vector3.zero;
    private const int _crouchHeight = 1;

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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
    }

    void LateUpdate()
    {

    }
    void FixedUpdate()
    {
        Rotate();
        Move();
        Crouch();
        Jump();

    }


    private void Rotate()
    {
        Quaternion cameraRotation = Quaternion.Euler(new Vector3(0, m_playerCamera.transform.eulerAngles.y, 0));
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraRotation, Time.smoothDeltaTime * _RotateSpeed);

    }

    private void Move()
    {
        float targetSpeed = _isSprinting ? _SprintSpeed : _WalkSpeed;
        if (_moveDirection == Vector2.zero) targetSpeed = 0;
        Vector3 inputDirection = new Vector3(_moveDirection.x, 0f, _moveDirection.y).normalized;
        Vector3 targetDirection = Quaternion.Euler(0.0f, m_playerCamera.transform.eulerAngles.y, 0.0f) * inputDirection;
        m_rigidBody.MovePosition(m_rigidBody.position + targetDirection.normalized * targetSpeed * Time.fixedDeltaTime);
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

        float _cameraHeightVelocity = 0f;
        float _targetCameraHeight;
        float _cameraTransitionSpeed = 5f;
        m_capsuleCollider.height = _isCrouching ? _crouchHeight : _walkHeight;
        m_capsuleCollider.center = _isCrouching ? _crouchCenter : _walkCenter;

        // float heightDiff = _isCrouching ? 0 : 1;
        // _targetCameraHeight = transform.position.y + (_isCrouching ? _crouchCenter.y : _walkCenter.y);

        // // Smooth interpolation for camera position
        // float currentCameraY = m_playerCameraRoot.transform.position.y;
        // float smoothedHeight = Mathf.SmoothDamp(currentCameraY, _targetCameraHeight, ref _cameraHeightVelocity, 1f / _cameraTransitionSpeed);

        // m_playerCameraRoot.transform.position = new Vector3(
        //     m_playerCameraRoot.transform.position.x,
        //     smoothedHeight,
        //     m_playerCameraRoot.transform.position.z

        // );


    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.forward);

    }
}
