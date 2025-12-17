using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class Token : TimerBehavior
{

    [field: SerializeField] private Image tokenImage;
    [field: SerializeField] private AudioBehaviour tokenSFX;
    [field: SerializeField] private float price;

    [field: SerializeField] private EffectData effectData;

    [field: Header("Timer")]
    [field: SerializeField] private float timerDuration;
    [field: SerializeField] TimerBehavior tokenTimer;

}
