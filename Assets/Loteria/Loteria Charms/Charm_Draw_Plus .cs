using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Charm_Draw_Plus: Charm
{
    private int additionalDraws = 2;

    protected override void ActivateCharm()
    {
        Cantador.Instance.DrawAmount += additionalDraws;
    }

    protected override void DestroyCharm()
    {
        Cantador.Instance.DrawAmount += additionalDraws;
        Destroy(this);
    }


    public override void OnRoundStart()
    {
        ActivateCharm();
    }

    public override void OnDestroy()
    {
        DestroyCharm();
    }
}
