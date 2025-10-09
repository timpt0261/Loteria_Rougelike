using UnityEngine;
using Unity.Cinemachine;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera main_camera;
    [SerializeField] private CinemachineInputAxisController axisController;


    public CinemachineCamera GetCinemachineCamera() => main_camera;

    



}
