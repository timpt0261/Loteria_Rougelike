using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class Cantador_UI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform cardDeckTransform;
    [SerializeField] private RectTransform drawnCardsRectTransform;
    [SerializeField] private GridLayoutGroup drawnCardsGridGroup;
    [SerializeField] private TextMeshProUGUI remainingCardsText;
    [SerializeField] private TextMeshProUGUI currentRoundText;
    [SerializeField] private Slider revealSlider;

    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<GameObject> displayedCardPool;

    // Grid Layout Constants
    private const float CELL_WIDTH = 40f;
    private const float CELL_HEIGHT = 57.777f;
    private static readonly Vector2 CELL_SIZE = new Vector2(CELL_WIDTH, CELL_HEIGHT);

    // Animation Constants
    private const float GRID_EXPANSION_DURATION = 1f;
    private const int GRID_COMPRESSED_PADDING_RIGHT = -326;
    private const int GRID_EXPANDED_PADDING_RIGHT = 0;
    private const float GRID_COMPRESSED_SPACING_X = -40f;
    private const float GRID_EXPANDED_SPACING_X = 9f;

    private const float DISCARD_MOVE_X_OFFSET = -900f;
    private const float DISCARD_PUNCH_DURATION = 1f;

    private static readonly Vector3 INITIAL_CARD_SCALE = Vector3.one;
    private static readonly Vector3 CARD_FACE_DOWN_ROTATION = new Vector3(0f, -180f, 0f);
    private static readonly Vector3 CARD_FACE_UP_ROTATION = Vector3.zero;
    private static readonly Vector3 DISCARD_PUNCH_ROTATION = new Vector3(2f, 0f, 2f);

    // Slider state
    private Coroutine sliderCountdownCoroutine;

    private void Awake()
    {
        InitializeComponents();
        ConfigureGridLayout();
        DeactivateAllCardsInPool();

        // Initialize slider
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
            revealSlider.minValue = 0f;
            revealSlider.maxValue = 1f;
            revealSlider.value = 1f;
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartEvent>(OnRunStart);
        EventBus.Subscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Subscribe<DrawCardsEvent>(OnDrawCards);
        EventBus.Subscribe<RevealDrawnCardsEvent>(OnRevealCards);
        EventBus.Subscribe<DiscardCardEvent>(OnDiscardCards);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<RunStartEvent>(OnRunStart);
        EventBus.Unsubscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Unsubscribe<DrawCardsEvent>(OnDrawCards);
        EventBus.Unsubscribe<RevealDrawnCardsEvent>(OnRevealCards);
        EventBus.Unsubscribe<DiscardCardEvent>(OnDiscardCards);

        // Clean up coroutine if running
        if (sliderCountdownCoroutine != null)
        {
            StopCoroutine(sliderCountdownCoroutine);
        }
    }

    private void InitializeComponents()
    {
        if (drawnCardsRectTransform == null)
            drawnCardsRectTransform = GetComponent<RectTransform>();

        if (drawnCardsGridGroup == null)
            drawnCardsGridGroup = GetComponent<GridLayoutGroup>();

        if (remainingCardsText == null)
            remainingCardsText = GameObject.Find("RemainingCardText").GetComponent<TextMeshProUGUI>();

        if (currentRoundText == null)
            currentRoundText = GameObject.Find("CurrentRoundText").GetComponent<TextMeshProUGUI>();
    }

    private void ConfigureGridLayout()
    {
        drawnCardsGridGroup.cellSize = CELL_SIZE;
    }

    private void DeactivateAllCardsInPool()
    {
        foreach (GameObject card in displayedCardPool)
        {
            card.SetActive(false);
        }
    }

    private void UpdateRemainingCardsText(int remainingCards, int totalCards)
    {
        remainingCardsText.text = $"{remainingCards} / {totalCards}";
    }

    #region Event Handling

    private void OnRunStart(RunStartEvent runStartEvent)
    {
        int total = runStartEvent.CurrentDeck.Count;
        UpdateRemainingCardsText(total, total);
    }

    private void OnRoundStart(RoundStartEvent roundStartEvent)
    {
        currentRoundText.text = $"Round: {roundStartEvent.Round}";

        // Hide slider at round start
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
        }
    }

    private void OnDrawCards(DrawCardsEvent drawCardEvent)
    {
        int drawnAmount = drawCardEvent.DrawnAmount;
        float displayAnimationDuration = drawCardEvent.DrawTime;

        if (drawnAmount > displayedCardPool.Count || drawnAmount == 0)
        {
            Debug.LogError($"Invalid drawn cards count: {drawnAmount}. Must be between 1 and {displayedCardPool.Count}");
            return;
        }

        // Hide slider during draw
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
        }

        // Animate the draw phase (expand grid, activate cards face-down)
        SetGridToCompressedState();
        ActivateDisplayCardsInPool(drawnAmount);
        DeactivateUnusedCards(drawnAmount);
        AnimateGridExpansion(displayAnimationDuration);
    }

    private void OnRevealCards(RevealDrawnCardsEvent revealDrawnCardsEvent)
    {
        float delayBetweenReveal = revealDrawnCardsEvent.DelayTimeBetweenIntervals;
        float cardRotationSpeed = revealDrawnCardsEvent.CardRotationSpeed;
        List<LoteriaCardsData> drawnCards = revealDrawnCardsEvent.drawnCardsData;

        // Hide slider during reveal
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(false);
        }

        // Set card data for each active card
        for (int i = 0; i < drawnCards.Count; i++)
        {
            if (i >= displayedCardPool.Count) break;

            LoteriaCard card = displayedCardPool[i].GetComponent<LoteriaCard>();
            card.SetCardData(drawnCards[i]);
        }

        // Play reveal animation sequence
        RevealCardsSequence(delayBetweenReveal, cardRotationSpeed);
    }

    private void OnDiscardCards(DiscardCardEvent discardCardEvent)
    {
        float discardTime = discardCardEvent.DiscardTime;

        // Show and initialize slider
        if (revealSlider != null)
        {
            revealSlider.gameObject.SetActive(true);
            revealSlider.value = 1f; // Start at full
        }

        // Start slider countdown
        if (sliderCountdownCoroutine != null)
        {
            StopCoroutine(sliderCountdownCoroutine);
        }
        sliderCountdownCoroutine = StartCoroutine(AnimateSliderCountdown(discardTime));

        // Play discard animation
        DiscardCardsSequence(discardTime);
    }

    #endregion

    #region Draw Animation

    private void SetGridToCompressedState()
    {
        var padding = drawnCardsGridGroup.padding;
        padding.right = GRID_COMPRESSED_PADDING_RIGHT;
        drawnCardsGridGroup.padding = padding;

        drawnCardsGridGroup.spacing = new Vector2(GRID_COMPRESSED_SPACING_X, 0);
    }

    private void AnimateGridExpansion(float animationDuration)
    {
        Sequence layoutSequence = DOTween.Sequence();

        // Animate padding expansion
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

        // Animate spacing expansion (simultaneously)
        layoutSequence.Join(
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
    }

    #endregion

    #region Reveal Animation

    private void RevealCardsSequence(float delayBetweenCardReveals, float cardRotationSpeed)
    {
        Sequence revealSequence = DOTween.Sequence();

        foreach (GameObject card in displayedCardPool)
        {
            if (!card.activeSelf) continue;

            RectTransform cardRectTransform = card.GetComponent<RectTransform>();
            LoteriaCardsData cardData = card.GetComponent<LoteriaCard>().CurrentLoteriaCardData;

            // Delay before revealing this card
            revealSequence.AppendInterval(delayBetweenCardReveals);

            // Rotate card face-up
            revealSequence.Append(
                cardRectTransform.DOLocalRotateQuaternion(
                    Quaternion.Euler(CARD_FACE_UP_ROTATION),
                    cardRotationSpeed
                )
            );

            // Raise event for LoteriaTable to check this card
            revealSequence.AppendCallback(() =>
            {
                EventBus.Raise(new RevealSingleCardEvent(cardData));
            });
        }

        // After all cards are revealed, notify Cantador
        revealSequence.OnComplete(() =>
        {
            EventBus.Raise(new RevealDrawnCardsCompleteEvent());
        });
    }

    #endregion

    #region Discard Animation

    private void DiscardCardsSequence(float totalDiscardTime)
    {
        Sequence discardSequence = DOTween.Sequence();

        // Calculate time per card for the discard animation
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

            // discardSequence.Append(
            //     cardRectTransform.DOPunchRotation(DISCARD_PUNCH_ROTATION, DISCARD_PUNCH_DURATION)
            // );
            discardSequence.Append(
                cardRectTransform.DOLocalMoveX(DISCARD_MOVE_X_OFFSET, discardTimePerCard, true)
            );
        }

        discardSequence.OnComplete(() =>
        {
            DeactivateAllCardsInPool();

            // Hide slider when discard is complete
            if (revealSlider != null)
            {
                revealSlider.gameObject.SetActive(false);
            }

            // Notify Cantador that discard is complete
            EventBus.Raise(new DiscardCardsCompleteEvent());
        });
    }

    /// <summary>
    /// Animates the slider counting down from 1 to 0 over the specified duration
    /// </summary>
    private IEnumerator AnimateSliderCountdown(float duration)
    {
        if (revealSlider == null) yield break;

        float elapsedTime = 0f;
        revealSlider.value = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / duration;

            // Countdown from 1 to 0
            revealSlider.value = 1f - normalizedTime;

            yield return null;
        }

        // Ensure it ends at 0
        revealSlider.value = 0f;
    }

    #endregion

    #region Utility Methods

    private void DeactivateUnusedCards(int activeCardCount)
    {
        for (int i = activeCardCount; i < displayedCardPool.Count; i++)
        {
            displayedCardPool[i].SetActive(false);
        }
    }

    private void ActivateDisplayCardsInPool(int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            GameObject displayedCard = displayedCardPool[i];

            // Set initial appearance (face down)
            RectTransform cardRectTransform = displayedCard.GetComponent<RectTransform>();
            cardRectTransform.localRotation = Quaternion.Euler(CARD_FACE_DOWN_ROTATION);
            cardRectTransform.localScale = INITIAL_CARD_SCALE;

            displayedCard.SetActive(true);
        }
    }

    #endregion
}