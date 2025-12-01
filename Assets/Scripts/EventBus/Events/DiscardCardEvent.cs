using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DiscardCardEvent : IGameEvent
{

	private float _discardTime;
	public DiscardCardEvent(float discardTime)
	{
		_discardTime = discardTime;
	}
}
