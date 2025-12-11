using System.Collections.Generic;
using UnityEngine;

// Base constraint for all card data types
public interface ICardData { }

public class RunStartEvent<T> : IGameEvent where T : ScriptableObject
{
	public List<T> CurrentDeck { get; private set; }
	public int GridSize { get; private set; }

	public RunStartEvent(int _gridSize, List<T> _currentDeck)
	{
		GridSize = _gridSize;
		CurrentDeck = _currentDeck;
	}
}

public class RoundStartEvent : IGameEvent
{
	public int Round { get; private set; }
	public TablaWinningRuleState WinTableState { get; private set; }
	public int TargetLength { get; private set; }

	public RoundStartEvent(int _round, TablaWinningRuleState _winTableState, int _targetLength)
	{
		Round = _round;
		WinTableState = _winTableState;
		TargetLength = _targetLength;
	}
}

public class RoundEndEvent : IGameEvent
{
	public bool Win { get; private set; }
	public RoundEndEvent(bool winState)
	{
		Win = winState;
	}
}

public class DrawCardsEvent : IGameEvent
{
	public int DrawnAmount { get; private set; }
	public float DrawTime { get; private set; }

	public DrawCardsEvent(int drawnAmount, float drawTime)
	{
		DrawnAmount = drawnAmount;
		DrawTime = drawTime;
	}
}

public class DiscardCardEvent : IGameEvent
{
	public float DiscardTime { get; private set; }

	public DiscardCardEvent(float discardTime)
	{
		DiscardTime = discardTime;
	}
}

public class LoteiaCallEvent : IGameEvent
{
	public LoteiaCallEvent()
	{
	}
}

public class RevealDrawnCardsEvent<T> : IGameEvent where T : ScriptableObject
{
	public float DelayTimeBetweenIntervals { get; private set; }
	public float CardRotationSpeed { get; private set; }
	public List<T> DrawnCardsData { get; private set; }

	public RevealDrawnCardsEvent(float _delayTimeBetweenIntervals, float _cardRotationSpeed, List<T> _drawnCardsData)
	{
		DelayTimeBetweenIntervals = _delayTimeBetweenIntervals;
		CardRotationSpeed = _cardRotationSpeed;
		DrawnCardsData = _drawnCardsData;
	}
}

public class RevealSingleCardEvent<T> : IGameEvent where T : ScriptableObject
{
	public T DrawnCardData { get; private set; }

	public RevealSingleCardEvent(T _drawnCardData)
	{
		DrawnCardData = _drawnCardData;
	}
}

public class RevealDrawnCardsCompleteEvent : IGameEvent
{
	public RevealDrawnCardsCompleteEvent()
	{
	}
}

public class DiscardCardsCompleteEvent : IGameEvent
{
	public DiscardCardsCompleteEvent()
	{
	}
}