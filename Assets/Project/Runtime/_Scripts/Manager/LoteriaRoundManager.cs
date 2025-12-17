using System;
using System.Collections.Generic;
using UnityEngine;

// get current round to increment by 1 in setup new round, must be within range of ( min round to max). if increment by max then reset to 0
public class LoteriaRoundManager : MonoBehaviour
{
    private enum ROUND_STATE { START, CANTADOR_DRAW, PLAYER_DRAW, END };
    [field: Header("References")]
    [field: SerializeField] private List<LoteriaCardsData> allLoteriaCards;
    [field: SerializeField] private Cantador cantador;
    [field: SerializeField] private LoteriaTabla loteriaTable;
    [field: SerializeField] private CharmManager charmManager;

    [field: Header("Game Stats")]

    [field: SerializeField] private AnimationCurve levelProgression;
    [field: SerializeField] private Int32 currentLevel;
    public Int32 CurrentLevel
    {
        get { return currentLevel; }
    }
    [field: SerializeField] private float roundScore;
    [field: SerializeField] private int totalRounds = 3;

    [field: SerializeField] private Int32 currentRound = 0;
    public int CurrentRound
    {
        get { return currentRound; }

        set
        {
            if (value < 0) { currentRound = 0; return; }
            if (value > totalRounds) { currentRound = 0; return; }
            currentRound = value;
        }

    }

    [field: Header("Round Win Condition")]
    [field: SerializeField] private TablaWinningRuleState winTableState;
    [field: SerializeField] private int targetLength;

    [field: Header("Randomization")]
    [field: SerializeField] private string currentSeed;

    //const variables
    private const int MIN_TOTAL_LEVELS = 1;
    private const int MAX_TOTAL_LEVELS = 3;

    private const int EAST_LEVEL_MAX = 1; // level 1 should be easiest

    private const int MIDDLE_LEVEL_MAX = 2; // Middle Difficulty

    private const int DIFFICULT_LEVELS_MAX = 3; // Most Difficulty

    private const int INIT_TOTAL_ROUNDS = 3;
    private const int MAX_TOTAL_ROUNDS = 9;
    private const int INIT_GRID_SIZE = 3;

    // set game condtion to win
    // game over works
    // shows up after round is completed
    private void OnEnable()
    {
        EventBus.Subscribe<RoundEndEvent>(OnRoundEnd);
    }


    private void OnDisable()
    {
        EventBus.Unsubscribe<RoundEndEvent>(OnRoundEnd);

    }

    void Start()
    {
        cantador = Cantador.Instance;
        loteriaTable = LoteriaTabla.Instance;
        charmManager = CharmManager.Instance;
        StartRun();

    }

    #region State Actions
    public void StartRun()
    {
        GenerateRandomNumber(); // Generate Random Number
        currentLevel = (int)levelProgression.Evaluate(0);
        EventBus.Raise(new RunStartEvent<LoteriaCardsData>(INIT_GRID_SIZE, allLoteriaCards));

    }


    public void SetUpNewRound()
    {
        CurrentRound++;
        // deterime win condtion
        (winTableState, targetLength) = DetermineWinningCondition();
        EventBus.Raise(new RoundStartEvent(CurrentRound, winTableState, targetLength));
    }


    private void OnRoundEnd(RoundEndEvent roundEndEvent)
    {
        bool winState = roundEndEvent.Win;
        if (winState)
        {
            // pop-up store to buy stuff
            return;
        }

        // make x pop in 
    }

    #endregion

    #region Win Condition
    private (TablaWinningRuleState, int) DetermineWinningCondition()
    {
        TablaWinningRuleState choosenState = (TablaWinningRuleState)DiceRoll(2) - 1; // assume it's a 3x by 3
        int numberToComplete = 1; // value between 1-3

        return (choosenState, numberToComplete);
    }
    #endregion


    #region Random Utility

    private int DiceRoll(int d)
    {
        return UnityEngine.Random.Range(1, d);
    }
    private void GenerateRandomNumber()
    {
        int seed = Mathf.Abs(System.DateTime.Now.Date.ToLongTimeString().GetHashCode());
        currentSeed = seed.ToString();
        UnityEngine.Random.InitState(seed);
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

        UnityEngine.Random.InitState(tempSeed);
    }
    #endregion


}