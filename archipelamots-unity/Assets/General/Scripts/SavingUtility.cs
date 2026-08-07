using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;

public static class SavingUtility
{
    public const int VERSION = 3;
    private const string SAVEFILE = "/save.dat";
    private const string OLD_SAVEFILE = "/save_backup_{0}.dat";

    private static SaveData CurrentSaveData;

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        CurrentSaveData = null;
    }

    private static string SaveDataFilePath()
    {
        return Application.persistentDataPath + SAVEFILE;
    }

    public static void LoadSaveData()
    {
        string destination = SaveDataFilePath();
        FileStream file;

        bool error = false;

        if (File.Exists(destination))
        {
            file = File.OpenRead(destination);
        }
        else
        {
            Debug.LogWarning("Save file not found. Creating a new one.");
            CurrentSaveData = new SaveData();
            OverwriteSaveData();
            return;
        }

        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            CurrentSaveData = (SaveData) bf.Deserialize(file);
            if (CurrentSaveData.firstSeed != YAMLLoader.Instance.Grids[0].seedString)
            {
                Debug.LogWarning($"Save file was related to another Archipelago generation. Backing up, then recreating a save file.");
                BackupSaveFile(destination);
                CurrentSaveData = new SaveData();
            }
            else if (VERSION > CurrentSaveData.version)
            {
                Debug.LogWarning($"Save file was of another version, resetting it.");
                BackupSaveFile(destination);
                CurrentSaveData = new SaveData();
            }
        }
        catch (Exception e)
        {
            error = true;
            Debug.LogError($"Error when loading save file: {e.Message}");
            CurrentSaveData = new SaveData();
        }

        if (error)
        {
            try
            {
                BackupSaveFile(destination);
                OverwriteSaveData();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        file.Close();

        CurrentSaveData.version = VERSION;
        OverwriteSaveData();
    }

    private static void BackupSaveFile(string destination)
    {
        File.Copy(destination, Application.persistentDataPath + string.Format(OLD_SAVEFILE, Application.version.Replace(".", "-").Replace(" ", "-") + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-s")));
    }

    public static void OverwriteSaveData()
    {
        try
        {
            string destination = SaveDataFilePath();
            FileStream file;

            if (File.Exists(destination)) file = File.OpenWrite(destination);
            else file = File.Create(destination);

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, CurrentSaveData);
            file.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static void SaveGridCharacter(LetterGridSquare gridSquare, char character)
    {
        if (CurrentSaveData.gridsData == null)
        {
            CurrentSaveData.firstSeed = YAMLLoader.Instance.Grids[0].seedString;
            CurrentSaveData.gridsData = new SaveData.GridData[YAMLLoader.Instance.Grids.Length];
            for (int i = 0; i < CurrentSaveData.gridsData.Length; i++)
            {
                SaveData.GridData gridData = new SaveData.GridData();
                gridData.characters = new SaveData.CharacterData[YAMLLoader.Instance.Grids[i].grid.Length, YAMLLoader.Instance.Grids[i].grid[0].Length];
                CurrentSaveData.gridsData[i] = gridData;
            }
        }

        SaveData.CharacterData characterData = CurrentSaveData.gridsData[gridSquare.Grid.GridNb].characters[gridSquare.R, gridSquare.C];
        characterData.character = character;
        characterData.lockedIn = gridSquare.LockedIn;
        CurrentSaveData.gridsData[gridSquare.Grid.GridNb].characters[gridSquare.R, gridSquare.C] = characterData;

        OverwriteSaveData();
    }

    public static void LoadGridData(CrosswordGrid grid)
    {
        if (CurrentSaveData == null)
        {
            LoadSaveData();
        }

        if (CurrentSaveData.gridsData == null)
            return;

        for (int r = 0; r < grid.GridSquares.GetLength(0); r++)
        {
            for (int c = 0; c < grid.GridSquares.GetLength(1); c++)
            {
                if (grid.GridSquares[r, c] is LetterGridSquare)
                {
                    LetterGridSquare gridSquare = grid.GridSquares[r, c] as LetterGridSquare;
                    SaveData.CharacterData characterData = CurrentSaveData.gridsData[grid.GridNb].characters[r, c];
                    if (characterData.character != '\0')
                    {
                        gridSquare.Set(characterData.character, false);
                        if (characterData.lockedIn)
                            gridSquare.LockIn(false);
                    }
                    else
                    {
                        gridSquare.Erase(false);
                    }
                }
            }
        }
    }
}
