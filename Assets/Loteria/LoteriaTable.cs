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

	private int score = 0;
	public int Score => score;

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
		ResetCheckMarks();
		SetTable();
		score = 0;
	}

	private void SetTable()
	{
		List<LoteriaCardsData> shuffled = Shuffle();

		for (int i = 0; i < TOTAL_TABLA_COUNT; i++)
		{
			var currentSlot = cardPrefabs[i];
			var loteriaCard = currentSlot.GetComponent<LoteriaCard>();
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
	#endregion

	#region Game State Updates
	public void UpdateTabla(LoteriaCardsData drawnCard)
	{
		if (drawnCard == null)
		{
			Debug.Log("Called Drawn Card is null");
			return;
		}

		UpdateTokenPlacement(drawnCard);

		if (IsCompleted())
		{
			OnTableCompleted?.Invoke();
		}

		CalculateScore();
		scoreUI.text = $"{score}";
	}

	private void UpdateTokenPlacement(LoteriaCardsData drawnCard)
	{
		if (!tableGrid.Contains(drawnCard.id)) return;

		loteriaSlots[drawnCard.id].PlaceToken(true);
	}

	private void ResetCheckMarks()
	{
		foreach (LoteriaCard card in loteriaSlots.Values)
		{
			card.PlaceToken(false);
		}
	}
	#endregion

	#region Score Calculation
	private void CalculateScore()
	{
		score = 0;
		score += CheckIndividual();
		score += CheckHorizontal();
		score += CheckVertical();
		score += CheckLeftDiagonal();
		score += CheckRightDiagonal();
		score += AllPositionMarked(tableGrid) ? fullMultiplier : 0;
	}

	private int CheckIndividual()
	{
		int points = 0;
		
		foreach (var id in tableGrid)
		{
			points += loteriaSlots[id].TokenPlaced() ? singleMultiplier : 0;
		}
		
		return points;
	}

	private int CheckHorizontal()
	{
		int points = 0;

		for (int row = 0; row < GRID_SIZE; row++)
		{
			var rowIds = tableGrid.GetRange(row * GRID_SIZE, GRID_SIZE);
			points += AllPositionMarked(rowIds) ? horizontalMultiplier : 0;
		}

		return points;
	}

	private int CheckVertical()
	{
		int points = 0;

		for (int col = 0; col < GRID_SIZE; col++)
		{
			List<int> columnIds = new();

			for (int row = 0; row < GRID_SIZE; row++)
			{
				columnIds.Add(tableGrid[(row * GRID_SIZE) + col]);
			}

			points += AllPositionMarked(columnIds) ? verticalMultiplier : 0;
		}

		return points;
	}

	private int CheckLeftDiagonal()
	{
		List<int> diagonalIds = new List<int>
		{
			tableGrid[TOP_LEFT_INDEX],
			tableGrid[CENTER_LEFT_TOP_INDEX],
			tableGrid[CENTER_RIGHT_BOTTOM_INDEX],
			tableGrid[BOTTOM_RIGHT_INDEX]
		};

		return AllPositionMarked(diagonalIds) ? diagonalMultiplier : 0;
	}

	private int CheckRightDiagonal()
	{
		List<int> diagonalIds = new List<int>
		{
			tableGrid[TOP_RIGHT_INDEX],
			tableGrid[CENTER_RIGHT_TOP_INDEX],
			tableGrid[CENTER_LEFT_BOTTOM_INDEX],
			tableGrid[BOTTOM_LEFT_INDEX]
		};

		return AllPositionMarked(diagonalIds) ? diagonalMultiplier : 0;
	}

	private bool AllPositionMarked(List<int> cardIds)
	{
		foreach (int id in cardIds)
		{
			if (!loteriaSlots[id].TokenPlaced())
				return false;
		}

		return true;
	}

	public bool IsCompleted()
	{
		return AllPositionMarked(tableGrid);
	}
	#endregion
}