using System;
using FMODUnity;
using UnityEngine;

[Serializable]
public class ElevatorSFX : FMODSoundEffectCategory
{
	[field: SerializeField] public EventReference ElevatorOpen { get; private set; }
	[field: SerializeField] public EventReference ElevatorClose { get; private set; }
}



