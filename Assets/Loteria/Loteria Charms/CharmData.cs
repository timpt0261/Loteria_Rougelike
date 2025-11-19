using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Loteria/Charm", menuName = "CharmData")]
public class CharmData : ScriptableObject
{

	public List<CharmEffect> effects;
}
