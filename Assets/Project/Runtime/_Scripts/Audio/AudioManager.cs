using System;
using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;

    [Range(0, 1)]
    public float musicVolume = 1;

    [Range(0, 1)]
    public float sfxVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    private EventInstance musicEventInstance;
    [field: SerializeField] private List<EventInstance> eventInstances;
    [field: SerializeField] private List<StudioEventEmitter> eventEmitters;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        eventInstances = new();
        eventEmitters = new();

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
    }

    private void Start() => InitializeBackgroundMusic(FMODEvents.Instance.BackGroundMusic);

    private void Update()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(sfxVolume);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPosition) => RuntimeManager.PlayOneShot(sound.Guid, worldPosition);

    public EventInstance CreateEventInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference.Guid);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject gameObjectEmitter)
    {
        StudioEventEmitter eventEmitter = gameObjectEmitter.GetComponent<StudioEventEmitter>();
        eventEmitter.EventReference = eventReference;
        eventEmitters.Add(eventEmitter);
        return eventEmitter;
    }

    private void InitializeBackgroundMusic(EventReference backGroundMusicEventReference)
    {
        musicEventInstance = CreateEventInstance(backGroundMusicEventReference);
        musicEventInstance.start();
    }




    private void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }

}
