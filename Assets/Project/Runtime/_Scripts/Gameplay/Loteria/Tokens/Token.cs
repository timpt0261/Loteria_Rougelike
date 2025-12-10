using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class Token : TimerBehavior
{

    [SerializeField] private Image tokenImage;
    [SerializeField] private AudioBehaviour tokenSFX;
    [SerializeField] private float price;

    [SerializeField] private EffectData effectData;

    [Header("Timer")]
    [SerializeField] private float timerDuration;
    [SerializeField] TimerBehavior tokenTimer;

}
