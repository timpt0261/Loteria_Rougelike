using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoteriaCardsData", menuName = "Loteria/LoteriaCardData", order = 0)]
public class LoteriaCardsData : ScriptableObject
{
	public int id;
	public float chance;
	public Sprite sprite;

	public static explicit operator LoteriaCardsData(List<ScriptableObject> v)
	{
		throw new NotImplementedException();
	}
}
