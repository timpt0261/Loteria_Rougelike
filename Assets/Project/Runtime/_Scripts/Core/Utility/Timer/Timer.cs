using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private TimerType type = TimerType.DELTA;
    private bool timerIsRunning = false;
    private float duration; // Store initial duration

    public float RemainingSeconds { get; private set; }

    // Events
    public event Action OnTimerStart;
    public event Action OnTimerEnd;

    public Timer(float duration)
    {
        this.duration = duration;
        RemainingSeconds = duration;
    }

    public Timer(float duration, TimerType timerType)
    {
        this.duration = duration;
        RemainingSeconds = duration;
        type = timerType;
    }

    public async Task StartTimer()
    {
        RemainingSeconds = duration; // Reset to initial duration

        switch (type)
        {
            case TimerType.DELTA:
                timerIsRunning = true;
                OnTimerStart?.Invoke();
                break;
            case TimerType.COROUTINE:
                StopAllCoroutines();
                StartCoroutine(TickCoroutine(duration));
                break;
            case TimerType.ASYNC:
                TickAsync(duration);
                break;
        }
    }

    public virtual void Tick(float deltaTime)
    {
        if (this.type != TimerType.DELTA) { return; }
        if (!timerIsRunning) { return; }
        if (RemainingSeconds == 0f) { return; }

        RemainingSeconds -= deltaTime;
        CheckForTimerEnd();
    }

    public virtual IEnumerator TickCoroutine(float duration)
    {
        OnTimerStart?.Invoke();
        timerIsRunning = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            RemainingSeconds = Mathf.Max(0f, duration - elapsed);
            yield return null;
        }

        RemainingSeconds = 0f;
        timerIsRunning = false;
        OnTimerEnd?.Invoke();
    }

    public virtual async void TickAsync(float durationInSeconds)
    {
        OnTimerStart?.Invoke();
        timerIsRunning = true;

        float elapsed = 0f;
        while (elapsed < durationInSeconds)
        {
            float deltaTime = Time.unscaledDeltaTime;
            elapsed += deltaTime;
            RemainingSeconds = Mathf.Max(0f, durationInSeconds - elapsed);
            await Task.Yield();
        }

        RemainingSeconds = 0f;
        timerIsRunning = false;
        OnTimerEnd?.Invoke();
    }

    private void CheckForTimerEnd()
    {
        if (RemainingSeconds > 0f) { return; }

        RemainingSeconds = 0f;
        timerIsRunning = false;
        OnTimerEnd?.Invoke();
    }

    private int ConvertMilliSecondsToSeconds(float duration)
    {
        const int THOUSAND = 1000;
        return (int)(duration * THOUSAND);
    }
}