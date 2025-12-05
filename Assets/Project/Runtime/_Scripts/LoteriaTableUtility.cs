using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public struct TableState
{
	public int TableGridSize { get; set; }
	private const int threshold = 0;
	public List<bool> tokenStates;

	public int TotalTokensOnBoard
	{
		get
		{
			if (tokenStates == null) return 0;

			int count = 0;
			for (int i = 0; i < tokenStates.Count; i++)
			{
				count += tokenStates[i] ? 1 : 0;
			}
			return count;
		}
	}

	public int RowsCompleted { get; set; }
	public int ColumnsCompleted { get; set; }
	public int DiagonalsCompleted { get; set; }

	public bool IsTableWithCompletedRow()
	{
		return RowsCompleted > threshold;
	}

	public bool IsTableWithCompletedColumn()
	{
		return ColumnsCompleted > threshold;
	}

	public bool IsTableWithCompletedDiagonal()
	{
		return DiagonalsCompleted > threshold;
	}

	public bool IsTableCompleted()
	{
		return RowsCompleted == TableGridSize && ColumnsCompleted == TableGridSize;
	}

	public void ResetTableState()
	{
		int totalCells = TableGridSize * TableGridSize;
		tokenStates = new List<bool>(totalCells);

		// Initialize all tokens as false
		for (int i = 0; i < totalCells; i++)
		{
			tokenStates.Add(false);
		}

		RowsCompleted = threshold;
		ColumnsCompleted = threshold;
		DiagonalsCompleted = threshold;
	}
}

public struct TableLayoutConfig
{
	public int GridSize { get; private set; }
	public int CellWidth { get; private set; }
	public int CellHeight { get; private set; }
	public int CanvasWidth { get; private set; }
	public int CanvasHeight { get; private set; }
	public int CardsActive { get; private set; }
	public int PaddingSize { get; private set; }

	public Vector2 Spacing { get; private set; }

	public Vector2 CellSize => new Vector2(CellWidth, CellHeight);
	public Vector2 CanvasSize => new Vector2(CanvasWidth, CanvasHeight);
	public RectOffset RectOffset => new RectOffset(PaddingSize, PaddingSize, PaddingSize, PaddingSize);

	public TableLayoutConfig(int gridSize, int cellWidth, int cellHeight, int canvasWidth, int canvasHeight, int paddingSize = 5, Vector2 spacing = new())
	{
		GridSize = gridSize;
		CellWidth = cellWidth;
		CellHeight = cellHeight;
		CanvasWidth = canvasWidth;
		CanvasHeight = canvasHeight;
		CardsActive = gridSize * gridSize;
		PaddingSize = paddingSize;
		Spacing = spacing;
	}

	// Preset configurations
	public static TableLayoutConfig Create3x3()
	{
		return new TableLayoutConfig(
			gridSize: 3,
			cellWidth: 50,
			cellHeight: 50,
			canvasWidth: 180,
			canvasHeight: 180,
			paddingSize: 5,
			spacing: new Vector2(10,10)
		);
	}

	public static TableLayoutConfig Create4x4()
	{
		return new TableLayoutConfig(
			gridSize: 4,
			cellWidth: 30,
			cellHeight: 40,
			canvasWidth: 170,
			canvasHeight: 200,
			paddingSize: 5,
			spacing: new Vector2(13,10)
		);
	}
}


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


public static class TablaScorePoints
{
	private const int INIT_SINGLE = 1;
	private const int INIT_HORIZONTAL = 4;
	private const int INIT_VERTICAL = 4;
	private const int INIT_DIAGONAL = 8;
	private const int INIT_FULL = 16;

	public static int SingleMultiplier { get; set; } = INIT_SINGLE;
	public static int HorizontalMultiplier { get; set; } = INIT_HORIZONTAL;
	public static int VerticalMultiplier { get; set; } = INIT_VERTICAL;
	public static int DiagonalMultiplier { get; set; } = INIT_DIAGONAL;
	public static int FullMultiplier { get; set; } = INIT_FULL;

	// Call this if you want to reset to default values
	public static void ResetToDefaults()
	{
		SingleMultiplier = INIT_SINGLE;
		HorizontalMultiplier = INIT_HORIZONTAL;
		VerticalMultiplier = INIT_VERTICAL;
		DiagonalMultiplier = INIT_DIAGONAL;
		FullMultiplier = INIT_FULL;
	}
}
