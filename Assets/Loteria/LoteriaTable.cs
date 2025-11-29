using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoteriaTable : MonoBehaviour
{
	public static LoteriaTable Instance { get; private set; }

	[Header("Table Configuration")]
	[SerializeField] private List<LoteriaCardsData> loteriaDeck;
	public void SetLoteriaDeck(List<LoteriaCardsData> loteriaCardDeck)
	{
		this.loteriaDeck = loteriaCardDeck;
	}
	[SerializeField] private List<GameObject> cardPrefabs;

	private const int TOTAL_TABLA_COUNT = 16;
	private const int GRID_SIZE = 4;
	private const int TOP_LEFT_INDEX = 0;
	private const int TOP_RIGHT_INDEX = 3;
	private const int CENTER_LEFT_TOP_INDEX = 5;
	private const int CENTER_RIGHT_TOP_INDEX = 6;
	private const int CENTER_LEFT_BOTTOM_INDEX = 9;
	private const int CENTER_RIGHT_BOTTOM_INDEX = 10;
	private const int BOTTOM_LEFT_INDEX = 12;
	private const int BOTTOM_RIGHT_INDEX = 15;

	private List<int> tableGrid = new();
	private Dictionary<int, LoteriaCard> loteriaSlots = new();

	[Header("Scoring")]
	[SerializeField] private TextMeshProUGUI scoreUI;
	[SerializeField] private int singleMultiplier = 1;
	[SerializeField] private int horizontalMultiplier = 4;
	[SerializeField] private int verticalMultiplier = 4;
	[SerializeField] private int diagonalMultiplier = 8;
	[SerializeField] private int fullMultiplier = 16;

	private float score = 0;
	public float Score => score;

	[SerializeField] private List<bool> tokenState = new();
	// table states
	[SerializeField] private bool IsTableWithToken = false;
	[SerializeField] private bool IsTableWithCompletedRow = false;
	[SerializeField] private bool IsTableWithCompletedColumn = false;
	[SerializeField] private bool IsTableWithCompletedDiagonal = false;
	[SerializeField] private bool IsTableCompleted = false;


	private List<int> unmarkedSlots;    // keep  track of unmarked slots
	public List<int> UnmarkedSlots { get { return unmarkedSlots; } }

	private List<int> markedSlots; // keep track of marked slots
	public List<int> MarkedSlots { get { return markedSlots; } }

	// be able to set slots
	// be able to switch slots

	private string currentSeed;
	public string CurrentSeed { get { return currentSeed; } private set { currentSeed = value; } }
	public int MarkedCount;

	[Header("Events")]
	public UnityEvent OnTableCompleted;

	public UnityEvent OnLoteriaWinConditionMet;

	private void OnEnable()
	{
		DrawnCardDisplay.OnCardRevealed += UpdateTokenPlacement;
	}

	private void Osable()
	{
			DrawnCardDisplay.OnCardRevealed -= UpdateTokenPlacement;
	}

	#region Unity Lifecycle
	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;
		unmarkedSlots = tableGrid;
	}
	#endregion

	#region Table Setup
	public void ResetTable()
	{
		tableGrid.Clear();
		loteriaSlots.Clear();
		ResetTokenPlacers();
		SetTable();
		score = 0;

	}

	void Update()
	{
		if (LoteriaWinConditionIsMet())
		{
			OnLoteriaWinConditionMet?.Invoke();
			ResetTableState();
		}

	}
	private void SetTable()
	{
		List<LoteriaCardsData> shuffled = Shuffle();

		for (int i = 0; i < TOTAL_TABLA_COUNT; i++)
		{
			GameObject currentSlot = cardPrefabs[i];
			LoteriaCard loteriaCard = currentSlot.GetComponent<LoteriaCard>();
			loteriaCard.SetCardData(shuffled[i % shuffled.Count]);

			int cardId = loteriaCard.ID;
			tableGrid.Add(cardId);
			loteriaSlots.Add(cardId, loteriaCard);
		}
	}

	private List<LoteriaCardsData> Shuffle()
	{
		int seed = (int)System.DateTime.Now.Ticks;
		Random.InitState(seed);
		var shuffled = new List<LoteriaCardsData>(loteriaDeck);

		for (int i = 0; i < shuffled.Count; i++)
		{
			int randomIndex = Random.Range(i, shuffled.Count);
			var temp = shuffled[i];
			shuffled[i] = shuffled[randomIndex];
			shuffled[randomIndex] = temp;
		}

		return shuffled;
	}
	private List<int> GenerateRandomTable()
	{
		int count = loteriaDeck.Count;
		HashSet<int> choosenSlots = new();
		while (choosenSlots.Count < TOTAL_TABLA_COUNT)
		{

			int x = Random.Range(0, count);
			choosenSlots.Add(x);
		}

		return new List<int>(choosenSlots);
	}


	#endregion

	// Add these public functions to your LoteriaTable class

	#region Slot Management

	/// <summary>
	/// Sets a specific slot to display a card from the loteria deck
	/// </summary>
	/// <param name="slotIndex">Index of the slot (0-15)</param>
	/// <param name="cardData">The card data to set in this slot</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool SetSlotCard(int slotIndex, LoteriaCardsData cardData)
	{
		// Validate slot index
		if (slotIndex < 0 || slotIndex >= TOTAL_TABLA_COUNT)
		{
			Debug.LogWarning($"Invalid slot index: {slotIndex}. Must be between 0 and {TOTAL_TABLA_COUNT - 1}");
			return false;
		}

		// Validate card data
		if (cardData == null)
		{
			Debug.LogWarning("Cannot set slot with null card data");
			return false;
		}

		// Get the current card ID at this slot
		int oldCardId = tableGrid[slotIndex];

		// Remove old mapping
		if (loteriaSlots.ContainsKey(oldCardId))
		{
			loteriaSlots.Remove(oldCardId);
		}

		// Get the card component and update it
		GameObject slotObject = cardPrefabs[slotIndex];
		LoteriaCard loteriaCard = slotObject.GetComponent<LoteriaCard>();
		loteriaCard.SetCardData(cardData);

		// Update mappings
		tableGrid[slotIndex] = cardData.id;
		loteriaSlots[cardData.id] = loteriaCard;

		if (cardData.id == 0)
		{
			UpdateTokenPlacement(drawnCardData: cardData);
		}

		return true;
	}

	/// <summary>
	/// Sets a slot by finding the card with the specified ID and replacing it
	/// </summary>
	/// <param name="currentCardId">ID of the card currently in the table to replace</param>
	/// <param name="cardToSet">The new card data to set</param>
	/// <returns>True if successful, false if card ID not found</returns>
	public bool SetSlotByCardId(int currentCardId, LoteriaCardsData cardToSet)
	{
		// Validate card data
		if (cardToSet == null)
		{
			Debug.LogWarning("Cannot set slot with null card data");
			return false;
		}

		// Check if the card ID exists in the table
		if (!loteriaSlots.ContainsKey(currentCardId))
		{
			Debug.LogWarning($"Card ID {currentCardId} not found in table");
			return false;
		}

		// Find the slot index for this card ID
		int slotIndex = tableGrid.IndexOf(currentCardId);
		if (slotIndex == -1)
		{
			Debug.LogWarning($"Card ID {currentCardId} not found in grid");
			return false;
		}

		// Get the card component
		LoteriaCard loteriaCard = loteriaSlots[currentCardId];

		// Remove old mapping
		loteriaSlots.Remove(currentCardId);

		// Update the card visually
		loteriaCard.SetCardData(cardToSet);

		// Update mappings with new card ID
		tableGrid[slotIndex] = cardToSet.id;
		loteriaSlots[cardToSet.id] = loteriaCard;

		return true;
	}

	/// <summary>
	/// Swaps the cards between two slots on the table
	/// </summary>
	/// <param name="slotIndexA">First slot index (0-15)</param>
	/// <param name="slotIndexB">Second slot index (0-15)</param>
	/// <returns>True if successful, false otherwise</returns>
	public bool SwapSlots(int slotIndexA, int slotIndexB)
	{
		// Validate indices
		if (slotIndexA < 0 || slotIndexA >= TOTAL_TABLA_COUNT)
		{
			Debug.LogWarning($"Invalid slot index A: {slotIndexA}. Must be between 0 and {TOTAL_TABLA_COUNT - 1}");
			return false;
		}

		if (slotIndexB < 0 || slotIndexB >= TOTAL_TABLA_COUNT)
		{
			Debug.LogWarning($"Invalid slot index B: {slotIndexB}. Must be between 0 and {TOTAL_TABLA_COUNT - 1}");
			return false;
		}

		if (slotIndexA == slotIndexB)
		{
			Debug.LogWarning("Cannot swap a slot with itself");
			return false;
		}

		// Get card IDs at both slots
		int cardIdA = tableGrid[slotIndexA];
		int cardIdB = tableGrid[slotIndexB];

		// Get the card components
		LoteriaCard loteriaCardA = loteriaSlots[cardIdA];
		LoteriaCard loteriaCardB = loteriaSlots[cardIdB];

		// Store card data temporarily
		LoteriaCardsData tempCardData = loteriaCardA.CurrentLoteriaCardData;

		// Swap the visual cards
		loteriaCardA.SetCardData(loteriaCardB.CurrentLoteriaCardData);
		loteriaCardB.SetCardData(tempCardData);

		// Swap in the grid
		tableGrid[slotIndexA] = cardIdB;
		tableGrid[slotIndexB] = cardIdA;

		return true;
	}

	/// <summary>
	/// Gets the card data at a specific slot
	/// </summary>
	/// <param name="slotIndex">Index of the slot (0-15)</param>
	/// <returns>The card data at that slot, or null if invalid</returns>
	public LoteriaCardsData GetSlotCard(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= TOTAL_TABLA_COUNT)
		{
			Debug.LogWarning($"Invalid slot index: {slotIndex}");
			return null;
		}

		int cardId = tableGrid[slotIndex];
		return loteriaSlots[cardId].CurrentLoteriaCardData;
	}

	/// <summary>
	/// Gets the card ID at a specific slot
	/// </summary>
	/// <param name="slotIndex">Index of the slot (0-15)</param>
	/// <returns>The card ID at that slot, or -1 if invalid</returns>
	public int GetSlotCardId(int slotIndex)
	{
		if (slotIndex < 0 || slotIndex >= TOTAL_TABLA_COUNT)
		{
			Debug.LogWarning($"Invalid slot index: {slotIndex}");
			return -1;
		}

		return tableGrid[slotIndex];
	}

	#endregion

	#region Game State Updates
	public void UpdateTabla(List<LoteriaCardsData> drawnCards)
	{
		if (drawnCards == null)
		{
			Debug.Log("Called Drawn Card is null");
			return;
		}

		foreach (LoteriaCardsData drawnCard in drawnCards)
		{
			UpdateTokenPlacement(drawnCard);
		}

	}
	private void UpdateTokenPlacement(LoteriaCardsData drawnCardData)
	{
		if (!tableGrid.Contains(drawnCardData.id)) return;
		if (loteriaSlots[drawnCardData.id].TokenPlaced()) return;
		loteriaSlots[drawnCardData.id].CanPlaceToken(true);
		unmarkedSlots.Remove(drawnCardData.id);
		markedSlots.Add(drawnCardData.id);
	}

	public void UpdateScore()
	{
		CalculateScore();
		scoreUI.text = $"Score: {score}";
	}

	private void ResetTokenPlacers()
	{
		foreach (LoteriaCard card in loteriaSlots.Values)
		{
			card.CanPlaceToken(false);
		}
	}

	private void ResetTableState()
	{
		IsTableWithToken = false;
		IsTableWithCompletedRow = false;
		IsTableWithCompletedColumn = false;
		IsTableWithCompletedDiagonal = false;
		IsTableCompleted = false;
	}
	#endregion

	#region Score Calculation
	private void CalculateScore()
	{
		int CacheTokenState(bool[] tokenStates, float[] tokenMultiplier, int markedCount)
		{
			for (int i = 0; i < TOTAL_TABLA_COUNT; i++)
			{
				tokenStates[i] = loteriaSlots[tableGrid[i]].TokenPlaced();
				tokenMultiplier[i] = tokenStates[i] ? loteriaSlots[tableGrid[i]].TimerBonusMultiplier : 1;
				if (tokenStates[i])
				{
					markedCount++;

					score += singleMultiplier * tokenMultiplier[i];
				}
			}

			return markedCount;
		}

		score = 0;

		// Check all patterns in a single pass through the grid
		bool[] tokenStates = new bool[TOTAL_TABLA_COUNT];
		float[] tokenMultiplier = new float[TOTAL_TABLA_COUNT];
		int markedCount = 0;

		// Cache token states to avoid repeated lookups
		markedCount = CacheTokenState(tokenStates, tokenMultiplier, markedCount);
		IsTableWithToken = markedCount >= 1;

		// Early exit if not enough tokens for patterns
		if (markedCount < GRID_SIZE) return;


		// Check horizontal patterns
		score += ScoreHorizontalPatterns(tokenStates);

		// Check vertical patterns
		score += ScoreVerticalPatterns(tokenStates);

		// Check diagonals
		int diagonalScore = ScoreLeftDiagonal(tokenStates) + ScoreRightDiagonal(tokenStates);
		IsTableWithCompletedDiagonal = diagonalScore > 0;
		score += diagonalScore;

		// Check full board
		score += ScoreFullBoard(markedCount);


	}

	private int ScoreHorizontalPatterns(bool[] tokenStates)
	{
		int horizontalScore = 0;
		for (int row = 0; row < GRID_SIZE; row++)
		{
			if (CheckPattern(tokenStates, row * GRID_SIZE, GRID_SIZE, 1))
			{
				horizontalScore += horizontalMultiplier;
			}
		}
		IsTableWithCompletedRow = horizontalScore > 0;
		return horizontalScore;
	}

	private int ScoreVerticalPatterns(bool[] tokenStates)
	{
		int verticalScore = 0;
		for (int col = 0; col < GRID_SIZE; col++)
		{
			if (CheckPattern(tokenStates, col, GRID_SIZE, GRID_SIZE))
			{
				verticalScore += verticalMultiplier;
			}
		}
		IsTableWithCompletedColumn = verticalScore > 0;
		return verticalScore;
	}

	private int ScoreLeftDiagonal(bool[] tokenStates)
	{
		return CheckDiagonal(tokenStates, TOP_LEFT_INDEX, CENTER_LEFT_TOP_INDEX, CENTER_RIGHT_BOTTOM_INDEX, BOTTOM_RIGHT_INDEX) ? diagonalMultiplier : 0;
	}

	private int ScoreRightDiagonal(bool[] tokenStates)
	{
		return CheckDiagonal(tokenStates, TOP_RIGHT_INDEX, CENTER_RIGHT_TOP_INDEX, CENTER_LEFT_BOTTOM_INDEX, BOTTOM_LEFT_INDEX) ? diagonalMultiplier : 0;
	}

	private int ScoreFullBoard(int markedCount)
	{
		int fullScore = markedCount == TOTAL_TABLA_COUNT ? fullMultiplier : 0;
		IsTableCompleted = fullScore > 0;
		return fullScore;
	}

	private bool CheckPattern(bool[] tokenStates, int startIndex, int count, int step)
	{
		for (int i = 0; i < count; i++)
		{
			if (!tokenStates[startIndex + (i * step)])
				return false;
		}
		return true;
	}

	private bool CheckDiagonal(bool[] tokenStates, int idx1, int idx2, int idx3, int idx4)
	{
		return tokenStates[idx1] && tokenStates[idx2] && tokenStates[idx3] && tokenStates[idx4];
	}
	#endregion

	public bool LoteriaWinConditionIsMet()
	{
		return IsTableWithCompletedRow || IsTableWithCompletedColumn || IsTableWithCompletedDiagonal;
	}

	public bool IsCompleted()
	{
		return IsTableCompleted;
	}


	#region Radom Utility
	public void GenerateRandomSeed()
	{
		int seed = (int)System.DateTime.Now.Ticks;
		currentSeed = seed.ToString();
		Random.InitState(seed);
	}
	public void SetRandomSeed(string seed = "")
	{
		currentSeed = seed;
		int tempSeed = 0;
		// Source - https://stackoverflow.com/a
		// Posted by mqp, modified by community. See post 'Timeline' for change history
		// Retrieved 2025-11-12, License - CC BY-SA 4.0

		var isNumeric = int.TryParse(currentSeed, out _);

		if (isNumeric)
			tempSeed = System.Int32.Parse(seed);
		else
			tempSeed = currentSeed.GetHashCode();

		Random.InitState(tempSeed);
	}
	#endregion
}