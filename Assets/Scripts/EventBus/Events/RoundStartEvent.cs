using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

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
