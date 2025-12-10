using UnityEngine;
using UnityEngine.Events;



public abstract class Charm : MonoBehaviour
{
    protected enum RarityType { COMMON, UNCOMMON, RARE, LEGENDARY }

    [Header("Charm Stats")]
    [SerializeField] public Sprite charmSprite;
    [SerializeField] protected int charmID;
    [SerializeField] protected RarityType rarity;
    [SerializeField] protected int uses = 1;  // how many times it can be used 
    [SerializeField] protected float chanceOfActivation = 1f;
    [SerializeField] protected int buyPrice = 2;
    [SerializeField] protected int sellPrice = 1;

    protected const int minimumChanceRange = 0;
    protected const int maximumChanceRange = 1;

    // [Header("Effects(Buffs/De-Buffs)")]
    // [SerializeField] protected EffectData charmData;


    [SerializeField] protected UnityEvent OnActivateEffect;
    [SerializeField] protected UnityEvent OnDestroyEffect;


    // protected virtual void PerformEfect()
    // {
    //     Debug.Log($"{gameObject}'s effect");
    //     foreach (var effect in charmData.effects)
    //     {
    //         effect.Perform();
    //     }
    // }

    #region  Chance/Probability
    protected virtual bool ActivateCondition() { return true; }
    protected virtual bool DestroyCondition() { return true; }
    protected bool Chance()
    {
        if ((int)chanceOfActivation != maximumChanceRange) { return true; }
        float x = Random.Range(minInclusive: minimumChanceRange, maxExclusive: maximumChanceRange);
        return x <= chanceOfActivation;
    }

    protected virtual void ActivateCharm() { return; }

    protected virtual void DestroyCharm() { return; }
    #endregion

    #region Event Functions

    public virtual void OnRunStart() { return; }

    public virtual void OnRunEnd() { return; }
    public virtual void OnRoundStart() { return; }

    public virtual void OnRoundEnd() { return; }

    public virtual void OnDraw() { return; }

    public virtual void OnReveal() { return; }

    public virtual void OnDestroy() { return; }

    public virtual void OnSell() { return; }

    public virtual void OnBuy() { return; }

    #endregion

}
