using System.Collections.Generic;

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
