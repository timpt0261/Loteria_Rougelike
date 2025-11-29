using System.Collections.Generic;
using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using System.Collections;


public class DrawnCardDisplay : MonoBehaviour
{
    [Header("UI")]

    [SerializeField] private RectTransform cardDeckTransform;
    [SerializeField] private RectTransform drawnCardsRectTransform;


    [SerializeField] private GridLayoutGroup drawnCardsGridGroup;

    [SerializeField] private TextMeshProUGUI remainingText;


    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private List<GameObject> displayedCardPool;


    [SerializeField] private float revealSpeed = 1f;
    [SerializeField] private float revealTimeBetweenInterval = 1f;

    [SerializeField] private float discardSpeed = 1f;
    [SerializeField] private float discardTimeBetweenInterval = 1f;

    // grid layout

    private const float CELL_WIDTH = 60f;
    private const float CELL_HEIGHT = 86.666f;
    private static Vector2 CELL_SIZE = new Vector2(CELL_WIDTH, CELL_HEIGHT);

    private static Vector3 INIT_CARD_SCALE = Vector3.one;

    private Vector3 startRotation = new Vector3(0f, -180f, 0f);
    private Vector3 endRotation = Vector3.zero;

    // events
    public static event Action<LoteriaCardsData> OnCardRevealed;




    private void Awake()
    {
        if (drawnCardsRectTransform == null) { drawnCardsRectTransform = GetComponent<RectTransform>(); }
        if (drawnCardsGridGroup == null) { drawnCardsGridGroup = GetComponent<GridLayoutGroup>(); }
        if (remainingText == null) { GameObject.Find("RemainingCardText").GetComponent<TextMeshProUGUI>(); }
        drawnCardsGridGroup.cellSize = CELL_SIZE;


        foreach (GameObject child in displayedCardPool)
        {
            child.gameObject.SetActive(false);
        }

    }


    private void OnEnable()
    {
        Cantador.OnUpdateRemainingCards += UpdateRemainingCardsText;
        Cantador.OnDrawingCards += UpdateCardDisplay;
    }


    void OnDisable()
    {
        Cantador.OnUpdateRemainingCards -= UpdateRemainingCardsText;
        Cantador.OnDrawingCards -= UpdateCardDisplay;

    }

    private void UpdateRemainingCardsText(int remaining, int total)
    {
        remainingText.text = $"{remaining} / {total}";
    }

    private void UpdateCardDisplay(List<LoteriaCardsData> drawnCards)
    {
        Sequence masterSequence = DOTween.Sequence();

        masterSequence.AppendCallback(() => AddCardsToDisplay(drawnCards));

        masterSequence.AppendInterval(1.5f);

        masterSequence.AppendCallback(() => RevealCards());

        masterSequence.AppendInterval(revealTimeBetweenInterval * displayedCardPool.Count + revealSpeed + 6f);

        masterSequence.AppendCallback(() => DiscardCards());

    }


    private void AddCardsToDisplay(List<LoteriaCardsData> drawnCards)
    {
        if (drawnCards == null) { return; }
        if (drawnCards.Count > displayedCardPool.Count || drawnCards.Count == 0)
        {
            Debug.Log("Index error: drawn cards count is invalid");
            return;
        }

        // Set grid layout to initial compressed state
        drawnCardsGridGroup.padding.right = -700;
        drawnCardsGridGroup.spacing = new Vector2(-60, 0);

        // Set active displayed cards and assign card data
        int displayCount = drawnCards.Count;
        ActivateDisplayCardsInPool(drawnCards, displayCount);

        // Deactivate unused cards in pool
        DeactivateUnusedCards(displayCount);

        // Animate grid layout expansion using DOTween
        float duration = 1f;

        Sequence layoutSequence = DOTween.Sequence();

        // Animate padding right from -700 to 0
        layoutSequence.Append(
            DOTween.To(
                () => drawnCardsGridGroup.padding.right,
                x =>
                {
                    var padding = drawnCardsGridGroup.padding;
                    padding.right = (int)x;
                    drawnCardsGridGroup.padding = padding;
                },
                0,
                duration
            ).SetEase(Ease.OutQuad)
        );

        // Animate spacing X from -60 to 16 (simultaneously with padding)
        layoutSequence.Join(
            DOTween.To(
                () => drawnCardsGridGroup.spacing.x,
                x =>
                {
                    drawnCardsGridGroup.spacing = new Vector2(x, drawnCardsGridGroup.spacing.y);
                },
                16f,
                duration
            ).SetEase(Ease.OutQuad)
        );
    }

    public void RevealCards()
    {
        Sequence revealSequence = DOTween.Sequence();

        foreach (GameObject card in displayedCardPool)
        {
            RectTransform rect = card.GetComponent<RectTransform>();
            LoteriaCardsData cardData = card.GetComponent<LoteriaCard>().CurrentLoteriaCardData;
            revealSequence.AppendInterval(revealTimeBetweenInterval).
            Append(rect.DOLocalRotateQuaternion(Quaternion.Euler(endRotation), revealSpeed));
            OnCardRevealed?.Invoke(cardData);
        }



    }

    public void DiscardCards()
    {
        Sequence discardSequence = DOTween.Sequence();

        foreach (GameObject card in displayedCardPool)
        {
            RectTransform rect = card.GetComponent<RectTransform>();
            discardSequence.
            Append(rect.DOPunchRotation(new Vector3(2, 0, 2), 1f)).
            Append(rect.DOLocalMoveX(-900f, discardSpeed, true));
        }

        discardSequence.OnComplete(() =>
        {
            foreach (GameObject card in displayedCardPool)
                card.SetActive(false);
        });



    }

    #region  Utility 
    private void DeactivateUnusedCards(int displayCount)
    {
        for (int i = displayCount; i < displayedCardPool.Count; i++)
        {
            displayedCardPool[i].SetActive(false);
        }
    }

    private void ActivateDisplayCardsInPool(List<LoteriaCardsData> drawnCards, int displayCount)
    {
        for (int i = 0; i < displayCount; i++)
        {
            GameObject displayedCard = displayedCardPool[i];

            // Set card data
            LoteriaCard loteriaCard = displayedCard.GetComponent<LoteriaCard>();
            loteriaCard.SetCardData(drawnCards[i]);

            // Set initial rotation (face down)
            RectTransform rect = displayedCard.GetComponent<RectTransform>();
            rect.localRotation = Quaternion.Euler(startRotation);
            rect.localScale = INIT_CARD_SCALE;

            displayedCard.SetActive(true);
        }
    }

    #endregion

}
