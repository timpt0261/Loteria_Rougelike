using System.Collections.Generic;
using UnityEngine;

public class RunStartEvent : IGameEvent
{
	public List<LoteriaCardsData> CurrentDeck { get; private set; }
	public int GridSize { get; private set; }

	public RunStartEvent(int _gridSize, List<LoteriaCardsData> _currentDeck)
	{
		GridSize = _gridSize;
		CurrentDeck = _currentDeck;
	}

}
