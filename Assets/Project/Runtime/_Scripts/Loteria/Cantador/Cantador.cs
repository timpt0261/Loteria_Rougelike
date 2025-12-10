using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;



public class Cantador : MonoBehaviour
{
    public static Cantador Instance { get; private set; }
    [SerializeField] private int drawAmount = 4;

    public int DrawAmount { get { return drawAmount; } set { drawAmount = value; } }

    [Header("Card Data")]
    private int _initialTotal;
    [SerializeField] private List<LoteriaCardsData> loteriaDeck = new();
    [SerializeField] private List<LoteriaCardsData> deckLoteriaCards = new();
    [SerializeField] private List<LoteriaCardsData> discardLoteriaCards = new();

    public List<LoteriaCardsData> DrawnLoteriaCardsThisRound { get { return discardLoteriaCards; } private set { discardLoteriaCards = value; } }
    public List<LoteriaCardsData> DrawnLoteriaCardsThisTurn = new();

    [Header("Timer Settings")]
    [SerializeField] private float drawTime = 5f;
    [SerializeField] private float revealTimeDelay = .5f;
    [SerializeField] private float cardRotationSpeed = 2.5f;
    [SerializeField] private float discardTime = 5f;

    

    private int currentRound;

    // states
    private float timer;
    private bool isDrawingCard;
    private bool isReady = true;
    private bool IsDeckEmpty = false;
    private bool isProcessingDraw = false; // Prevent overlapping draws

    private bool revealComplete = false;
    private bool discardComplete = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (IsDeckEmpty || isProcessingDraw)
        {
            return;
        }

        // Check if deck is empty
        if (deckLoteriaCards.Count == 0)
        {   // player loses
            EventBus.Raise(new RoundEndEvent(winState: false));
            IsDeckEmpty = true;
            return;
        }

        if (CanDraw())
        {
            StartCoroutine(ProcessDrawRevealDiscardSequence());
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartEvent>(OnRunStart);
        EventBus.Subscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Subscribe<RevealDrawnCardsCompleteEvent>(OnRevealDrawnCardsComplete);
        EventBus.Subscribe<DiscardCardsCompleteEvent>(OnDiscardCardsComplete);
        EventBus.Subscribe<RoundEndEvent>(OnRoundEnd);
    }


    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartEvent>(OnRunStart);
        EventBus.Unsubscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Unsubscribe<RevealDrawnCardsCompleteEvent>(OnRevealDrawnCardsComplete);
        EventBus.Unsubscribe<DiscardCardsCompleteEvent>(OnDiscardCardsComplete);
        EventBus.Unsubscribe<RoundEndEvent>(OnRoundEnd);
    }

    private void OnRunStart(RunStartEvent runStartEvent)
    {
        loteriaDeck = runStartEvent.CurrentDeck;
        _initialTotal = loteriaDeck.Count;
    }

    private void OnRoundStart(RoundStartEvent roundStartEvent)
    {
        currentRound = roundStartEvent.Round;
        IsDeckEmpty = false;
        isProcessingDraw = false;
        ShuffleEntireDeck();
    }


    private void OnRoundEnd(RoundEndEvent roundEndEvent)
    {
        StopAllCoroutines();
        StartCoroutine(DiscardCards());
    }

    #region Draw-Reveal-Discard Sequence

    private IEnumerator ProcessDrawRevealDiscardSequence()
    {
        isProcessingDraw = true;
        isReady = false;

        // STEP 1: DRAW
        yield return StartCoroutine(DrawCards());

        // STEP 2: REVEAL (waits for completion via event)
        yield return StartCoroutine(RevealCards());

        // STEP 3: DISCARD (waits for completion via event)
        yield return StartCoroutine(DiscardCards());

        // Ready for next draw
        isProcessingDraw = false;
        isReady = true;
    }

    private IEnumerator DrawCards()
    {
        // Calculate how many cards to draw
        int remainingCards = deckLoteriaCards.Count;
        int drawnCount = drawAmount > remainingCards ? remainingCards : drawAmount;

        // Raise event to animate the drawing of cards (UI handles animation)
        EventBus.Raise(new DrawCardsEvent(drawnCount, drawTime));

        // Wait for draw animation duration
        yield return new WaitForSeconds(drawTime);
    }

    private IEnumerator RevealCards()
    {
        // Calculate how many cards to reveal
        int remainingCards = deckLoteriaCards.Count;
        int drawnCount = drawAmount > remainingCards ? remainingCards : drawAmount;

        // Choose which cards are being revealed
        List<LoteriaCardsData> drawnCards = ChooseCards(drawnCount);

        // Add to round tracking
        DrawnLoteriaCardsThisRound.AddRange(drawnCards);

        // Raise event with the actual card data to be revealed
        EventBus.Raise(new RevealDrawnCardsEvent(revealTimeDelay, cardRotationSpeed, drawnCards));

        // Wait for reveal to complete (triggered by Cantador_UI finishing animations)
        yield return new WaitUntil(() => revealComplete);

        revealComplete = false; // Reset flag
    }

    private IEnumerator DiscardCards()
    {
        // Raise event to discard cards
        EventBus.Raise(new DiscardCardEvent(discardTime));

        // Wait for discard to complete (triggered by Cantador_UI finishing animations)
        yield return new WaitUntil(() => discardComplete);

        discardComplete = false; // Reset flag
    }

    #endregion

    #region Card Selection

    private List<LoteriaCardsData> ChooseCards(int drawnCount)
    {
        List<LoteriaCardsData> data = new();

        for (int i = 0; i < drawnCount; i++)
        {
            // Draw a random card
            int index = UnityEngine.Random.Range(0, deckLoteriaCards.Count);
            LoteriaCardsData cardData = deckLoteriaCards[index];
            data.Add(cardData);

            // Update decks respectively
            deckLoteriaCards.RemoveAt(index);
            discardLoteriaCards.Add(cardData);
        }

        return data;
    }

    #endregion

    #region Event Completion Handlers



    private void OnRevealDrawnCardsComplete(RevealDrawnCardsCompleteEvent evt)
    {
        revealComplete = true;
    }

    private void OnDiscardCardsComplete(DiscardCardsCompleteEvent evt)
    {
        discardComplete = true;
    }

    #endregion

    #region Timer Management

    private void StartTimer()
    {
        isDrawingCard = true;
        isReady = false;
        timer = drawTime;
    }

    private void HandleTimer()
    {
        if (isDrawingCard)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                isDrawingCard = false;
                isReady = true;
            }
        }
    }

    private bool CanDraw()
    {
        if (isDrawingCard) return false;
        if (!isReady) return false;
        return true;
    }

    #endregion

    #region Deck Handling

    private void ShuffleEntireDeck()
    {
        var shuffled = new List<LoteriaCardsData>(loteriaDeck);
        ShuffleCards(shuffled);
        deckLoteriaCards = shuffled;

        discardLoteriaCards.Clear();
        DrawnLoteriaCardsThisTurn.Clear();
    }

    private static void ShuffleCards(List<LoteriaCardsData> shuffled)
    {
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = UnityEngine.Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
    }

    #endregion
}