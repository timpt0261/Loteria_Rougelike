using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms.Impl;

public class Cantador : MonoBehaviour
{
    public static Cantador Instance { get; private set; }
    [SerializeField] private int drawAmount = 4;

    [Header("Card Data")]
    [SerializeField] private List<LoteriaCardsData> loteriaDeck = new();
    public void SetLoteriaDeck(List<LoteriaCardsData> newloteriaDeck) => this.loteriaDeck = newloteriaDeck;
    [SerializeField] private List<LoteriaCardsData> deckLoteriaCards = new();
    [SerializeField] private List<LoteriaCardsData> discardLoteriaCards = new();

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

    [Header("Events")]
    public UnityEvent OnCardDrawn;
    public UnityEvent OnDeckShuffled;


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
        Shuffle();
        ResetTimer();
    }

    void Update()
    {
        TryDraw();
        HandleTimer();
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
        int seed = (int)System.DateTime.Now.Ticks + 100;
        Random.InitState(seed);

        if (!CanDraw()) return;

        // Start cooldown
        StartTimer();

        for (int i = 0; i < drawAmount; i++)
        {
            if (deckLoteriaCards.Count <= 1)
            {
                Shuffle();
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
        if (deckLoteriaCards.Count < 1) Shuffle();
        return true;
    }

    public void Shuffle()
    {
        var shuffled = new List<LoteriaCardsData>(loteriaDeck);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
        deckLoteriaCards = shuffled;
        discardLoteriaCards.Clear();
        DrawnLoteriaCardsThisTurn.Clear();
        OnDeckShuffled?.Invoke();
    }


}
