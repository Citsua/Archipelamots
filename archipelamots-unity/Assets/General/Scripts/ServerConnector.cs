using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using System;
using System.Linq;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    public static ServerConnector Instance { get; private set; }

    private ArchipelagoSession session;

    public bool Connected
    {
        get
        {
            return this.session != null;
        }
    }

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

    public void Connect()
    {
        this.session = Connect(UI.Instance.Connection.Address, UI.Instance.Connection.Port, UI.Instance.Connection.Slot, UI.Instance.Connection.Password);
        if (this.session != null)
        {
            this.session.Items.ItemReceived += this.OnItemReceived;
            YAMLLoader.Instance.ShowFileBrowser();
        }
    }

    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        ItemInfo info = helper.DequeueItem();
        Debug.Log($"Received Item '{info.ItemName}'");
        if (info.Flags.HasFlag(ItemFlags.Advancement))
        {
            if (info.ItemName.Contains("Definition n°"))
            {
                CrosswordGrid.Current.Reinitialize();
            }
            else if (info.ItemName.Contains("Grid n°"))
            {
                UI.Instance.GridSelector.Initialize();
            }
        }
        else
        {
            if (info.ItemName == "Word Check")
            {
                SavingUtility.IncreaseNumberOfWordChecks();
            }
            else if (info.ItemName == "Letter Reveal")
            {
                SavingUtility.IncreaseNumberOfLetterReveals();
            }
        }

        UI.Instance.UpdatePowerUI();
    }

    public void SendLocationCheck(string name)
    {
        long id = this.session.Locations.GetLocationIdFromName("Archipelamots", name);
        if (this.session.Locations.AllLocationsChecked.Contains(id))
            return;
        
        Debug.Log($"Completed Check '{name}' (id '{id}')");
        this.session.Locations.CompleteLocationChecks(id);
    }

    public bool HasItem(string name)
    {
        return this.session.Items.AllItemsReceived.ToList().Exists(x => x.ItemName == name);
    }

    private static ArchipelagoSession Connect(string server, int port, string user, string password)
    {
        LoginResult result;
        ArchipelagoSession session = null;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(server, port);
            result = session.TryConnectAndLogin("Archipelamots", user, ItemsHandlingFlags.AllItems);
        }
        catch (Exception e)
        {
            result = new LoginFailure(e.GetBaseException().Message);
        }

        if (!result.Successful)
        {
            LoginFailure failure = (LoginFailure) result;
            string errorMessage = $"Failed to connect to {server}:{port} as {user}:";
            foreach (string error in failure.Errors)
            {
                errorMessage += $"\n    {error}";
            }
            foreach (ConnectionRefusedError error in failure.ErrorCodes)
            {
                errorMessage += $"\n    {error}";
            }

            InfoDialog.Show(errorMessage);
            return null;
        }

        // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be
        // used to interact with the server and the returned `LoginSuccessful` contains some useful information about the
        // initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
        var loginSuccess = (LoginSuccessful) result;
        return session;
    }
}
