using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using DG.Tweening.Core.Easing;

public class Cantador : MonoBehaviour
{
    public static Cantador Instance { get; private set; }

    [field: Header("Game Settings")]

    // draw amount should be within range of 1 to LoteriaCard.Count
    [field: SerializeField] private int drawAmount = 4;

    [field: Header("Card Data")]
    [field: SerializeField] private List<LoteriaCardsData> loteriaDeck = new();
    [field: SerializeField] private List<LoteriaCardsData> deckLoteriaCards = new();
    [field: SerializeField] private List<LoteriaCardsData> discardLoteriaCards = new();
    public List<LoteriaCardsData> DrawnLoteriaCardsThisRound { get { return discardLoteriaCards; } }

    [field: Header("Draw Animation Settings")]
    [field: SerializeField] private float drawTime = 5f;
    [field: SerializeField] private Ease gridExpansionEase;


    [field: Header("Reveal Animation Setting")]
    [field: SerializeField] private float revealTimeDelay = 0.5f;
    [field: SerializeField] private float cardRotationSpeed = 2.5f;

    [field: Header("Discard Animation Setting")]
    [field: SerializeField] private float discardTime = 5f;

    [field: Header("UI References")]
    [field: SerializeField] private RectTransform cardDeckTransform;
    [field: SerializeField] private RectTransform drawnCardsRectTransform;
    [field: SerializeField] private GridLayoutGroup drawnCardsGridGroup;
    [field: SerializeField] private TextMeshProUGUI remainingCardsText;
    [field: SerializeField] private TextMeshProUGUI currentRoundText;
    [field: SerializeField] private TextMeshProUGUI winCondition;
    [field: SerializeField] private Slider revealSlider;
    [field: SerializeField] private List<Image> winIcons = new(3);

    [field: Header("Card Prefab")]
    [field: SerializeField] private GameObject cardPrefab;
    [field: SerializeField] private List<GameObject> displayedCardPool;

    // Game State
    private int currentRound;
    private int remainingCards;
    private int totalRemainingCards;
    private bool isReady = true;
    private bool isDeckEmpty = false;
    private bool isProcessingDraw = false;

    // Grid Layout Constants
    private const float CELL_WIDTH = 40f;
    private const float CELL_HEIGHT = 57.777f;
    private static readonly Vector2 CELL_SIZE = new Vector2(CELL_WIDTH, CELL_HEIGHT);

    // Grid Compressed
    private const float GRID_EXPANSION_DURATION = 1f;
    private const int GRID_COMPRESSED_PADDING_RIGHT = -326;
    private const int GRID_EXPANDED_PADDING_RIGHT = 0;
    private const float GRID_COMPRESSED_SPACING_X = -40f;
    private const float GRID_EXPANDED_SPACING_X = 9f;

    // Animation Constants
    private const float DISCARD_MOVE_X_OFFSET = -900f;
    private static readonly Vector3 INITIAL_CARD_SCALE = Vector3.one;
    private static readonly Vector3 CARD_FACE_DOWN_ROTATION = new Vector3(0f, -180f, 0f);
    private static readonly Vector3 CARD_FACE_UP_ROTATION = Vector3.zero;

