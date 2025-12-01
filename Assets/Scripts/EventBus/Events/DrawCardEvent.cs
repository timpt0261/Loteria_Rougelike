using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DrawCardEvent : IGameEvent
{

	public List<LoteriaCardsData> DrawnCards { get; private set; }
	public float DrawTime { get; private set; }
	public DrawCardEvent(float drawTime, List<LoteriaCardsData> drawnCards)
	{
		DrawnCards = drawnCards;
		DrawTime = drawTime;
	}
}
