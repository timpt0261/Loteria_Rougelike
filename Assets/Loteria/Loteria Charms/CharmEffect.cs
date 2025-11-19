using UnityEngine;

// [CreateAssetMenu(fileName = "CharmEffect", menuName = "Scriptable Objects/CharmEffect")]
public abstract class CharmEffect : ScriptableObject
{
    public abstract void Perform();
}
