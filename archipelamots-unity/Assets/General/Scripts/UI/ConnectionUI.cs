using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    private const string SERVER_ADDRESS_KEY = "SERVER_ADDRESS";
    private const string SERVER_PORT_KEY = "SERVER_PORT";
    private const string SERVER_SLOT_KEY = "SERVER_SLOT";
    private const string SERVER_PASSWORD_KEY = "SERVER__PASSWORD";

    [SerializeField] private TMP_InputField serverAddress;
    [SerializeField] private TMP_InputField serverPort;
    [SerializeField] private TMP_InputField serverSlot;
    [SerializeField] private TMP_InputField serverPassword;
    [SerializeField] private Button connectButton;

    public string Address
    {
        get
        {
            return this.serverAddress.text;
        }
    }

    public int Port
    {
        get
        {
            if (int.TryParse(this.serverPort.text, out int result))
            {
                return result;
            }

            return 0;
        }
    }

    public string Slot
    {
        get
        {
            return this.serverSlot.text;
        }
    }

    public string Password
    {
        get
        {
            return this.serverPassword.text;
        }
    }

    private void Start()
    {
        this.LoadValue(SERVER_ADDRESS_KEY, this.serverAddress);
        this.LoadValue(SERVER_PORT_KEY, this.serverPort);
        this.LoadValue(SERVER_SLOT_KEY, this.serverSlot);
        this.LoadValue(SERVER_PASSWORD_KEY, this.serverPassword);
        this.connectButton.onClick.AddListener(this.Connect);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Connect()
    {
        this.SaveValue(SERVER_ADDRESS_KEY, this.serverAddress);
        this.SaveValue(SERVER_PORT_KEY, this.serverPort);
        this.SaveValue(SERVER_SLOT_KEY, this.serverSlot);
        this.SaveValue(SERVER_PASSWORD_KEY, this.serverPassword);
        ServerConnector.Instance.Connect();
    }

    private void LoadValue(string key, TMP_InputField inputField)
    {
        if (PlayerPrefs.HasKey(key))
        {
            inputField.text = PlayerPrefs.GetString(key);
        }
    }

    private void SaveValue(string key, TMP_InputField inputField)
    {
        PlayerPrefs.SetString(key, inputField.text);
    }
}
