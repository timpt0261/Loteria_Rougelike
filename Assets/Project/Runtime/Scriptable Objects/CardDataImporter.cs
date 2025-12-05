using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

public class CardDataImporter : EditorWindow
{
    private TextAsset jsonFile;
    private string spriteFolderPath = "Assets/Loteria/Sprites";

    [MenuItem("Tools/Import Card Data")]
    public static void ShowWindow()
    {
        GetWindow<CardDataImporter>("Card Data Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Import Card Data from JSON", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);
        spriteFolderPath = EditorGUILayout.TextField("Sprite Folder Path", spriteFolderPath);

        if (GUILayout.Button("Import"))
        {
            if (jsonFile == null)
            {
                Debug.LogError("Please assign a JSON file.");
                return;
            }

            ImportData(jsonFile, spriteFolderPath);
        }
    }

    public static void ImportData(TextAsset json, string spritePath)
    {
        var cardDB = JsonUtility.FromJson<CardDatabase>(json.text);
        string folderPath = "Assets/Loteria/Card/CardData";

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var entry in cardDB.cards)
        {
            string assetPath = $"{folderPath}/{entry.sprite}.asset";
            LoteriaCardsData card = AssetDatabase.LoadAssetAtPath<LoteriaCardsData>(assetPath);

            if (card == null)
            {
                card = ScriptableObject.CreateInstance<LoteriaCardsData>();
                AssetDatabase.CreateAsset(card, assetPath);
            }

            // Load sprite using AssetDatabase
            string spriteFullPath = $"{spritePath}/{entry.sprite}.jpg";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFullPath);

            if (sprite == null)
                Debug.LogWarning($"Sprite not found at: {spriteFullPath}");

            card.id = entry.id;
            card.sprite = sprite;
            card.chance = entry.chance;

            EditorUtility.SetDirty(card);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Card data successfully imported or updated!");
    }
}


[Serializable]
public class CardEntry
{
    public int id;
    public string sprite;

    public float chance;
}

[Serializable]
public class CardDatabase
{
    public List<CardEntry> cards;
}