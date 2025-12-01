using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEditor.Overlays;
using UnityEngine;

public class Charm_Set_Space : Charm
{
    [SerializeField] private int chossenLoteriaSlotIndex;
    [SerializeField] private LoteriaCardsData previousLoteriaSlot;
    [SerializeField] private LoteriaCardsData newLoteriaCard;


    protected override void ActivateCharm()
    {
        // pick a random space between 1 to 16
        chossenLoteriaSlotIndex = Random.Range(0, 15);

        // set designated loteria table
        LoteriaTable.Instance.GetSlotCard(chossenLoteriaSlotIndex);
        LoteriaTable.Instance.SetSlotCard(chossenLoteriaSlotIndex, newLoteriaCard);
        uses--; ;

    }

    protected override void DestroyCharm()
    {
        LoteriaTable.Instance.SetSlotCard(chossenLoteriaSlotIndex, previousLoteriaSlot);
        Destroy(this);
    }
    public override void OnRoundStart()
    {
        if (uses > 0)
            ActivateCharm();
    }

    public override void OnDestroy()
    {

    }
}
