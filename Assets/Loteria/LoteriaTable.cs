using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LoteriaTable : MonoBehaviour
{

	[SerializeField] private Cantador cantador;
	[SerializeField] private GameObject cardPrefab;
	[SerializeField] private Transform gridContainer;
	[SerializeField] private List<LoteriaCardsData> loteriaCardsData;

	[SerializeField] private List<int> loteriaSlots = new();
	[SerializeField] private List<Image> checkMarks = new();
	private const int total = 16;

	[Header("Score Tracker")]
	[SerializeField] private TextMeshProUGUI scoreUI;
	[SerializeField] private int score = 0;

	void Start()
	{
		cantador = Cantador.Instance;
		GenerateGrid();
	}

	void Update()
	{
		if (score >= 56)
		{
			GenerateGrid();
			for (int i = 0; i < checkMarks.Count; i++)
			{
				checkMarks[i].enabled = false;
			}
			score = 0;
		}
	}

	private void GenerateGrid()
	{

		// Remove all existing sprites 
		foreach (Transform t in gridContainer) { Destroy(t.gameObject); }
		// shuffled all sprites
		List<LoteriaCardsData> shuffled = Shuffle();
		//
		for (int i = 0; i < total; i++)
		{
			var currentSlot = Instantiate(cardPrefab, gridContainer.transform);
			var loteriaCard = currentSlot.GetComponent<LoteriaCard>();
			loteriaCard.InitializeLoteriaCardPrefab(shuffled[i % shuffled.Count]);
			loteriaSlots.Add(currentSlot.GetComponent<LoteriaCard>().ID);

		}
	}

	private List<LoteriaCardsData> Shuffle()
	{
		var shuffled = new List<LoteriaCardsData>(loteriaCardsData);
		for (int i = 0; i < shuffled.Count; i++)
		{
			int r = Random.Range(i, shuffled.Count);
			var tmp = shuffled[i]; shuffled[i] = shuffled[r]; shuffled[r] = tmp;
		}

		return shuffled;
	}

	public void UpdateTabla()
	{

		LoteriaCardsData drawnCard = this.cantador.DrawnCard; // get the current drawn card

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
		score += AllEqual(checkMarks) ? 16 : 0;


		scoreUI.text = $"{score}";

	}



	private int CheckHorizontal()
	{
		int horizontalMultiplier = 0;
		for (int i = 0; i < 4; i++)
		{
			var temp = checkMarks.GetRange(i * 4, 4);
			horizontalMultiplier += AllEqual(temp) ? 4 : 0;
		}

		return horizontalMultiplier;
	}

	private int CheckVertical()
	{
		int verticalMultiplier = 0;
		for (int i = 0; i < 4; i++)
		{
			List<Image> temp = new();
			for (int j = 0; j < 4; j++)
			{
				temp.Add(checkMarks[(j * 4) + i]);
			}

			verticalMultiplier += AllEqual(temp) ? 4 : 0;
		}

		return verticalMultiplier;
	}


	private int CheckLeftDiagonal()
	{
		int leftDiagonalMultiplier = 0;
		List<Image> leftDiagonal = new List<Image>(new Image[] { checkMarks[0], checkMarks[5], checkMarks[10], checkMarks[15] });
		bool allLeftDiagonal = leftDiagonal.All(el => el.enabled == true);
		leftDiagonalMultiplier += allLeftDiagonal ? 4 : 0;
		return leftDiagonalMultiplier;
	}


	private int CheckRightDiagonal()
	{
		int rightDiagonalMultiplier = 0;
		List<Image> rightDiagonal = new List<Image>(new Image[] { checkMarks[3], checkMarks[6], checkMarks[9], checkMarks[12] });
		bool allRightDiagonal = rightDiagonal.All(el => el.enabled == true);
		rightDiagonalMultiplier += allRightDiagonal ? 4 : 0;
		return rightDiagonalMultiplier;
	}



	private void UpdateCheckmarks(LoteriaCardsData drawnCard)
	{
		if (!loteriaSlots.Contains(drawnCard.id)) { return; }
		int check = loteriaSlots.IndexOf(drawnCard.id);
		checkMarks[check].enabled = true;
		return;
	}

	private bool AllEqual(List<Image> temp)
	{
		return temp.All(el => el.enabled);
	}

	#region Score Tracking
	private void CalculateScore()
	{
		// Detect
	}
	#endregion
}
