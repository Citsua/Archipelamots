using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InfoDialog : MonoBehaviour
{
    private static InfoDialog Instance { get; set; }

    [SerializeField] private TMP_Text text;
    [SerializeField] private Button exitButton;

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

        this.exitButton.onClick.AddListener(this.OnClickExit);
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasReleasedThisFrame || Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            this.OnClickExit();
        }
    }

    public static void Show(string dialogMessage)
    {
        Instance.text.text = dialogMessage;
        Instance.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        Instance.gameObject.SetActive(false);
    }

    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }
}
