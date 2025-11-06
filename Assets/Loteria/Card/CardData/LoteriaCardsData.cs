using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoteriaCardsData", menuName = "Loteria/LoteriaCardData", order = 0)]
public class LoteriaCardsData : ScriptableObject
{
	public int id;
	public float chance;
	public Sprite sprite;
}
