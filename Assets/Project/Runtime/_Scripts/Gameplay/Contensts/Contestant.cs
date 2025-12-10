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

    [Header("GameObject References")]
    [SerializeField] private int contestantID;
    [SerializeField] private List<LoteriaCardsData> contestantDeck = new();
    [SerializeField] private LoteriaTabla contestantTabla;
    [SerializeField] private List<Charm> contestantOwnedCharms;
    public int ContestantID { get { return contestantID; } }
    public List<LoteriaCardsData> LoteriaDeck { get { return contestantDeck; } }
    public LoteriaTabla LoteriaTabla { get { return contestantTabla; } }
    public List<Charm> Charms { get { return contestantOwnedCharms; } }

}
