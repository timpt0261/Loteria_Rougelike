using System.Collections.Generic;
using UnityEngine;

public class LoteriaGameManager : MonoBehaviour
{

    [SerializeField] private List<LoteriaCardsData> allLoteriaCards;
    [SerializeField] private Cantador cantador;
    [SerializeField] private LoteriaTable loteriaTable;

    void Awake()
    {

    }
    void Start()
    {
        cantador = Cantador.Instance;
        loteriaTable = LoteriaTable.Instance;
        SetLoteriaCardReference();
        cantador.Initialize();
    }

    private void SetLoteriaCardReference()
    {
        cantador.SetLoteriaDeck(this.allLoteriaCards);
        loteriaTable.SetLoteriaDeck(this.allLoteriaCards);
    }

    public void HandleCardDrawn()
    {
        loteriaTable.UpdateTabla(cantador.DrawnCard);

    }

    public void HandleTableCompleted()
    {
        cantador.Shuffle();
    }

    public void HandleDeckShuffled()
    {
        loteriaTable.ResetTable();
    }
}
