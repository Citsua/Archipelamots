using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    public static ServerConnector Instance { get; private set; }

    private Dictionary<string, int> itemsReceived = new Dictionary<string, int>();
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

    private void FixedUpdate()
    {
        if (this.session == null || !CrosswordGrid.Current.Initialized)
            return;

        this.CheckForNewItems();
    }

    private void OnDestroy()
    {
        if (this.session != null)
        {
            this.session.Socket.DisconnectAsync();
        }
    }

    public void Connect()
    {
        this.session = this.Connect(UI.Instance.Connection.Address, UI.Instance.Connection.Port, UI.Instance.Connection.Slot, UI.Instance.Connection.Password);
        if (this.session != null)
        {
            YAMLLoader.Instance.ShowFileBrowser();
        }
    }

    private void CheckForNewItems()
    {
        ItemInfo info = this.session.Items.PeekItem();
        if (info != null)
        {
            this.session.Items.DequeueItem();
            this.ReceiveItem(info);
        } 
    }

    private void ReceiveItem(ItemInfo info)
    {
        if (this.itemsReceived.ContainsKey(info.ItemName))
        {
            this.itemsReceived[info.ItemName]++;
        }
        else
        {
            this.itemsReceived.Add(info.ItemName, 1);
        }

        SavingUtility.ReceiveItem(ref this.itemsReceived, info);
    }

    public void SendLocationCheck(string name)
    {
        long id = this.session.Locations.GetLocationIdFromName("Archipelamots", name);
        if (this.session.Locations.AllLocationsChecked.Contains(id))
            return;

        Debug.Log($"Completed Check '{name}' (id '{id}')");
        this.StartCoroutine(this.CScoutLocation(id));
        this.session.Locations.CompleteLocationChecks(id);
    }

    private IEnumerator CScoutLocation(long id)
    {
        var task = this.session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, id);
        yield return new WaitUntil(() => task.IsCompleted);
        UI.Instance.NotificationLog.SendLocation(task.Result[id].ItemName, task.Result[id].Player);
    }

    private ArchipelagoSession Connect(string server, int port, string user, string password)
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
