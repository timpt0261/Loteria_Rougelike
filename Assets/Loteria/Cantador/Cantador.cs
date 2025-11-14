using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using UnityEngine.Rendering;

public class Cantador : MonoBehaviour
{
    public static Cantador Instance { get; private set; }
    [SerializeField] private int drawAmount = 4;

    [Header("Card Data")]
    [SerializeField] private List<LoteriaCardsData> loteriaDeck = new();
    public void SetLoteriaDeck(List<LoteriaCardsData> newloteriaDeck) => this.loteriaDeck = newloteriaDeck;
    [SerializeField] private List<LoteriaCardsData> deckLoteriaCards = new();
    [SerializeField] private List<LoteriaCardsData> discardLoteriaCards = new();

    [SerializeField] private int turnCount = 0;
    public List<LoteriaCardsData> DrawnLoteriaCardsThisRound { get { return discardLoteriaCards; } private set { discardLoteriaCards = value; } }
    private List<LoteriaCardsData> DrawnLoteriaCardsThisTurn = new();

    [Header("Timer Settings")]
    [SerializeField] private Slider timeSlot;
    [SerializeField] private float drawTime = 3f;   // duration between draws
    [SerializeField] private float refillSpeed = 2f;

    // states
    private float timer;
    private bool isDrawingCard;
    private bool isReady = true;

    [SerializeField] private List<Image> drawingCardSlot;
    [SerializeField] private Transform drawingCardTransform;

    [SerializeField] private TextMeshProUGUI turnUI;

    [Header("Shuffle")]

    [SerializeField] private int shufflesRemaining = 3;

    private float shuffleCost = 2f;


    [Header("Events")]
    public UnityEvent OnCardDrawn;
    public UnityEvent OnGameStartDeckReset;
    public UnityEvent OnMidRoundDeckReShuffle;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        if (drawingCardSlot == null)
        {
            drawingCardSlot = new List<Image>(drawingCardTransform.GetComponentsInChildren<Image>());
        }

    }

    public void Initialize()
    {
        turnCount = 0;
        ResetShuffleToNewGame();
        ResetTimer();
    }

    void Update()
    {
        // turnUI.text = $"Turn: {turnCount}";
        // TryDraw();
        // HandleTimer();
    }

    private void HandleTimer()
    {
        // If drawing, count down the timer
        if (isDrawingCard)
        {
            timer -= Time.deltaTime;

            // Update the UI slider to represent remaining time
            timeSlot.value = Mathf.Clamp01(timer / drawTime);

            if (timer <= 0f)
            {
                // Finished cooldown → ready to draw again
                isDrawingCard = false;
                isReady = true;
                timeSlot.value = 1f;
            }
        }
    }

    public void TryDraw()
    {
        int HUNDRED = 100;
        int seed = (int)System.DateTime.Now.Ticks + HUNDRED;
        Random.InitState(seed);

        // StartTimer();

        for (int i = 0; i < drawAmount; i++)
        {
            
            if (deckLoteriaCards.Count <= 3)
            {
                ResetShuffleToNewGame();
            }

            // Draw a random card
            int index = Random.Range(0, deckLoteriaCards.Count);

            DrawnLoteriaCardsThisTurn.Add(deckLoteriaCards[index]);
            Sprite drawnCard = DrawnLoteriaCardsThisTurn[i].sprite;
            deckLoteriaCards.RemoveAt(index);
            discardLoteriaCards.Add(DrawnLoteriaCardsThisTurn[i]);
            drawingCardSlot[i].sprite = drawnCard;

        }
        OnCardDrawn?.Invoke();
        DrawnLoteriaCardsThisTurn.Clear();
    }

    private void StartTimer()
    {
        isDrawingCard = true;
        isReady = false;
        timer = drawTime;
        timeSlot.value = 1f; // start full, deplete over time
    }

    private void ResetTimer()
    {
        isDrawingCard = false;
        isReady = true;
        timer = 0f;
        timeSlot.value = 1f;
    }

    private bool CanDraw()
    {
        if (isDrawingCard) return false;
        if (!isReady) return false;
        if (deckLoteriaCards.Count < 1) ResetShuffleToNewGame();
        return true;
    }


    // shuffles the entire deck when round starts
    public void ResetShuffleToNewGame()
    {
        var shuffled = new List<LoteriaCardsData>(loteriaDeck); // create copy
        ShuffleCards(shuffled);
        deckLoteriaCards = shuffled; // set current deck to loteria deck

        discardLoteriaCards.Clear();
        DrawnLoteriaCardsThisTurn.Clear();
        OnGameStartDeckReset?.Invoke();
    }



    // shuffles undrawn cards in deck
    public void ReShuffleRemainingCards()
    {
        if (shufflesRemaining <= 0) { return; }
        shufflesRemaining--;
        int length = deckLoteriaCards.Count;
        if (length < drawAmount) return;
        ShuffleCards(deckLoteriaCards);
        OnMidRoundDeckReShuffle?.Invoke();
    }


    private static void ShuffleCards(List<LoteriaCardsData> shuffled)
    {
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
    }

    public void ResetShufflesRemaining(int newShufflesRemaining = 3)
    {
        this.shufflesRemaining = newShufflesRemaining;
    }
    public int GetShuffleChargesRemaining()
    {
        return this.shufflesRemaining;
    }
}
