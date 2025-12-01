using UnityEngine;

public class Charm_Swap_Space : Charm
{


    protected override void ActivateCharm()
    {
        // for each drawn card have a 1 in 4 chance to swap a random unmarked slot with one of the drawn cards

        var drawn = Cantador.Instance.DrawnLoteriaCardsThisTurn;
        var unmarked = LoteriaTable.Instance.UnmarkedSlots; // issue contains the id not index
        foreach (var card in drawn)
        {
            if (!Chance()) continue;
            int index = Random.Range(0, unmarked.Count);
            int id = unmarked[index];
            LoteriaTable.Instance.SetSlotByCardId(id, card);

        }

        uses--;
    }

    public override void OnReveal()
    {
        if (uses < 0)
            ActivateCharm();
    }
}
