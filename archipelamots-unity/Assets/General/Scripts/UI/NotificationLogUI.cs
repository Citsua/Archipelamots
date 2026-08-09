using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using System.Collections.Generic;
using UnityEngine;

public class NotificationLogUI : MonoBehaviour
{
    [SerializeField] private int maxNotificationsOnScreen = 3;
    [SerializeField] private float minDelayBetweenNotifications = 1f;
    [SerializeField] private Notification notificationPrefab;
    [SerializeField] private RectTransform notificationAnchor;

    private List<Notification> currentlyShownNotifications = new List<Notification>();
    private Queue<string> notifications = new Queue<string>();

    private float notificationCooldown;

    private void FixedUpdate()
    {
        if (this.currentlyShownNotifications.Count > 0 && this.currentlyShownNotifications[0] == null)
        {
            this.currentlyShownNotifications.RemoveAt(0);
        }

        if (Time.time > this.notificationCooldown && this.notifications.Count > 0 && this.currentlyShownNotifications.Count < this.maxNotificationsOnScreen)
        {
            string newNotification = this.notifications.Dequeue();
            this.ShowNotification(newNotification);
        }
    }

    public void SendLocation(string itemFound, PlayerInfo playerInfo)
    {
        this.AddNotificationToQueue($"Envoyé {itemFound} à {playerInfo.Name} sur {playerInfo.Game}");
    }

    public void ReceiveItem(ItemInfo item)
    {
        this.AddNotificationToQueue($"Reçu {item.ItemDisplayName} de la part de {item.Player.Name} sur {item.Player.Game}");
    }

    private void AddNotificationToQueue(string text)
    {
        this.notifications.Enqueue(text);
    }

    private void ShowNotification(string text)
    {
        Notification notification = Instantiate(this.notificationPrefab, this.notificationAnchor);
        notification.Initialize(text);
        this.currentlyShownNotifications.Add(notification);
        this.notificationCooldown = Time.time + this.minDelayBetweenNotifications;
    }
}
