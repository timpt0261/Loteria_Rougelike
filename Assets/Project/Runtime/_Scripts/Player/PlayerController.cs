using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
using DG.Tweening;
using DG.Tweening.Core.Easing;


public class PlayerController : MonoBehaviour
{
    [field: SerializeField] private float speed = 10f;

    [field: Header("Assest References")]
    [field: SerializeField] private CapsuleCollider capsuleCollider;
    [field: SerializeField] private Rigidbody rigidBody;
    [field: SerializeField] private Animator animator;
    [field: SerializeField] private CinemachineCamera mainCamera;
    [field: SerializeField] private Transform cameraRoot;

    [field: Header("Audio")]
    [field: SerializeField] private EventReference playerWalkAudioEvent;

    [field: Header("Input Values")]
    [field: SerializeField] private Vector2 move;
    [field: SerializeField] private Vector2 look;
    [field: SerializeField] private bool interact;
    [field: SerializeField] private bool pause;
    [field: SerializeField] private bool nextPressed;
    [field: SerializeField] private bool previousPressed;
    private PlayerInput playerInput;

    [field: Header("Interaction Handling")]
    [field: SerializeField] private float fanAngle = 90f; // Total angle of the fan in degrees
    [field: SerializeField] private int fanSticks = 5;
    [field: SerializeField] private float interactionRadius = 2f;
    [field: SerializeField] private LayerMask interactionLayer;
    [field: SerializeField] private InteractionPrompt prompt;
    [field: SerializeField] private Collider[] buffer = new Collider[32];
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

    private void Update()
    {
        Interact();
    }
    void FixedUpdate()
    {
        Move();
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
        // rigidBody.Move(targetDirection * speed * Time.fixedDeltaTime);

    }

    #region Interaction Handling
    private void Interact()
    {
        IInteractable nearest = FindNearestInteractble();
        UpdateFocus(nearest);
        if (focus != null && interact)
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

        for (int i = 0; i < fanSticks; i++)
        {
            float t = fanSticks > 1 ? (float)i / (fanSticks - 1) : 0.5f;
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
    private void Pause()
    {

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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 forward = mainCamera.transform.forward;
        float halfAngle = fanAngle * 0.5f;

        for (int i = 0; i < fanSticks; i++)
        {
            float t = fanSticks > 1 ? (float)i / (fanSticks - 1) : 0.5f;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * forward;
            Gizmos.DrawRay(mainCamera.transform.position, direction * interactionRadius);
        }
    }

}