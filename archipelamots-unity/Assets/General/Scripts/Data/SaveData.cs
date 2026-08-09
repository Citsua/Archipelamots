using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int version;

    [System.Serializable]
    public class GridData
    {
        public CharacterData[,] characters;
    }

    [System.Serializable]
    public struct CharacterData
    {
        public char character;
        public bool lockedIn;
    }

    public string firstSeed;
    public GridData[] gridsData;
    public int letterReveals;
    public int wordChecks;

    public Dictionary<string, int> ItemsReceived { get; set; }

    public SaveData()
    {
        this.version = SavingUtility.VERSION;
        this.firstSeed = YAMLLoader.Instance.Grids[0].seedString;
        this.ItemsReceived = new Dictionary<string, int>();
    }
}
