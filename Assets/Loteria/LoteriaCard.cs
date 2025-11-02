using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoteriaCard : MonoBehaviour
{

	[Header("Loteria Card Data")]
	private LoteriaCardsData cardsData;

	[SerializeField] private int id;
	[SerializeField] private float value;
	[SerializeField] private float chance;

	public int ID => id;
	public float Chance => chance;

	[Header("GameObject Components")]
	[SerializeField] private Image cardImages;



	public void InitializeLoteriaCardPrefab(LoteriaCardsData loteriaCardsData)
	{
		if (loteriaCardsData == null) { Debug.LogError("Failed to Create Card Instance as Loteria Card Data Doesn't Exist "); return; }
		this.cardsData = loteriaCardsData;
		this.id = loteriaCardsData.id;
		if (this.cardImages == null)
			this.cardImages = GetComponent<Image>();
		this.cardImages.sprite = loteriaCardsData.sprite;
		this.chance = loteriaCardsData.chance;



	}

}