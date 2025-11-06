using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoteriaTable : MonoBehaviour
{
	public static LoteriaTable Instance { get; private set; }
	[SerializeField] private Transform gridContainer;
	[SerializeField] private List<LoteriaCardsData> allloteriaCardsData;


	[SerializeField] private List<GameObject> cardPrefabs;
	[SerializeField] private List<int> tableGrid = new();
	[SerializeField] private Dictionary<int, LoteriaCard> loteriaSlots = new();
	private const int TOTAL_TABLA_COUNT = 16;

	[Header("Score Tracker")]
	[SerializeField] private TextMeshProUGUI scoreUI;
	[SerializeField] private int score = 0;

	[SerializeField] private int horizontalMultiplier = 4;
	[SerializeField] private int verticalMultiplier = 4;

	[SerializeField] private int diagonalMultiplier = 8;

	[SerializeField] private int fullMultiplier = 16;


	public int Score => score;

	[Header("Events")]
	public UnityEvent OnTableCompleted;

	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}
		Instance = this;
	}

	void Start()
	{
		SetTable();
	}

	public void ResetTable()
	{
		ResetCheckMarks();
		SetTable();
		score = 0;
	}
	private void SetTable()
	{

		// shuffled all sprites
		List<LoteriaCardsData> shuffled = Shuffle();
		//
		for (int i = 0; i < TOTAL_TABLA_COUNT; i++)
		{
			var currentSlot = cardPrefabs[i];
			var loteriaCard = currentSlot.GetComponent<LoteriaCard>();
			loteriaCard.SetCardData(shuffled[i % shuffled.Count]);
			int key = loteriaCard.ID;
			tableGrid.Add(key);
			loteriaSlots.Add(key, loteriaCard);


		}
	}

	private List<LoteriaCardsData> Shuffle()
	{
		var shuffled = new List<LoteriaCardsData>(allloteriaCardsData);
		for (int i = 0; i < shuffled.Count; i++)
		{
			int r = Random.Range(i, shuffled.Count);
			var tmp = shuffled[i]; shuffled[i] = shuffled[r]; shuffled[r] = tmp;
		}

		return shuffled;
	}

	public void UpdateTabla(LoteriaCardsData drawnCard)
	{

		if (drawnCard == null)
		{
			Debug.Log("Called Drawn Card is null");
			return;
		}

		if (IsCompleted())
		{
			OnTableCompleted?.Invoke();
		}

		// update cross on all horizontal rows

		// update the check mark 
		UpdateCheckmarks(drawnCard);
		score = 0;

		// update score 

		// check horizontal
		score += CheckHorizontal();
		// check vertical
		score += CheckVertical();
		// Check left diagonal
		score += CheckLeftDiagonal();
		// Check right diagonal
		score += CheckRightDiagonal();

		// Check all 
		score += AllPostionMarked(tableGrid) ? TOTAL_TABLA_COUNT : 0;


		scoreUI.text = $"{score}";

	}



	#region  

	private int CheckHorizontal()
	{
		int horizontalMultiplier = 0;
		for (int i = 0; i < 4; i++)
		{
			var temp = tableGrid.GetRange(i * 4, 4);
			horizontalMultiplier += AllPostionMarked(tableGrid) ? 4 : 0;
		}

		return horizontalMultiplier;
	}

	private int CheckVertical()
	{
		int verticalMultiplier = 0;
		for (int i = 0; i < 4; i++)
		{
			List<int> temp = new();
			for (int j = 0; j < 4; j++)
			{
				temp.Add(tableGrid[(j * 4) + i]);
			}

			verticalMultiplier += AllPostionMarked(temp) ? 4 : 0;
		}

		return verticalMultiplier;
	}


	private int CheckLeftDiagonal()
	{
		int leftDiagonalMultiplier = 0;
		List<int> leftDiagonal = new List<int>(new int[] { tableGrid[0], tableGrid[5], tableGrid[10], tableGrid[15] });
		bool allLeftDiagonal = AllPostionMarked(leftDiagonal);
		leftDiagonalMultiplier += allLeftDiagonal ? 4 : 0;
		return leftDiagonalMultiplier;
	}


	private int CheckRightDiagonal()
	{
		int rightDiagonalMultiplier = 0;
		List<int> rightDiagonal = new List<int>(new int[] { tableGrid[3], tableGrid[6], tableGrid[9], tableGrid[12] });
		bool allRightDiagonal = AllPostionMarked(rightDiagonal);
		rightDiagonalMultiplier += allRightDiagonal ? 4 : 0;
		return rightDiagonalMultiplier;
	}

	private void ResetCheckMarks()
	{
		foreach (LoteriaCard el in loteriaSlots.Values)
		{
			el.PlaceToken(false);
		}
	}

	private void UpdateCheckmarks(LoteriaCardsData drawnCard)
	{
		if (!tableGrid.Contains(drawnCard.id)) { return; }
		loteriaSlots[drawnCard.id].PlaceToken(true);
		return;
	}

	#endregion
	private bool AllPostionMarked(List<int> postions)
	{
		foreach (int id in postions)
		{
			if (!loteriaSlots[id].TokenPlaced())
				return false;
		}

		return true;
	}

	#region Score Tracking
	private void CalculateScore()
	{
		// Detect
	}

	public bool IsCompleted()
	{
		return AllPostionMarked(tableGrid);
	}
	#endregion
}
