using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TimerBehavior : MonoBehaviour
{
    [field: SerializeField] private float duration;
    [field: SerializeField] private UnityEvent onTimerStart = null;
    [field: SerializeField] private UnityEvent onTimerEnd = null;

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


