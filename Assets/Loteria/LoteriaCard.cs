using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class LoteriaCard : MonoBehaviour
{

	[Header("Loteria Card Data")]
	private LoteriaCardsData CurrentLoteriaCardData
	{
		set
		{
			this.id = value.id;
			this.chance = value.chance;
			this.loteriaCard_Artwork.sprite = value.sprite;
			this.loteriaCard_TokenMarker.enabled = false;
			this.loteriaCard_TokenButton.interactable = false;
			OnCardSet?.Invoke();
		}
	}

	[SerializeField] private int id;
	[SerializeField] private float priceValue;
	[SerializeField] private float chance;


	public int ID => id;
	public float Chance => chance;

	public bool TokenPlaced() => this.loteriaCard_TokenMarker.enabled;

	[Header("UI Components")]
	[SerializeField] private TextMeshProUGUI loteriaCard_ID;
	[SerializeField] private Image loteriaCard_Artwork;
	[SerializeField] private Image loteriaCard_TokenMarker; // sprite that contains token
	[SerializeField] private Button loteriaCard_TokenButton;

	[Header("Animator")]
	[SerializeField] private Animator cardAnimator;

	[Header("Loteria Card Events")]
	public UnityEvent OnCardSet;
	public UnityEvent OnCanPlaceToken; // Can start timer
	public UnityEvent OnTokenPlaced; // Can end timer

	public void SetCardData(LoteriaCardsData newLoteriaCardData)
	{

		if (newLoteriaCardData == null)
		{
			Debug.Log("the new loteria card data doesn't exist");
			return;
		}

		if (loteriaCard_ID == null)
		{
			loteriaCard_ID = this.GetComponent<TextMeshProUGUI>();
		}

		if (loteriaCard_Artwork == null)
		{
			loteriaCard_Artwork = this.GetComponent<Image>();
		}

		if (loteriaCard_TokenMarker == null)
		{
			loteriaCard_TokenMarker = this.GetComponentInChildren<Image>();
		}

		if (loteriaCard_TokenButton == null)
		{
			loteriaCard_TokenButton = this.GetComponentInChildren<Button>();
		}

		CurrentLoteriaCardData = newLoteriaCardData;



	}


	public void CanPlaceToken(bool tokenEnabled)
	{
		loteriaCard_TokenButton.interactable = tokenEnabled;
		OnCanPlaceToken?.Invoke();

	}

	public void OnPlaceToken()
	{
		Debug.Log("Pressing on Token");
		loteriaCard_TokenMarker.enabled = true;
		OnTokenPlaced?.Invoke(); // here's where it should update the token
	}


}