using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 10f;

    [Header("Assest References")]
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Animator animator;
    [SerializeField] private CinemachineCamera mainCamera;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private AudioSource audioSource;

    [Header("Input Values")]
    [SerializeField] private Vector2 move;
    [SerializeField] private Vector2 look;
    [SerializeField] private bool interact;
    [SerializeField] private bool pause;
    [SerializeField] private bool nextPressed;
    [SerializeField] private bool previousPressed;
    private PlayerInput playerInput;

    [Header("Interaction Handling")]

    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private InteractionPrompt prompt;
    [SerializeField] private Collider[] buffer = new Collider[32];
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
        if (audioSource == null) { audioSource = GetComponent<AudioSource>(); }
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
        int count = Physics.OverlapSphereNonAlloc(transform.position, interactionRadius, buffer, interactionLayer, QueryTriggerInteraction.Collide);
        IInteractable nearest = null;
        float bestDistSq = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider col = buffer[i];
            if (col == null) continue;
            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;
            if (!interactable.CanInteract()) continue;
            float distSq = (col.transform.position - transform.position).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                nearest = interactable;
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
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

}
