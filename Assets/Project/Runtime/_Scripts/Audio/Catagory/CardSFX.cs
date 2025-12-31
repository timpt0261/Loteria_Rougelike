using System;
using FMODUnity;
using UnityEngine;

[Serializable]
public class CardSFX : FMODSoundEffectCategory
{
	[field: SerializeField] public EventReference CardMove { get; private set; }

	[field: SerializeField] public EventReference CardReveal { get; private set; }

	[field: SerializeField] public EventReference CardDiscard { get; private set; }
}



