using System.Collections.Generic;
using UnityEngine;

public class Contestant : MonoBehaviour
{

    // stats 
    private int level;
    private int money;
    private int wins;
    private int losses;
    private int luck;
    private int longestStreak;

    [field: Header("GameObject References")]
    [field: SerializeField] private int contestantID;
    [field: SerializeField] private List<LoteriaCardsData> contestantDeck = new();
    [field: SerializeField] private LoteriaTabla contestantTabla;
    [field: SerializeField] private List<Charm> contestantOwnedCharms;
    public int ContestantID { get { return contestantID; } }
    public List<LoteriaCardsData> LoteriaDeck { get { return contestantDeck; } }
    public LoteriaTabla LoteriaTabla { get { return contestantTabla; } }
    public List<Charm> Charms { get { return contestantOwnedCharms; } }

}
