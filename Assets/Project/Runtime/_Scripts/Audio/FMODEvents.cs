using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents Instance { get; private set; }



    [field: Header("Music")]

    [field: SerializeField] public EventReference BackGroundMusic;


    [field: Header("SFX")]

    [field: SerializeField] public ElevatorSFX ElevatorSFX;

    [field: SerializeField] public PlatformSFX PlatformSFX;

    [field: Header("Token")]
    [field: SerializeField] public EventReference TokenPlacement { get; private set; }

    [field: SerializeField] public CardSFX CardSFX;


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



