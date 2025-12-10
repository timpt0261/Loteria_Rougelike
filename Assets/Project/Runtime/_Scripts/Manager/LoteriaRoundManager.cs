using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoteriaRoundManager : MonoBehaviour
{
    private enum ROUND_STATE { START, CANTADOR_DRAW, PLAYER_DRAW, END };
    [Header("UI")]
    [SerializeField] private Image BackGroundImage;
    [SerializeField] private TextMeshProUGUI CurrentRound_UI;
    [SerializeField] private Button startRound_Button;

    [Header("References")]
    [SerializeField] private List<LoteriaCardsData> allLoteriaCards;
    [SerializeField] private Cantador cantador;
    [SerializeField] private LoteriaTabla loteriaTable;
    [SerializeField] private CharmManager charmManager;

    [Header("Game Stats")]

    [SerializeField] private AnimationCurve levelProgression;
    [SerializeField] private Int32 currentLevel;
    public Int32 CurrentLevel
    {
        get { return currentLevel; }
    }
    [SerializeField] private float roundScore;
    [SerializeField] private int totalRounds = 3;

    private Int32 _currentRound;
    public int CurrentRound
    {
        get { return _currentRound; }

        set
        {
            if (value < 1) { _currentRound = 1; return; }
            if (value > totalRounds) { _currentRound = totalRounds; return; }
        }

    }

    private TablaWinningRuleState _winTableState;
    private int _targetLength;

    [Header("Randomization")]
    [SerializeField] private string currentSeed;

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
    private void Awake()
    {
        // BackGroundImage.enabled = false;
    }

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
        _currentRound = MIN_TOTAL_LEVELS;
        // deterime win condtion
        (_winTableState, _targetLength) = DetermineWinningCondition();
        // Sequence sequence = DOTween.Sequence();
        // CurrentRound_UI.text = $"Round {CurrentRound}";
        // Vector2 currentSize = BackGroundImage.rectTransform.sizeDelta;
        // BackGroundImage.rectTransform.position = new Vector3(-currentSize.x, 0, 0);
        // sequence.Append(BackGroundImage.rectTransform.DOAnchorPos3D(new Vector3(currentSize.x, 0, 0), 5, true));
        // startRound_Button.interactable = false;
        EventBus.Raise(new RoundStartEvent(_currentRound, _winTableState, _targetLength));
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