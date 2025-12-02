using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using System.Collections;

public class Cantador_UI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform cardDeckTransform;
    [SerializeField] private RectTransform drawnCardsRectTransform;
    [SerializeField] private GridLayoutGroup drawnCardsGridGroup;
    [SerializeField] private TextMeshProUGUI remainingCardsText;

    [Header("Card Prefab")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<GameObject> displayedCardPool;

    [Header("Animation Timing")]
    [SerializeField] private float cardRevealDuration = 1f;
    [SerializeField] private float delayBetweenCardReveals = 1f;
    [SerializeField] private float cardDiscardDuration = 1f;
    [SerializeField] private float delayBetweenCardDiscards = 1f;

    // Grid Layout Constants
    private const float CELL_WIDTH = 60f;
    private const float CELL_HEIGHT = 86.666f;
    private static readonly Vector2 CELL_SIZE = new Vector2(CELL_WIDTH, CELL_HEIGHT);

    // Animation Constants
    private const float GRID_EXPANSION_DURATION = 1f;
    private const int GRID_COMPRESSED_PADDING_RIGHT = -425;
    private const int GRID_EXPANDED_PADDING_RIGHT = 0;
    private const float GRID_COMPRESSED_SPACING_X = -60f;
    private const float GRID_EXPANDED_SPACING_X = 16f;

    private const float DELAY_BEFORE_REVEAL = 1.5f;
    private const float DELAY_AFTER_REVEAL_BUFFER = 6f;

    private const float DISCARD_MOVE_X_OFFSET = -900f;
    private const float DISCARD_PUNCH_DURATION = 1f;

    private static readonly Vector3 INITIAL_CARD_SCALE = Vector3.one;
    private static readonly Vector3 CARD_FACE_DOWN_ROTATION = new Vector3(0f, -180f, 0f);
    private static readonly Vector3 CARD_FACE_UP_ROTATION = Vector3.zero;
    private static readonly Vector3 DISCARD_PUNCH_ROTATION = new Vector3(2f, 0f, 2f);

    // Events
    public static event Action<LoteriaCardsData> OnCardRevealed;

    private void Awake()
    {
        InitializeComponents();
        ConfigureGridLayout();
        DeactivateAllCardsInPool();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<RunStartEvent>(OnRunStart);
        EventBus.Subscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Subscribe<DrawCardEvent>(OnAddCardsToDisplay);


    }


    private void OnDisable()
    {
        EventBus.Subscribe<RunStartEvent>(OnRunStart);
        EventBus.Unsubscribe<RoundStartEvent>(OnRoundStart);
        EventBus.Unsubscribe<DrawCardEvent>(OnAddCardsToDisplay);

    }

    private void InitializeComponents()
    {
        if (drawnCardsRectTransform == null)
        {
            drawnCardsRectTransform = GetComponent<RectTransform>();
        }

        if (drawnCardsGridGroup == null)
        {
            drawnCardsGridGroup = GetComponent<GridLayoutGroup>();
        }

        if (remainingCardsText == null)
        {
            remainingCardsText = GameObject.Find("RemainingCardText").GetComponent<TextMeshProUGUI>();
        }
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

    // private void UpdateCardDisplay(List<LoteriaCardsData> drawnCards)
    // {
    //     Sequence masterSequence = DOTween.Sequence();

    //     masterSequence.AppendCallback(() => AddCardsToDisplay(drawnCards));
    //     masterSequence.AppendInterval(DELAY_BEFORE_REVEAL);
    //     masterSequence.AppendCallback(() => RevealCards());

    //     float totalRevealTime = CalculateTotalRevealTime();
    //     masterSequence.AppendInterval(totalRevealTime);

    //     masterSequence.AppendCallback(() => DiscardCards());
    // }

    private float CalculateTotalRevealTime()
    {
        return (delayBetweenCardReveals * displayedCardPool.Count) +
               cardRevealDuration +
               DELAY_AFTER_REVEAL_BUFFER;
    }

    private void OnRunStart(RunStartEvent runStartEvent)
    {
        int total = runStartEvent.CurrentDeck.Count;
        UpdateRemainingCardsText(total, total);
    }

    private void OnRoundStart(RoundStartEvent roundStartEvent)
    {
        // reset cards to remaining
        // update upper ui button to say current round

    }


    private void OnAddCardsToDisplay(DrawCardEvent drawCardEvent)
    {
        bool ValidateDrawnCards(List<LoteriaCardsData> drawnCards)
        {
            if (drawnCards == null)
            {
                return false;
            }

            if (drawnCards.Count > displayedCardPool.Count || drawnCards.Count == 0)
            {
                Debug.LogError($"Invalid drawn cards count: {drawnCards.Count}. Must be between 1 and {displayedCardPool.Count}");
                return false;
            }

            return true;
        }

        void SetGridToCompressedState()
        {
            var padding = drawnCardsGridGroup.padding;
            padding.right = GRID_COMPRESSED_PADDING_RIGHT;
            drawnCardsGridGroup.padding = padding;

            drawnCardsGridGroup.spacing = new Vector2(GRID_COMPRESSED_SPACING_X, 0);
        }


        void AnimateGridExpansion(float animationDuration)
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

        float displayAnimationDuration = drawCardEvent.DrawTime;
        List<LoteriaCardsData> drawnCards = drawCardEvent.DrawnCards;
        if (!ValidateDrawnCards(drawnCards))
        {
            return;
        }

        SetGridToCompressedState();

        int cardCount = drawnCards.Count;
        ActivateDisplayCardsInPool(drawnCards, cardCount);
        DeactivateUnusedCards(cardCount);

        AnimateGridExpansion(displayAnimationDuration);
    }

    public void RevealCards()
    {
        Sequence revealSequence = DOTween.Sequence();

        foreach (GameObject card in displayedCardPool)
        {
            if (!card.activeSelf) continue;

            RectTransform cardRectTransform = card.GetComponent<RectTransform>();
            LoteriaCardsData cardData = card.GetComponent<LoteriaCard>().CurrentLoteriaCardData;

            revealSequence.AppendInterval(delayBetweenCardReveals);
            revealSequence.Append(
                cardRectTransform.DOLocalRotateQuaternion(
                    Quaternion.Euler(CARD_FACE_UP_ROTATION),
                    cardRevealDuration
                )
            );
            revealSequence.AppendCallback(() => EventBus.Raise(new RevealCardEvent(cardData)));
        }
    }

    public void DiscardCards()
    {
        Sequence discardSequence = DOTween.Sequence();

        foreach (GameObject card in displayedCardPool)
        {
            if (!card.activeSelf) continue;

            RectTransform cardRectTransform = card.GetComponent<RectTransform>();

            discardSequence.Append(
                cardRectTransform.DOPunchRotation(DISCARD_PUNCH_ROTATION, DISCARD_PUNCH_DURATION)
            );
            discardSequence.Append(
                cardRectTransform.DOLocalMoveX(DISCARD_MOVE_X_OFFSET, cardDiscardDuration, true)
            );
        }

        discardSequence.OnComplete(() =>
        {
            DeactivateAllCardsInPool();
        });
    }

    #region Utility Methods

    private void DeactivateUnusedCards(int activeCardCount)
    {
        for (int i = activeCardCount; i < displayedCardPool.Count; i++)
        {
            displayedCardPool[i].SetActive(false);
        }
    }

    private void ActivateDisplayCardsInPool(List<LoteriaCardsData> drawnCards, int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            GameObject displayedCard = displayedCardPool[i];

            // Set card data
            LoteriaCard loteriaCard = displayedCard.GetComponent<LoteriaCard>();
            loteriaCard.SetCardData(drawnCards[i]);

            // Set initial appearance (face down)
            RectTransform cardRectTransform = displayedCard.GetComponent<RectTransform>();
            cardRectTransform.localRotation = Quaternion.Euler(CARD_FACE_DOWN_ROTATION);
            cardRectTransform.localScale = INITIAL_CARD_SCALE;

            displayedCard.SetActive(true);
        }
    }

    #endregion
}