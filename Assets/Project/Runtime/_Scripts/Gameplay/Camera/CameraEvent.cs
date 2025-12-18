using UnityEngine;
using Unity.Cinemachine;



[RequireComponent(typeof(Collider))]
public class CameraEvent : MonoBehaviour
{
    [field: SerializeField] private CameraController cameraController = CameraController.Instance;
    [field: SerializeField] private BoxCollider boxCollider;
    [field: SerializeField] private CinemachineCamera virtualCamera;
    [field: SerializeField] private PlayerController player;
    private void Start()
    {
        if (boxCollider == null) { boxCollider = GetComponent<BoxCollider>(); }
        boxCollider.isTrigger = true;

        if (player == null) { player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>(); }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {

            cameraController.SwitchCameras(virtualCamera);
            player.DisablePlayerMovement();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            cameraController.SwitchCameras(player.MainCamera);
            player.EnablePlayerMovement();
        }

    }


}
