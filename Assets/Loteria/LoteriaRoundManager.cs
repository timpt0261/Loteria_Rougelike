using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoteriaRoundManager : MonoBehaviour
{
    private enum ROUNDSTATE{ ROUND_START, DRAW, REVEAL, ROUND_END}

    [Header("References")]
    [SerializeField] private List<LoteriaCardsData> allLoteriaCards;
    [SerializeField] private Cantador cantador;
    [SerializeField] private LoteriaTable loteriaTable;

    [SerializeField] private CharmManager charmManager;

    [Header("Game Stats")]
    [SerializeField] private float debt;
    [SerializeField] private float playerCash;
    [SerializeField] private float roundScore;
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private int reshufflesRemaining;


    private const int MAX_SHUFFLE_CHARGES = 3;
    private const int MAX_TURNS_PER_ROUND = 7;
    private const int INITIAL_PLAYER_CASH = 2;

    // set game condtion to win
    // game over works
    // shows up after round is completed
    void Awake()
    {
        InitializeRound();
    }

    void Start()
    {
        cantador = Cantador.Instance;
        loteriaTable = LoteriaTable.Instance;
        charmManager = CharmManager.Instance;
        SetLoteriaCardReference();
        SetupNewRound();
    }

    #region Initialization
    private void InitializeRound()
    {
        totalRounds = 0;
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
        totalRounds++;
        roundScore = 0f;

        cantador.Initialize();
        loteriaTable.ResetTable();

        
        charmManager.OnRoundStart?.Invoke();
    }

    private void ProcessRoundEnd()
    {
        // Calculate final score and cash earned
        roundScore = loteriaTable.Score;
        playerCash += roundScore;
        Debug.Log($"Round {totalRounds} completed! Score: {roundScore}, Total Cash: {playerCash}");
        charmManager.OnRoundEnd?.Invoke();
        //OpenShop();

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
    }



    public void HandleDeckShuffleOnStart()
    {
        loteriaTable.ResetTable();
    }

    public void HandleDeckShuffleMidRound()
    {
        
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
    #endregion


}