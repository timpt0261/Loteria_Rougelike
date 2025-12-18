using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;
using FMODUnity;


public class PlayerController : MonoBehaviour
{
    [field: SerializeField] private float speed = 10f;

    [field: Header("Assest References")]
    [field: SerializeField] private CapsuleCollider capsuleCollider;
    [field: SerializeField] private Rigidbody rigidBody;
    [field: SerializeField] private Animator animator;
    [field: SerializeField] private CinemachineCamera mainCamera;
    [field: SerializeField] private Transform cameraRoot;
    public CinemachineCamera MainCamera => mainCamera;
    private bool disableMovement;


    [field: Header("Audio")]
    [field: SerializeField] private EventInstance playerFootSteps;

    [field: Header("Input Values")]
    [field: SerializeField] private Vector2 move;
    [field: SerializeField] private Vector2 look;
    [field: SerializeField] private bool interactThisFrame;
    [field: SerializeField] private bool pause;
    private PlayerInput playerInput;

    [field: Header("Interaction Handling")]
    [field: SerializeField] private float fanAngle = 90f; // Total angle of the fan in degrees
    [field: SerializeField] private int fanLines = 5;
    [field: SerializeField] private float interactionRadius = 2f;
    [field: SerializeField] private LayerMask interactionLayer;
    [field: SerializeField] private InteractionPrompt prompt;
    private bool disableInteraction;
    private IInteractable focus;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (rigidBody == null) { rigidBody = GetComponent<Rigidbody>(); }
        if (capsuleCollider == null) { capsuleCollider = GetComponent<CapsuleCollider>(); }
        if (animator == null) { animator = GetComponent<Animator>(); }
        if (playerInput == null) { playerInput = GetComponent<PlayerInput>(); }
        if (prompt == null) { prompt = GetComponentInChildren<InteractionPrompt>(); }
    }

    private void Start()
    {
        playerFootSteps = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.PlayerFootSteps);

    }

    private void Update()
    {
        if (disableInteraction)
        {
            focus = null;
            interactThisFrame = false;
            return;
        }
        Interact();
        interactThisFrame = false;
    }

    void FixedUpdate()
    {
        if (disableMovement)
        {
            playerInput.enabled = false;
            rigidBody.linearVelocity = Vector3.zero;
            UpdateSound();
            return;
        }
        Move();
        UpdateSound();
    }

    private void Move()
    {
        // Calculate movement direction relative to camera
        Vector3 inputDirection = new Vector3(move.x, 0f, move.y);
        Vector3 targetDirection = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f) * inputDirection;

        // Apply movement
        rigidBody.MovePosition(
            rigidBody.position + targetDirection.normalized * speed * Time.fixedDeltaTime
        );
    }
    private void UpdateSound()
    {
        playerFootSteps.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        // start footsteps event if the player has an x velocity and is on the ground
        if (move != Vector2.zero)
        {
            // get the playback state
            PLAYBACK_STATE playbackState;
            playerFootSteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootSteps.start();
            }
        }
        // otherwise, stop the footsteps event
        else
        {
            playerFootSteps.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    #region Interaction Handling
    private void Interact()
    {
        IInteractable nearest = FindNearestInteractble();
        UpdateFocus(nearest);

        // Changed: only interact if the button was pressed this frame
        if (focus != null && interactThisFrame)
        {
            focus.Interact(this.gameObject);
        }
    }

    private IInteractable FindNearestInteractble()
    {
        IInteractable nearest = null;
        float bestDistSq = float.MaxValue;

        Vector3 forward = mainCamera.transform.forward;
        float halfAngle = fanAngle * 0.5f;

        for (int i = 0; i < fanLines; i++)
        {
            float t = fanLines > 1 ? (float)i / (fanLines - 1) : 0.5f;
            // Map t from [0,1] to [-halfAngle, halfAngle]
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);

            Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * forward;

            RaycastHit hit;
            if (Physics.Raycast(mainCamera.transform.position, direction, out hit, interactionRadius, interactionLayer, QueryTriggerInteraction.Collide))
            {
                IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable != null && interactable.CanInteract())
                {
                    float distSq = (hit.point - mainCamera.transform.position).sqrMagnitude;
                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        nearest = interactable;
                    }
                }
            }
        }

        return nearest;
    }

    private void UpdateFocus(IInteractable nearest)
    {
        if (ReferenceEquals(focus, nearest)) return;
        focus?.OnFocusLost();
        focus = nearest;
        if (focus == null)
        {
            prompt.Hide();
            return;
        }
        focus?.OnFocusGained();
        prompt.Show(focus);
        return;
    }
    #endregion

    public void EnablePlayerMovement()
    {
        HandlePlayerActivation(true);
    }

    public void DisablePlayerMovement()
    {
        HandlePlayerActivation(false);
    }

    private void HandlePlayerActivation(bool activation)
    {
        disableMovement = activation;
        disableInteraction = activation;
        playerInput.enabled = activation;
    }

    #region Player Input Events

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

        if (value.performed)
        {
            interactThisFrame = true;
        }
    }

    public void OnPause(InputAction.CallbackContext value)
    {
        pause = value.action.triggered;
    }

    #endregion

    void OnDrawGizmos()
    {
        if (mainCamera == null) return; // Safety check

        Gizmos.color = Color.yellow;

        Vector3 forward = mainCamera.transform.forward;
        float halfAngle = fanAngle * 0.5f;

        for (int i = 0; i < fanLines; i++)
        {
            float t = fanLines > 1 ? (float)i / (fanLines - 1) : 0.5f;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * forward;
            Gizmos.DrawRay(mainCamera.transform.position, direction * interactionRadius);
        }
    }
}