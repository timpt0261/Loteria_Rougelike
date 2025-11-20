using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPS_Controller : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    [Header("Assest References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineCamera mainCamera;
    [SerializeField] private Transform cameraRoot;

    [SerializeField] private AudioSource audioSource;

    #region Input Values 

    [Header("Input Values")]
    [SerializeField] private Vector2 move;
    [SerializeField] private Vector2 look;
    [SerializeField] private bool interact;

    [SerializeField] private bool pause;
    [SerializeField] private bool nextPressed;
    [SerializeField] private bool previousPressed;

    public void OnMove(InputAction.CallbackContext value)
    {
        move = value.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext value)
    {
        look = value.ReadValue<Vector2>();
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        interact = value.action.triggered;
    }

    public void OnPause(InputAction.CallbackContext value)
    {
        interact = value.action.triggered;
    }

    public void OnNext(InputAction.CallbackContext value)
    {
        nextPressed = value.action.triggered;
    }

    public void OnPrevious(InputAction.CallbackContext value)
    {
        previousPressed = value.action.triggered;
    }

    #endregion 


    #region  UI Handling


    #endregion
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (characterController == null) { characterController = GetComponent<CharacterController>(); }
        if (capsuleCollider == null) { capsuleCollider = GetComponent<CapsuleCollider>(); }
        if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }
        if (animator == null) { animator = GetComponent<Animator>(); }
    }

    // Update is called once per frame
    void Update()
    {
        

    }

    void FixedUpdate()
    {
        Move();
    }

    public void Move()
    {

        float targetSpeed = speed;

        // Calculate movement direction relative to camera
        Vector3 inputDirection = new Vector3(move.x, 0f, move.y);
        Vector3 targetDirection = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f) * inputDirection;

        // Apply movement
        // rigidBody.MovePosition(
        //     rigidBody.position + targetDirection.normalized * targetSpeed * Time.fixedDeltaTime
        // );
        characterController.Move(targetDirection * speed * Time.fixedDeltaTime);
    }

    



}
