using System.Collections.Generic;
using UnityEngine;

public interface IGameEvent { }

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


	public RoundEndEvent()
	{

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


public class RevealDrawnCardsEvent : IGameEvent
{
	public float DelayTimeBetweenIntervals
	{
		get; private set;
	}

	public float CardRotationSpeed { get; private set; }
	public List<LoteriaCardsData> drawnCardsData
	{
		get; private set;
	}
	public RevealDrawnCardsEvent(float _delayTimeBetweenIntervals, float _cardRotationSpeed, List<LoteriaCardsData> _drawnCardsData)
	{
		DelayTimeBetweenIntervals = _delayTimeBetweenIntervals;
		CardRotationSpeed = _cardRotationSpeed;
		drawnCardsData = _drawnCardsData;

	}
}

public class RevealSingleCardEvent : IGameEvent
{
	public LoteriaCardsData drawnCardData { get; private set; }
	public RevealSingleCardEvent(LoteriaCardsData _drawnCardData)
	{
		drawnCardData = _drawnCardData;
	}
}

// RevealDrawnCardsCompleteEvent.cs
public class RevealDrawnCardsCompleteEvent : IGameEvent
{
    public RevealDrawnCardsCompleteEvent()
    {
    }
}

// DiscardCardsCompleteEvent.cs
public class DiscardCardsCompleteEvent : IGameEvent
{
    public DiscardCardsCompleteEvent()
    {
    }
}