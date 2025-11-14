using System.Collections.Generic;
using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoteriaTable : MonoBehaviour
{
	public static LoteriaTable Instance { get; private set; }

	[Header("Table Configuration")]
	[SerializeField] private Transform gridContainer;
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

	#region Unity Lifecycle
	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;
	}
	#endregion

	#region Table Setup
	public void ResetTable()
	{
		tableGrid.Clear();
		loteriaSlots.Clear();
		ResetTokenPlacers();
		ResetTableState();
		SetTable();
		score = 0;

	}

	void Update()
	{
		// if (IsCompleted())
		// {
		// 	OnTableCompleted?.Invoke();
		// }

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

	private void SwitchSlots(int cardSlotA, int cardSlotB)
	{
		int keyA = tableGrid[cardSlotA];
		int keyB = tableGrid[cardSlotB];

		var tempCard = loteriaSlots[keyA];
		LoteriaCard loteriaCardA = loteriaSlots[keyA];
		LoteriaCard loteriaCardB = loteriaSlots[keyB];

		// switch card data
		// set a to b
		loteriaCardA.SetCardData(loteriaCardB.CurrentLoteriaCardData);

		// set b to a
		loteriaCardB.SetCardData(tempCard.CurrentLoteriaCardData);

		int temp = tableGrid[cardSlotA];
		tableGrid[cardSlotA] = tableGrid[cardSlotB];
		tableGrid[cardSlotB] = temp;
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
	private void UpdateTokenPlacement(LoteriaCardsData drawnCard)
	{
		if (!tableGrid.Contains(drawnCard.id)) return;
		loteriaSlots[drawnCard.id].CanPlaceToken(true);
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