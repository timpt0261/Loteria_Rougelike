using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
// TO DO: Invoke the charm manager Events Appropriately
public class CharmManager : MonoBehaviour
{
    // there should only be on instance of this
    public static CharmManager Instance { get; private set; }
    // contains all charms in field
    [field: SerializeField] private List<Charm> activeCharms;
    [field: SerializeField] private int charmLimit = 3;

    // activates charms in accordance to their effect

    [field: Header("Events")]
    [field: SerializeField] public UnityEvent OnRunStart;
    [field: SerializeField] public UnityEvent OnRunEnd;
    [field: SerializeField] public UnityEvent OnRoundStart;
    [field: SerializeField] public UnityEvent OnRoundEnd;
    [field: SerializeField] public UnityEvent OnDraw;
    [field: SerializeField] public UnityEvent OnReveal;
    [field: SerializeField] public UnityEvent OnDestroy;
    [field: SerializeField] public UnityEvent OnBuy;
    [field: SerializeField] public UnityEvent OnSell;

    [field: Header("UI")]
    [field: SerializeField] private List<Image> charmSlots;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        for (int i = 0; i < charmLimit; i++)
        {
            charmSlots[i].sprite = activeCharms[i].charmSprite;
        }
    }



    #region Update Charm List

    public void AddCharm(Charm newCharm)
    {
        if (activeCharms.Count == charmLimit) return;
        activeCharms.Add(newCharm);

    }

    public void SetCharms(int index, Charm selectedCharm)
    {
        if (index < 0 || index > activeCharms.Count)
        {
            Debug.Log("No active Charms");
            return;
        }

        activeCharms[index] = selectedCharm;
    }


    public void DestroyCharm(Charm selectedCharm)
    {
        if (activeCharms.Count < 1)
        {
            Debug.Log("No active Charms");
            return;
        }
        activeCharms.Remove(selectedCharm);

    }

    public void DestroyCharm(int index)
    {
        if (index < 0 || index > activeCharms.Count)
        {
            Debug.Log("No active Charms");
            return;
        }

        Charm selectedCharm = activeCharms[index];
        activeCharms.Remove(selectedCharm);

    }
    #endregion

    #region  Event Handling
    public void HandleRunStart()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnRunStart();
        }
    }
    public void HandleRunEnd()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnRunEnd();
        }
    }

    public void HandleRoundStart()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnRoundStart();
        }
    }

    public void HandleRoundEnd()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnRoundEnd();
        }

    }

    public void HandleOnDraw()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnDraw();
        }
    }

    public void HandleOnReval()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnReveal();
        }
    }

    public void HandleOnDestroy()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnDestroy();
        }
    }
    public void HandleOnBuy()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnBuy();
        }
    }

    public void HandleOnSell()
    {
        foreach (Charm charm in activeCharms)
        {
            charm.OnSell();
        }
    }

    #endregion


}
