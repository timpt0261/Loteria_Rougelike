using System;
using FMODUnity;
using UnityEngine;

[Serializable]
public class BackGroundMusic : FMODSoundEffectCategory
{
	[field: SerializeField] public EventReference MainBackGroundMusic { get; private set; }

	[field: SerializeField] public EventReference BossBackGroundMusic { get; private set; }
}
