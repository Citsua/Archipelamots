using SimpleFileBrowser;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

public class YAMLLoader : MonoBehaviour
{
    public static YAMLLoader Instance { get; private set; }

    public YAML.ArchipelagoYAML YAML {  get; private set; }
    public YAML.Grid[] Grids { get; private set; }

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.Exception($"{this.GetType()} Singleton already exists in the scene");
        Instance = this;
    }

    public void ShowFileBrowser()
    {
        try
        {
            FileBrowser.ShowLoadDialog(this.OnFileLoadingSuccess, this.OnFileLoadingCancel, FileBrowser.PickMode.Files, false, null, null, "Charger le fichier YAML généré", "Sélectionner");
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OnFileLoadingSuccess(string[] paths)
    {
        if (paths.Length != 1)
            throw new System.Exception("There should be exactly one file selected");

        if (!paths[0].EndsWith("-generated.yaml"))
            throw new System.Exception("Incorrect file selected: it should be a GENERATED yaml file");

        IDeserializer deserializer = new DeserializerBuilder().Build();
        using (StreamReader streamReader = File.OpenText(paths[0]))
        {
            this.YAML = deserializer.Deserialize<YAML.ArchipelagoYAML>(streamReader);
            string obj = $"grid_data:\n{this.YAML.Archipelamots.grid_data}";
            YAML.ArchipelagoGridDataYAML gridData = deserializer.Deserialize<YAML.ArchipelagoGridDataYAML>(obj);
            this.Grids = gridData.grid_data;

            UI.Instance.Connection.Hide();
            SavingUtility.LoadSaveData();
            CrosswordGrid.Current.Initialize(0);
        }
    }

    private void OnFileLoadingCancel()
    {
        throw new System.Exception("Need to select generated YAML file before playing");
    }
}
