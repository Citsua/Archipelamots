using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

public class ServerConnector : MonoBehaviour
{
    public string server;
    public int port;
    public string user;
    public string password;

    private ArchipelagoSession session;

    private void Start()
    {
        Debug.Log("start");
        this.session = Connect(this.server, this.port, this.user, this.password);
        if (this.session != null)
        {
            session.Items.ItemReceived += OnItemReceived;
            session.Locations.CheckedLocationsUpdated += this.OnCheckedLocationsUpdated;
            this.session.Locations.CompleteLocationChecks(1);
            Debug.Log("complete loc 1");
        }
    }

    private void OnItemReceived(ReceivedItemsHelper helper)
    {
        ItemInfo info = helper.DequeueItem();
        Debug.Log("received item " + info.ItemName);
    }

    private void OnCheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations)
    {
        foreach (long i in newCheckedLocations)
        {
            Debug.Log("checked location " + i);
        }
    }

    private static ArchipelagoSession Connect(string server, int port, string user, string password)
    {
        LoginResult result;
        ArchipelagoSession session = null;

        try
        {
            session = ArchipelagoSessionFactory.CreateSession(server, port);
            // handle TryConnectAndLogin attempt here and save the returned object to `result`
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

            return null; // Did not connect, show the user the contents of `errorMessage`
        }

        // Successfully connected, `ArchipelagoSession` (assume statically defined as `session` from now on) can now be
        // used to interact with the server and the returned `LoginSuccessful` contains some useful information about the
        // initial connection (e.g. a copy of the slot data as `loginSuccess.SlotData`)
        var loginSuccess = (LoginSuccessful) result;
        return session;
    }
}
