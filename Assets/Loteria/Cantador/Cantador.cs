using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;
using System;
using System.Collections;
using System.Xml.Schema;


public class Cantador : MonoBehaviour
{
    public static Cantador Instance { get; private set; }
    [SerializeField] private int drawAmount = 4;

    public int DrawAmount { get { return drawAmount; } set { drawAmount = value; } }

    [Header("Card Data")]

    private int _initalTotal;
    [SerializeField] private List<LoteriaCardsData> loteriaDeck = new();
    [SerializeField] private List<LoteriaCardsData> deckLoteriaCards = new();
    [SerializeField] private List<LoteriaCardsData> discardLoteriaCards = new();

    public List<LoteriaCardsData> DrawnLoteriaCardsThisRound { get { return discardLoteriaCards; } private set { discardLoteriaCards = value; } }
    public List<LoteriaCardsData> DrawnLoteriaCardsThisTurn = new();

    [Header("Timer Settings")]
    [SerializeField] private Slider timeSlot;
    [SerializeField] private float drawTime = 3f;   // duration between draws
    [SerializeField] private float refillSpeed = 2f;

    // states
    private float timer;
    private bool isDrawingCard;
    private bool isReady = true;

    [SerializeField] private Transform drawingCardTransform;

    [SerializeField] private TextMeshProUGUI turnUI;




    [Header("Events")]
    public UnityEvent OnCardDrawn;
    public UnityEvent OnGameStartDeckReset;
    public UnityEvent OnMidRoundDeckReShuffle;

    public static event Action<int, int> OnUpdateRemainingCards;
    public static event Action<List<LoteriaCardsData>> OnDrawingCards;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartEvent>(OnRunStart);
        EventBus.Subscribe<RoundStartEvent>(OnRoundStart);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartEvent>(OnRunStart);
        EventBus.Unsubscribe<RoundStartEvent>(OnRoundStart);
    }

    private void OnRunStart(RunStartEvent runStartEvent)
    {
        loteriaDeck = runStartEvent.CurrentDeck;
    }


    private void OnRoundStart(RoundStartEvent roundStartEvent)
    {

        _initalTotal = this.loteriaDeck.Count;
        ResetShuffleToNewGame();
    }

    public void DrawCards()
    {
        DrawnLoteriaCardsThisTurn.Clear();
        int remainingCards = deckLoteriaCards.Count;
        int drawnCount = drawAmount > remainingCards ? remainingCards : drawAmount;


        OnUpdateRemainingCards?.Invoke(remainingCards - drawAmount, _initalTotal);

        for (int i = 0; i < drawnCount; i++)
        {
            // Draw a random card
            int index = UnityEngine.Random.Range(0, deckLoteriaCards.Count);
            LoteriaCardsData cardData = deckLoteriaCards[index];

            // update decks respectively
            deckLoteriaCards.RemoveAt(index);
            discardLoteriaCards.Add(cardData);

            DrawnLoteriaCardsThisTurn.Add(cardData);
            DrawnLoteriaCardsThisRound.Add(cardData);

        }

        OnDrawingCards?.Invoke(DrawnLoteriaCardsThisTurn); // call display to drawn selected card

    }

    private bool CanDraw()
    {
        if (isDrawingCard) return false;
        if (!isReady) return false;
        if (deckLoteriaCards.Count < 1) ResetShuffleToNewGame();
        return true;
    }


    // shuffles the entire deck when round starts
    private void ResetShuffleToNewGame()
    {
        var shuffled = new List<LoteriaCardsData>(loteriaDeck); // create copy
        ShuffleCards(shuffled);
        deckLoteriaCards = shuffled; // set current deck to loteria deck

        discardLoteriaCards.Clear();
        DrawnLoteriaCardsThisTurn.Clear();
    }



    // shuffles undrawn cards in deck
    private static void ShuffleCards(List<LoteriaCardsData> shuffled)
    {
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
    }

}
