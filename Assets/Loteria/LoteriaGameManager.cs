using UnityEngine;

public class LoteriaGameManager : MonoBehaviour
{
    [SerializeField] private Cantador cantador;
    [SerializeField] private LoteriaTable loteriaTable;

    void Awake()
    {

    }
    void Start()
    {
        cantador = Cantador.Instance;
        loteriaTable = LoteriaTable.Instance;
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
