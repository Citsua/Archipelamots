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

    public static void Initialize()
    {
        InfoDialog dialog = FindFirstObjectByType<InfoDialog>(FindObjectsInactive.Include);

        if (Instance != null)
            throw new System.Exception($"{dialog.GetType()} Singleton already exists in the scene");
        Instance = dialog;

        dialog.exitButton.onClick.AddListener(dialog.OnClickExit);
        dialog.gameObject.SetActive(false);
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

    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
    }
}
