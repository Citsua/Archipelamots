using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ConfirmationDialog : MonoBehaviour
{
    private static ConfirmationDialog Instance { get; set; }

    [SerializeField] private TMP_Text text;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private System.Action storedActionOnConfirm;

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

        this.confirmButton.onClick.AddListener(this.OnClickConfirm);
        this.cancelButton.onClick.AddListener(this.OnClickCancel);
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasReleasedThisFrame)
        {
            this.OnClickConfirm();
        }
        else if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            this.OnClickCancel();
        }
    }

    public static void Show(string dialogMessage, System.Action actionOnConfirm)
    {
        Instance.storedActionOnConfirm = actionOnConfirm;
        Instance.text.text = dialogMessage;
        Instance.cancelButton.gameObject.SetActive(true);
        Instance.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        Instance.gameObject.SetActive(false);
    }

    private void OnClickConfirm()
    {
        if (this.storedActionOnConfirm != null)
        {
            this.storedActionOnConfirm();
            this.storedActionOnConfirm = null;
        }

        this.gameObject.SetActive(false);
    }

    private void OnClickCancel()
    {
        this.storedActionOnConfirm = null;
        this.gameObject.SetActive(false);
    }
}
