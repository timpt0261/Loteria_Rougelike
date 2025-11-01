using System.Collections.Generic;
using UnityEngine;

public class LoteriaCardFactory : MonoBehaviour
{
    public static LoteriaCardFactory instance { get; private set; }

    [SerializeField] private GameObject cardPrefab;

    public List<LoteriaCardsData> cardData;

    void Start()
    {
        if (LoteriaCardFactory.instance == null)
        {
            LoteriaCardFactory.instance = this;
        }
    }

    public void GenerateLoteriaCards()
    {


    }
}