    private Coroutine sliderCountdownCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeUI();
    }

    private void Update()
    {
        if (isDeckEmpty || isProcessingDraw) return;

        if (deckLoteriaCards.Count == 0)
        {
            EventBus.Raise(new RoundEndEvent(winState: false));
            isDeckEmpty = true;
            return;
        }

        if (CanDraw())
        {
            StartCoroutine(ProcessDrawRevealDiscardSequence());
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartEvent<LoteriaCardsData>>(OnRunStart);
        EventBus.Subscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Subscribe<RoundEndEvent>(OnRoundEnd);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartEvent<LoteriaCardsData>>(OnRunStart);
        EventBus.Unsubscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Unsubscribe<RoundEndEvent>(OnRoundEnd);

        if (sliderCountdownCoroutine != null)
        {
            StopCoroutine(sliderCountdownCoroutine);
        }
    }

    #region Initialization

    private void InitializeUI()
    {
        if (drawnCardsGridGroup != null)
        {
            drawnCardsGridGroup.cellSize = CELL_SIZE;
        }

        DeactivateAllCardsInPool();

        if (winCondition != null)
        {
            winCondition.text = "";
        }

        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
            revealSlider.minValue = 0f;
            revealSlider.maxValue = 1f;
            revealSlider.value = 1f;
        }

        foreach (Image icon in winIcons)
        {
            icon.color = Color.red;
            icon.fillAmount = 0;
        }
    }

    #endregion

    #region Event Handlers

    private void OnRunStart(RunStartEvent<LoteriaCardsData> runStartEvent)
    {
        loteriaDeck = runStartEvent.CurrentDeck;
        remainingCards = loteriaDeck.Count;
        totalRemainingCards = loteriaDeck.Count;
        UpdateCardCountText();
    }

    private void OnRoundStart(RoundStartEvent roundStartEvent)
    {
        currentRound = roundStartEvent.Round;
        isDeckEmpty = false;
        isProcessingDraw = false;
        ShuffleEntireDeck();

        if (currentRoundText != null)
        {
            currentRoundText.text = $"Round: {currentRound}";
        }

        if (winCondition != null)
        {
            int winNumber = roundStartEvent.TargetLength;
            string condition = roundStartEvent.WinTableState switch
            {
                TablaWinningRuleState.ROW => "Rows",
                TablaWinningRuleState.COLUMNS => "Columns",
                TablaWinningRuleState.DIAGONAL => "Diagonals",
                TablaWinningRuleState.FULL => "Full",
                _ => ""
            };
            winCondition.text = $"{winNumber} {condition}";
        }

        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
        }
    }

    private void OnRoundEnd(RoundEndEvent roundEndEvent)
    {
        StopAllCoroutines();
        StartCoroutine(DiscardCardsCoroutine());

        int index = currentRound - 1;
        if (index < 0 || index >= winIcons.Count) return;

        Sequence winIconSequence = DOTween.Sequence();
        Color targetColor = roundEndEvent.Win ? Color.green : Color.red;

        winIconSequence.Append(winIcons[index].DOFillAmount(1, 1.5f));
        winIconSequence.Join(winIcons[index].DOColor(targetColor, 0.5f));
    }

    #endregion

    #region Draw-Reveal-Discard Sequence

    private IEnumerator ProcessDrawRevealDiscardSequence()
    {
        isProcessingDraw = true;
        isReady = false;
        drawAmount = Mathf.Min(drawAmount, deckLoteriaCards.Count);

        yield return StartCoroutine(DrawCardsCoroutine());
        yield return StartCoroutine(RevealCardsCoroutine());
        yield return StartCoroutine(DiscardCardsCoroutine());

        isProcessingDraw = false;
        isReady = true;
    }

    private IEnumerator DrawCardsCoroutine()
    {
        if (drawAmount <= 0 || drawAmount > displayedCardPool.Count)
        {
            Debug.LogError($"Invalid drawn cards count: {drawAmount}");
            yield break;
        }

        remainingCards -= drawAmount;
        UpdateCardCountText();

        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
        }

        SetGridToCompressedState();
        ActivateDisplayCardsInPool(drawAmount);
        DeactivateUnusedCards(drawAmount);

        // Wait for grid expansion animation to complete
        bool expansionComplete = false;
        DrawCardSequence(drawTime, () => expansionComplete = true);

        yield return new WaitUntil(() => expansionComplete);
    }

    private IEnumerator RevealCardsCoroutine()
    {
        List<LoteriaCardsData> drawnCards = ChooseCards(drawAmount);
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
        }

        // Set card data for each active card
        for (int i = 0; i < drawnCards.Count && i < displayedCardPool.Count; i++)
        {
            LoteriaCard card = displayedCardPool[i].GetComponent<LoteriaCard>();
            card.SetCardData(drawnCards[i]);
        }

        // Wait for reveal sequence to complete
        bool revealComplete = false;
        RevealCardsSequence(revealTimeDelay, cardRotationSpeed, () => revealComplete = true);

        yield return new WaitUntil(() => revealComplete);
    }

    private IEnumerator DiscardCardsCoroutine()
    {
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(true);
            revealSlider.value = 1f;
        }

        if (sliderCountdownCoroutine != null)
        {
            StopCoroutine(sliderCountdownCoroutine);
        }
        sliderCountdownCoroutine = StartCoroutine(AnimateSliderCountdown(discardTime));

        // Wait for discard sequence to complete
        bool discardComplete = false;
        DiscardCardsSequence(discardTime, () => discardComplete = true);

        yield return new WaitUntil(() => discardComplete);
    }

    #endregion

    #region Card Selection

    private List<LoteriaCardsData> ChooseCards(int drawnCount)
    {
        List<LoteriaCardsData> data = new();

        for (int i = 0; i < drawnCount; i++)
        {
            int index = Random.Range(0, deckLoteriaCards.Count);
            LoteriaCardsData cardData = deckLoteriaCards[index];
            data.Add(cardData);

            deckLoteriaCards.RemoveAt(index);
            discardLoteriaCards.Add(cardData);
        }

        return data;
    }

    #endregion

    #region Deck Handling

    private void ShuffleEntireDeck()
    {
        var shuffled = new List<LoteriaCardsData>(loteriaDeck);
        ShuffleCards(shuffled);
        deckLoteriaCards = shuffled;

        discardLoteriaCards.Clear();
    }

    private static void ShuffleCards(List<LoteriaCardsData> shuffled)
    {
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
    }

    #endregion

    #region UI Animations

    private void SetGridToCompressedState()
    {
        if (drawnCardsGridGroup == null) return;

        RectOffset padding = drawnCardsGridGroup.padding;
        padding.right = GRID_COMPRESSED_PADDING_RIGHT;
        drawnCardsGridGroup.padding = padding;
        drawnCardsGridGroup.spacing = new Vector2(GRID_COMPRESSED_SPACING_X, 0);
    }

    private void DrawCardSequence(float animationDuration, System.Action onComplete)
    {
        if (drawnCardsGridGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        Sequence layoutSequence = DOTween.Sequence();

        layoutSequence.Append(
            DOTween.To(
                () => drawnCardsGridGroup.padding.right,
                paddingValue =>
                {
                    var padding = drawnCardsGridGroup.padding;
                    padding.right = (int)paddingValue;
                    drawnCardsGridGroup.padding = padding;
                },
                GRID_EXPANDED_PADDING_RIGHT,
                animationDuration
            ).SetEase(Ease.OutQuad)
        );
        layoutSequence.AppendInterval(0.5f);

        layoutSequence.Append(
            DOTween.To(
                () => drawnCardsGridGroup.spacing.x,
                spacingValue =>
                {
                    drawnCardsGridGroup.spacing = new Vector2(spacingValue, drawnCardsGridGroup.spacing.y);
                },
                GRID_EXPANDED_SPACING_X,
                animationDuration
            ).SetEase(Ease.OutQuad)
        );

        layoutSequence.OnComplete(() => onComplete?.Invoke());
    }

    private void RevealCardsSequence(float delayBetweenCardReveals, float cardRotationSpeed, System.Action onComplete)
    {
        Sequence revealSequence = DOTween.Sequence();
        // For Each active card in scene
        foreach (GameObject card in displayedCardPool)
        {
            if (!card.activeSelf) continue;

            RectTransform cardRectTransform = card.GetComponent<RectTransform>();
            LoteriaCardsData cardData = card.GetComponent<LoteriaCard>().CurrentLoteriaCardData;

            revealSequence.AppendInterval(delayBetweenCardReveals);
            revealSequence.Append(
                cardRectTransform.DOLocalRotateQuaternion(
                    Quaternion.Euler(CARD_FACE_UP_ROTATION),
                    cardRotationSpeed
                )
            );
            revealSequence.AppendCallback(() =>
            {
                EventBus.Raise(new RevealSingleCardEvent<LoteriaCardsData>(cardData));
            });
        }

        revealSequence.OnComplete(() => onComplete?.Invoke());
    }

    private void DiscardCardsSequence(float totalDiscardTime, System.Action onComplete)
    {
        Sequence discardSequence = DOTween.Sequence();

        int activeCardCount = 0;
        foreach (GameObject card in displayedCardPool)
        {
            if (card.activeSelf) activeCardCount++;
        }

        float discardTimePerCard = activeCardCount > 0 ? totalDiscardTime / activeCardCount : totalDiscardTime;

        foreach (GameObject card in displayedCardPool)
        {
            if (!card.activeSelf) continue;

            RectTransform cardRectTransform = card.GetComponent<RectTransform>();
            discardSequence.Append(
                cardRectTransform.DOLocalMoveX(DISCARD_MOVE_X_OFFSET, discardTimePerCard, true)
            );
        }

        discardSequence.OnComplete(() =>
        {
            DeactivateAllCardsInPool();

            if (revealSlider != null)
            {
                revealSlider.gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        });
    }

    private IEnumerator AnimateSliderCountdown(float duration)
    {
        if (revealSlider == null) yield break;

        float elapsedTime = 0f;
        revealSlider.value = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            revealSlider.value = 1f - (elapsedTime / duration);
            yield return null;
        }

        revealSlider.value = 0f;
    }

    #endregion

    #region Utility Methods

    private bool CanDraw()
    {
        return isReady;
    }

    private void UpdateCardCountText()
    {
        if (remainingCardsText != null)
        {
            remainingCardsText.text = $"{remainingCards} / {totalRemainingCards}";
        }
    }

    private void DeactivateUnusedCards(int activeCardCount)
    {
        for (int i = activeCardCount; i < displayedCardPool.Count; i++)
        {
            displayedCardPool[i].SetActive(false);
        }
    }

    private void DeactivateAllCardsInPool()
    {
        foreach (GameObject card in displayedCardPool)
        {
            card.SetActive(false);
        }
    }

    private void ActivateDisplayCardsInPool(int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            GameObject displayedCard = displayedCardPool[i];
            RectTransform cardRectTransform = displayedCard.GetComponent<RectTransform>();
            cardRectTransform.localRotation = Quaternion.Euler(CARD_FACE_DOWN_ROTATION);
            cardRectTransform.localScale = INITIAL_CARD_SCALE;
            displayedCard.SetActive(true);
        }
    }

    #endregion
}