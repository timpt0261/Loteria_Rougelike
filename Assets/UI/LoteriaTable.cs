using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoteriaTable : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private List<Sprite> cardSprites;
    private const int total = 16;
    void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {

        // Remove all existing sprites 
        foreach (Transform t in gridContainer) { Destroy(t.gameObject); }
        // shuffled all sprites
        var shuffled = new List<Sprite>(cardSprites);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int r = Random.Range(i, shuffled.Count);
            var tmp = shuffled[i]; shuffled[i] = shuffled[r]; shuffled[r] = tmp;
        }
        //
        for (int i = 0; i < total; i++)
        {
            var currentSlot = Instantiate(cardPrefab, gridContainer.transform);
            var image = currentSlot.GetComponent<Image>();
            image.sprite = shuffled[i % shuffled.Count];

        }
    }


}
