using System;
using FMODUnity;
using UnityEngine;

[Serializable]
public class PlatformSFX : FMODSoundEffectCategory
{
	[field: SerializeField] public EventReference PlatformOpen { get; private set; }
	[field: SerializeField] public EventReference PlatformClose { get; private set; }
}



