using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoteriaGameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<LoteriaCardsData> allLoteriaCards;
    [SerializeField] private Cantador cantador;
    [SerializeField] private LoteriaTable loteriaTable;

    [Header("Game Stats")]
    [SerializeField] private float debt;
    [SerializeField] private float playerCash;
    [SerializeField] private float roundScore;
    [SerializeField] private int turnsRemaining;
    [SerializeField] private int roundCount;
    [SerializeField] private int reshufflesRemaining;


    private const int MAX_SHUFFLE_CHARGES = 3;
    private const int MAX_TURNS_PER_ROUND = 7;
    private const int INITIAL_PLAYER_CASH = 2;

    [Header("UI")]

    [SerializeField] private TextMeshProUGUI playerCash_UI;
    [SerializeField] private TextMeshProUGUI turnCount_UI;

    // Game Over
    [SerializeField] private GameObject gameOverElement;
    [SerializeField] private TextMeshProUGUI gameOver_UI;

    // Shop
    [SerializeField] private GameObject shopUIElement;

    [SerializeField] private TextMeshProUGUI round_UI;
    // set game condtion to win
    // game over works
    // shows up after round is completed
    void Awake()
    {
        InitializeRound();
        gameOverElement.SetActive(false);
        shopUIElement.SetActive(false);
    }

    void Start()
    {
        cantador = Cantador.Instance;
        loteriaTable = LoteriaTable.Instance;
        SetLoteriaCardReference();
        cantador.Initialize();

    }

    void Update()
    {
        UpdateUI();
        // call onnce not on update
        // if (this.loteriaTable.LoteriaWinConditionIsMet())
        // {
        //     ProcessRoundEnd();
        //     return;
        // }

        // // game over if the turns end
        if (turnsRemaining <= 0)
        {
            ProcessGameOver();
            return;
        }


    }

    #region Initialization
    private void InitializeRound()
    {
        reshufflesRemaining = MAX_SHUFFLE_CHARGES;
        turnsRemaining = MAX_TURNS_PER_ROUND;
        roundCount = 0;
        playerCash = INITIAL_PLAYER_CASH;
        debt = 0f;
        roundScore = 0f;
    }

    private void SetLoteriaCardReference()
    {
        cantador.SetLoteriaDeck(allLoteriaCards);
        loteriaTable.SetLoteriaDeck(allLoteriaCards);
    }
    #endregion




    #region State Actions
    private void SetupNewRound()
    {
        roundCount++;
        turnsRemaining = MAX_TURNS_PER_ROUND;
        roundScore = 0f;

        cantador.Initialize();
        loteriaTable.ResetTable();

        reshufflesRemaining = cantador.GetShuffleChargesRemaining();
    }

    private void ProcessRoundEnd()
    {
        // Calculate final score and cash earned
        roundScore = loteriaTable.Score;
        playerCash += roundScore;
        Debug.Log($"Round {roundCount} completed! Score: {roundScore}, Total Cash: {playerCash}");
        OpenShop();

    }



    private void OpenShop()
    {
        Debug.Log("Opening shop...");
        // Shop UI logic here
        shopUIElement.SetActive(true);
    }

    private void ProcessGameOver()
    {
        if (gameOverElement.activeInHierarchy) return;
        gameOverElement.SetActive(true);
        gameOver_UI.text = $"Game Over\n Final Cash: {playerCash}\n Rounds Completed: {roundCount} ";
        Debug.Log($"Game Over! Final Cash: {playerCash}, Rounds Completed: {roundCount}");
    }
    #endregion

    #region Public State Control

    public void RestartGame()
    {
        InitializeRound();
    }
    #endregion

    #region Cantador Events
    public void HandleCardDrawn()
    {
        loteriaTable.UpdateTabla(cantador.DrawnLoteriaCardsThisRound);
        turnsRemaining--;
    }

    public void HandleTableCompleted()
    {
        cantador.ResetShufflesRemaining();
    }

    public void HandleDeckShuffleOnStart()
    {
        loteriaTable.ResetTable();
    }

    public void HandleDeckShuffleMidRound()
    {
        reshufflesRemaining = cantador.GetShuffleChargesRemaining();
    }
    #endregion

    #region  Loteria Table

    public void HandleLoteriaWinCondition()
    {
        ProcessRoundEnd();
    }

    public void HandleLoteriaLoseCondtion()
    {

    }
    #endregion

    #region Loteria Card Events
    public void HandleOnCardSet()
    {
        // Card initialization complete
    }

    public void HandleCanTokenBePlaced()
    {
        // Token placement is now available
    }

    public void HandleWhenTokenPlaced()
    {
        loteriaTable.UpdateScore();
    }
    #endregion

    #region UI Updates
    private void UpdateUI()
    {
        if (turnCount_UI != null)
        {
            turnCount_UI.text = $"Turns: {turnsRemaining}/{MAX_TURNS_PER_ROUND}";
        }

        if (round_UI != null)
        {
            round_UI.text = $"Round : {roundCount}";
        }

        if (playerCash_UI != null)
        {
            playerCash_UI.text = $"${playerCash}";
        }


    }
    #endregion


}