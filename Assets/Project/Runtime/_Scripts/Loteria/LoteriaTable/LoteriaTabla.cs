using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoteriaTabla : MonoBehaviour
{
	public static LoteriaTabla Instance { get; private set; }

	[Header("Table Configuration")]
	[SerializeField] private RectTransform rootCanvas;
	[SerializeField] private GridLayoutGroup loteriaTableGroup;
	private TablaGridLayout _grid_3x3;
	private TablaGridLayout _grid_4x4;
	[SerializeField] private List<LoteriaCardsData> loteriaDeck = new();
	[SerializeField] private List<GameObject> cardPrefabs = new();

	// Layout configurations
	private TableLayoutConfig layout3x3;
	private TableLayoutConfig layout4x4;
	private TableLayoutConfig currentLayout;

	// Score Handling
	private float score = 0;
	public float Score => score;
	private TableState tableState;
	private Dictionary<int, LoteriaCard> loteriaSlots = new();
	private List<int> tableGrid = new(); // Track card IDs in grid order

	// Win Condition 
	private TablaWinningRuleState winState;
	private int numberToWin;
	private int currentGridSize = 3; // Start with 3x3

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;

		if (rootCanvas == null) { rootCanvas = GetComponent<RectTransform>(); }

		// Initialize layout configurations
		layout3x3 = TableLayoutConfig.Create3x3();
		layout4x4 = TableLayoutConfig.Create4x4();
	}

	private void OnEnable()
	{
		EventBus.Subscribe<RunStartEvent>(OnRunStart);
		EventBus.Subscribe<RoundStartEvent>(OnRoundStart);
		EventBus.Subscribe<RevealSingleCardEvent>(UpdateTokenPlacement);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe<RunStartEvent>(OnRunStart);
		EventBus.Unsubscribe<RoundStartEvent>(OnRoundStart);
		EventBus.Unsubscribe<RevealSingleCardEvent>(UpdateTokenPlacement);
	}

	#region Unity Lifecycle
	private void Update()
	{
		if (IsWinConditionMet())
		{
			EventBus.Raise(new RoundEndEvent());
			tableState.ResetTableState();
		}
	}
	#endregion

	#region Event Bus
	private void OnRunStart(RunStartEvent runStartEvent)
	{
		currentGridSize = runStartEvent.GridSize;
		loteriaDeck = runStartEvent.CurrentDeck;

		// Initialize table state with grid size
		tableState = new TableState();
		tableState.TableGridSize = currentGridSize;
		tableState.ResetTableState();
		GenerateTabla(currentGridSize);
	}

	private void OnRoundStart(RoundStartEvent roundStartEvent)
	{
		winState = roundStartEvent.WinTableState;
		numberToWin = roundStartEvent.TargetLength;
	}

	private void OnRoundEnd(RoundEndEvent roundEndEvent)
	{
		tableState.ResetTableState();
		ResetTokenPlacers();
	}

	private void UpdateTokenPlacement(RevealSingleCardEvent revealCardEvent)
	{
		LoteriaCardsData drawnCardData = revealCardEvent.drawnCardData;
		if (!tableGrid.Contains(drawnCardData.id)) return;
		if (loteriaSlots[drawnCardData.id].TokenPlaced()) return;

		loteriaSlots[drawnCardData.id].CanPlaceToken(true);

		// Update score after token placement
		CalculateScore();
	}
	#endregion

	#region Table Generation
	private void GenerateTabla(int gridSize)
	{
		// Configure UI Grid Layout based on grid size
		ConfigureGridLayout(gridSize);

		List<LoteriaCardsData> shuffled = Shuffle();
		int totalGridSize = gridSize * gridSize;

		tableGrid.Clear();
		loteriaSlots.Clear();

		for (int i = 0; i < totalGridSize; i++)
		{
			GameObject currentSlot = cardPrefabs[i];
			currentSlot.SetActive(true);

			LoteriaCard loteriaCard = currentSlot.GetComponent<LoteriaCard>();
			loteriaCard.SetCardData(shuffled[i % shuffled.Count]);

			int cardId = loteriaCard.ID;
			tableGrid.Add(cardId);
			loteriaSlots.Add(cardId, loteriaCard);
		}

		// Deactivate unused cards
		for (int i = totalGridSize; i < cardPrefabs.Count; i++)
		{
			cardPrefabs[i].SetActive(false);
		}
	}

	private void ConfigureGridLayout(int gridSize)
	{
		if (loteriaTableGroup == null)
		{
			Debug.LogError("GridLayoutGroup is not assigned!");
			return;
		}

		// Select the appropriate layout configuration
		currentLayout = gridSize == 3 ? layout3x3 : layout4x4;

		// Apply layout settings to UI Grid Layout Group
		loteriaTableGroup.cellSize = currentLayout.CellSize;
		loteriaTableGroup.spacing = currentLayout.Spacing;
		loteriaTableGroup.padding = currentLayout.RectOffset;
		loteriaTableGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		loteriaTableGroup.constraintCount = currentLayout.GridSize;

		// Set canvas size
		if (rootCanvas != null)
		{
			rootCanvas.sizeDelta = currentLayout.CanvasSize;
		}

		// Store current layout configuration in TablaGridLayout
		if (gridSize == 3)
		{
			_grid_3x3 = new TablaGridLayout(
				currentLayout.RectOffset,
				currentLayout.CellSize,
				Vector2.zero,
				cardPrefabs,
				currentLayout.CardsActive
			);
		}
		else if (gridSize == 4)
		{
			_grid_4x4 = new TablaGridLayout(
				currentLayout.RectOffset,
				currentLayout.CellSize,
				Vector2.zero,
				cardPrefabs,
				currentLayout.CardsActive
			);
		}
		else
		{
			Debug.LogWarning($"Unsupported grid size: {gridSize}. Only 3x3 and 4x4 are supported.");
		}

		// Force layout rebuild
		LayoutRebuilder.ForceRebuildLayoutImmediate(loteriaTableGroup.GetComponent<RectTransform>());
	}

	private List<LoteriaCardsData> Shuffle()
	{
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

	public void UpgradeToFourByFour()
	{
		const int FOUR = 4;
		if (currentGridSize == FOUR) return;

		currentGridSize = FOUR;
		tableState.TableGridSize = FOUR;
		tableState.ResetTableState();
		GenerateTabla(FOUR);

	}
	#endregion

	#region Game State Updates


	public void UpdateScore()
	{
		CalculateScore();
	}

	private void ResetTokenPlacers()
	{
		foreach (LoteriaCard card in loteriaSlots.Values)
		{
			card.CanPlaceToken(false);
		}
	}
	#endregion

	#region Score Calculation
	private void CalculateScore()
	{
		score = 0;
		int totalCells = currentGridSize * currentGridSize;

		// Initialize token states based on current grid size
		tableState.tokenStates = new List<bool>(totalCells);
		List<float> tokenMultipliers = new List<float>(totalCells);

		// Cache token states
		int markedCount = CacheTokenState(tokenMultipliers);

		// Early exit if not enough tokens
		if (markedCount < currentGridSize) return;

		// Calculate scores for different patterns
		score += ScoreHorizontalPatterns();
		score += ScoreVerticalPatterns();
		score += ScoreDiagonals();
		score += ScoreFullBoard(markedCount);
	}

	private int CacheTokenState(List<float> tokenMultipliers)
	{
		int markedCount = 0;
		int totalCells = currentGridSize * currentGridSize;

		tableState.tokenStates.Clear();
		tokenMultipliers.Clear();

		for (int i = 0; i < totalCells; i++)
		{
			int cardId = tableGrid[i];
			bool hasToken = loteriaSlots[cardId].TokenPlaced();
			float multiplier = hasToken ? loteriaSlots[cardId].TimerBonusMultiplier : 1f;

			tableState.tokenStates.Add(hasToken);
			tokenMultipliers.Add(multiplier);

			if (hasToken)
			{
				markedCount++;
				score += TablaScorePoints.SingleMultiplier * multiplier;
			}
		}

		return markedCount;
	}

	private int ScoreHorizontalPatterns()
	{
		int horizontalScore = 0;
		tableState.RowsCompleted = 0;

		for (int row = 0; row < currentGridSize; row++)
		{
			if (CheckPattern(row * currentGridSize, currentGridSize, 1))
			{
				horizontalScore += TablaScorePoints.HorizontalMultiplier;
				tableState.RowsCompleted++;
			}
		}

		return horizontalScore;
	}

	private int ScoreVerticalPatterns()
	{
		int verticalScore = 0;
		tableState.ColumnsCompleted = 0;

		for (int col = 0; col < currentGridSize; col++)
		{
			if (CheckPattern(col, currentGridSize, currentGridSize))
			{
				verticalScore += TablaScorePoints.VerticalMultiplier;
				tableState.ColumnsCompleted++;
			}
		}

		return verticalScore;
	}

	private int ScoreDiagonals()
	{
		int diagonalScore = 0;
		tableState.DiagonalsCompleted = 0;

		// Left to right diagonal (top-left to bottom-right)
		if (CheckLeftDiagonal())
		{
			diagonalScore += TablaScorePoints.DiagonalMultiplier;
			tableState.DiagonalsCompleted++;
		}

		// Right to left diagonal (top-right to bottom-left)
		if (CheckRightDiagonal())
		{
			diagonalScore += TablaScorePoints.DiagonalMultiplier;
			tableState.DiagonalsCompleted++;
		}

		return diagonalScore;
	}

	private int ScoreFullBoard(int markedCount)
	{
		int totalCells = currentGridSize * currentGridSize;
		return markedCount == totalCells ? TablaScorePoints.FullMultiplier : 0;
	}

	private bool CheckPattern(int startIndex, int count, int step)
	{
		for (int i = 0; i < count; i++)
		{
			int index = startIndex + (i * step);
			if (index >= tableState.tokenStates.Count || !tableState.tokenStates[index])
				return false;
		}
		return true;
	}

	private bool CheckLeftDiagonal()
	{
		// Check from top-left (0) to bottom-right
		for (int i = 0; i < currentGridSize; i++)
		{
			int index = i * currentGridSize + i; // 0, 4, 8 for 3x3 or 0, 5, 10, 15 for 4x4
			if (index >= tableState.tokenStates.Count || !tableState.tokenStates[index])
				return false;
		}
		return true;
	}

	private bool CheckRightDiagonal()
	{
		// Check from top-right to bottom-left
		for (int i = 0; i < currentGridSize; i++)
		{
			int index = i * currentGridSize + (currentGridSize - 1 - i); // 2, 4, 6 for 3x3 or 3, 6, 9, 12 for 4x4
			if (index >= tableState.tokenStates.Count || !tableState.tokenStates[index])
				return false;
		}
		return true;
	}
	#endregion

	private bool IsWinConditionMet()
	{
		return winState switch
		{
			TablaWinningRuleState.ROW => numberToWin == tableState.RowsCompleted,
			TablaWinningRuleState.COLUMNS => numberToWin == tableState.ColumnsCompleted,
			TablaWinningRuleState.DIAGONAL => numberToWin == tableState.DiagonalsCompleted,
			TablaWinningRuleState.FULL => tableState.IsTableCompleted(),
			_ => false,
		};
	}
}