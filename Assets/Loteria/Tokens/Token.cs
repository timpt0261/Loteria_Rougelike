using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class Token : MonoBehaviour
{

    [SerializeField] private Image tokenImage;
    [SerializeField] private AudioBehaviour tokenSFX;
    [SerializeField] private float price;

    [SerializeField] private EffectData effectData;


    public void PerformEffect()
    {
        foreach (EffectSO effect in effectData.effects)
        {
            effect.Perform();
        }
    }


}
