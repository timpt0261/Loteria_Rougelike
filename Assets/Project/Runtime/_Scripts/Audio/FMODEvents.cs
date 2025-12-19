using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents Instance { get; private set; }

    [field: Header("Music")]

    [field: SerializeField] private EventReference BackGroundMusic;


    [field: Header("SFX")]

    [field: Header("Elevator")]
    [field: SerializeField] public EventReference ElevatorOpen { get; private set; }
    [field: SerializeField] public EventReference ElevatorClose { get; private set; }

    [field: Header("Token")]
    [field: SerializeField] public EventReference TokenPlacement { get; private set; }

    [field: Header("Player")]
    [field: SerializeField] public EventReference PlayerFootSteps { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

    }
}
