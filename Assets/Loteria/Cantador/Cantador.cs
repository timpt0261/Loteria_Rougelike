using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Cantador : MonoBehaviour
{

    public static Cantador Instance { get; private set; }

    [Header("Card Data")]
    [SerializeField] private List<Sprite> cardSprites = new();
    [SerializeField] private List<Sprite> loteriaCardsToDraw = new();
    [SerializeField] private List<Sprite> loteriaCardsNotDrawn = new();

    [Header("Timer Settings")]
    [SerializeField] private Slider timeSlot;
    [SerializeField] private float drawTime = 3f;   // duration between draws
    [SerializeField] private float refillSpeed = 2f;

    // states
    private float timer;
    private bool isDrawingCard;
    private bool isReady = true;

    [SerializeField] private Image drawCardImage;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        Shuffle();
        ResetTimer();
    }

    void Update()
    {
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
        // if (!CanDraw()) return;

        // // Start cooldown
        // StartTimer();

        // Draw a random card
        int index = Random.Range(0, loteriaCardsToDraw.Count);
        Sprite drawnCard = loteriaCardsToDraw[index];

        if (loteriaCardsToDraw.Count < 1)
            Shuffle();

        loteriaCardsToDraw.RemoveAt(index);
        loteriaCardsNotDrawn.Add(drawnCard);
        drawCardImage.sprite = drawnCard;
        Debug.Log($"Drew card: {drawnCard.name}");
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
        if (loteriaCardsToDraw.Count < 1) Shuffle();
        return true;
    }

    private void Shuffle()
    {
        var shuffled = new List<Sprite>(cardSprites);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            (shuffled[i], shuffled[r]) = (shuffled[r], shuffled[i]);
        }
        loteriaCardsToDraw = shuffled;
        loteriaCardsNotDrawn.Clear();
    }
}
