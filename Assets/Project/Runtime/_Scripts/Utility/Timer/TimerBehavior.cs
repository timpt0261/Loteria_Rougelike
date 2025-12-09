using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimerBehavior : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private UnityEvent onTimerStart = null;
    [SerializeField] private UnityEvent onTimerEnd = null;

    private Timer timer;
    void Start()
    {
        timer = new Timer(duration);
        timer.OnTimerStart += HandleTimerStart;
        timer.OnTimerEnd += HandleTimerEnd;

    }

    

    private void HandleTimerStart()
    {
        onTimerStart?.Invoke();
    }

    private void HandleTimerEnd()
    {
        onTimerEnd?.Invoke();
    }
}


