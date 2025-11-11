using TMPro;
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

	private const float HUNDRED = 100f;
	[SerializeField] private int id;
	[SerializeField] private float priceValue;
	[SerializeField] private float chance;

	public int ID => id;
	public float Chance => chance;
	public bool TokenPlaced() => this.loteriaCard_TokenMarker.enabled;

	[Header("UI Components")]
	[SerializeField] private TextMeshProUGUI loteriaCard_ID;
	[SerializeField] private Image loteriaCard_Artwork;
	[SerializeField] private Image loteriaCard_TokenMarker;
	[SerializeField] private Button loteriaCard_TokenButton;

	[Header("Token Timer Settings")]
	[SerializeField] private float maxBonusTime = 2f;
	[SerializeField] private float minBonusTime = 1f;
	[SerializeField] private float baseMultiplier = 1f;

	private float tokenTimerStart;
	private bool isTimerActive = false;

	public float TimerBonusMultiplier { get; private set; }

	[Header("Animator")]
	[SerializeField] private Animator cardAnimator;

	[Header("Loteria Card Events")]
	public UnityEvent OnCardSet;
	public UnityEvent OnCanPlaceToken;
	public UnityEvent OnTokenPlaced;

	public void SetCardData(LoteriaCardsData newLoteriaCardData)
	{
		if (newLoteriaCardData == null)
		{
			Debug.LogWarning("Attempted to set null card data");
			return;
		}

		InitializeComponents();
		CurrentLoteriaCardData = newLoteriaCardData;
	}

	private void InitializeComponents()
	{
		if (loteriaCard_ID == null)
			loteriaCard_ID = GetComponent<TextMeshProUGUI>();

		if (loteriaCard_Artwork == null)
			loteriaCard_Artwork = GetComponent<Image>();

		if (loteriaCard_TokenMarker == null)
			loteriaCard_TokenMarker = GetComponentInChildren<Image>();

		if (loteriaCard_TokenButton == null)
			loteriaCard_TokenButton = GetComponentInChildren<Button>();
	}

	public void CanPlaceToken(bool canPlace)
	{
		loteriaCard_TokenButton.interactable = canPlace;

		if (canPlace)
		{
			StartTokenTimer();
		}
		else
		{
			StopTokenTimer();
		}

		OnCanPlaceToken?.Invoke();
	}

	public void OnPlaceToken()
	{
		if (!loteriaCard_TokenButton.interactable) return;

		loteriaCard_TokenMarker.enabled = true;
		loteriaCard_TokenButton.interactable = false;

		TimerBonusMultiplier = CalculateTimerBonus();
		StopTokenTimer();

		OnTokenPlaced?.Invoke();
	}

	private void StartTokenTimer()
	{
		tokenTimerStart = Time.time;
		isTimerActive = true;
		TimerBonusMultiplier = baseMultiplier;
	}

	private void StopTokenTimer()
	{
		isTimerActive = false;
	}

	private float CalculateTimerBonus()
	{
		if (!isTimerActive)
			return baseMultiplier;

		float elapsedTime = Time.time - tokenTimerStart;

		// If placed within bonus window, calculate bonus
		if (elapsedTime <= maxBonusTime)
		{
			// Linear interpolation: faster = higher multiplier
			// At 0 seconds: returns maxBonusTime (e.g., 4.0)
			// At maxBonusTime: returns minBonusTime (e.g., 1.0)
			float bonus = Mathf.Lerp(maxBonusTime, minBonusTime, elapsedTime / maxBonusTime);
			return Mathf.Round(bonus * 100f) / 100f; // Round to 2 decimal places
		}

		// If too slow, return base multiplier
		return baseMultiplier;
	}

	public float GetCurrentTimeRemaining()
	{
		if (!isTimerActive)
			return 0f;

		float elapsed = Time.time - tokenTimerStart;
		return Mathf.Max(0f, maxBonusTime - elapsed);
	}

	public float GetCurrentBonusPreview()
	{
		if (!isTimerActive)
			return baseMultiplier;

		float elapsed = Time.time - tokenTimerStart;

		if (elapsed <= maxBonusTime)
		{
			float bonus = Mathf.Lerp(maxBonusTime, minBonusTime, elapsed / maxBonusTime);
			return Mathf.Round(bonus * HUNDRED) / HUNDRED;
		}

		return baseMultiplier;
	}
}