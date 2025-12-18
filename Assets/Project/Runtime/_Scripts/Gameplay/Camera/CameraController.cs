using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController Instance { get; private set; }
    // need the brain 
    [field: SerializeField] private Camera mainCamera;
    private CinemachineBrain cinemachineBrain;

    [field: SerializeField] public CinemachineCamera currentCamera { get; private set; }

    private int topPriorty = 0;
    private int lowPriority = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();

        if (currentCamera == null)
        {
            currentCamera = GameObject.Find("Player_Camera").GetComponent<CinemachineCamera>();
        }
    }


    // event based

    public void SwitchCameras(CinemachineCamera newCamera)
    {
        currentCamera.Priority = lowPriority;
        currentCamera = newCamera;
        currentCamera.Priority = topPriorty;
    }
}
