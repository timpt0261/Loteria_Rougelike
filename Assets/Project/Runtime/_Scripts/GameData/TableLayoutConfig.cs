using UnityEngine;

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
			spacing: new Vector2(10, 10)
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
			spacing: new Vector2(13, 10)
		);
	}
}
