using UnityEngine;
using System.Collections.Generic;

public class TablaGridLayout
{
	private int cardsActive;

	public RectOffset rectOffset;
	public Vector2 cellSize;
	public Vector2 spacing;
	public List<GameObject> CardPrefabPool;

	public TablaGridLayout(RectOffset _rectOffset, Vector2 _cellSize, Vector2 _spacing, List<GameObject> _cardPrefabPool, int _cardsActive)
	{
		rectOffset = _rectOffset;
		cellSize = _cellSize;
		spacing = _spacing;
		cardsActive = _cardsActive;

		CardPrefabPool = new List<GameObject>(_cardPrefabPool);

		// Activate/deactivate cards based on cardsActive
		for (int i = 0; i < _cardPrefabPool.Count; i++)
		{
			_cardPrefabPool[i].gameObject.SetActive(i < cardsActive);
		}
	}
}
