using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RevealCardEvent : IGameEvent
{

	public LoteriaCardsData drawnCardData{get; private set;}
	public RevealCardEvent(LoteriaCardsData _drawnCardData)
	{
		drawnCardData = _drawnCardData;
	}
}
