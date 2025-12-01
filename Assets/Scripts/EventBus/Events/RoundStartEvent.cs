using System.Collections.Generic;
using UnityEngine;

public class RoundStartEvent : IGameEvent
{

	public List<LoteriaCardsData> CurrentDeck { get; private set; }
	public RoundStartEvent(List<LoteriaCardsData> currentDeck)
	{
		CurrentDeck = currentDeck;
	}
}
