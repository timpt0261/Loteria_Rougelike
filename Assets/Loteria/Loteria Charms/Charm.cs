using System.Collections.Generic;
using UnityEngine;

public class Charm : MonoBehaviour
{
    [SerializeField] private CharmData charmData;

    public void PerformEfect()
    {
        Debug.Log($"{gameObject}'s effect");
        foreach (var effect in charmData.effects)
        {
            effect.Perform();
        }
    }

}
