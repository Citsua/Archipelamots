using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        Instance = null;

        // This is necessary only in the GameManager for Input to work correctly when domain reload is disabled
        // For some reason, without this line, no input is detected at all
        InputSystem.actions.Enable();
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.Exception($"{this.GetType()} Singleton already exists in the scene");
        Instance = this;
    }

    private void Start()
    {
        UI.Instance.Connection.Show();
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                SavingUtility.IncreaseNumberOfLetterReveals();
                SavingUtility.IncreaseNumberOfWordChecks();
                UI.Instance.UpdatePowerUI();
            }
        }
    }

    public void UseLetterRevealPower()
    {
        ConfirmationDialog.Show("Êtes-vous sûr(e) de vouloir révéler la lettre sélectionnée ?", () =>
        {
            SavingUtility.DecreaseNumberOfLetterReveals();
            LetterGridSquare currentlySelected = CrosswordGrid.Current.CurrentlySelected;
            currentlySelected.LockIn();
            UI.Instance.UpdatePowerUI();
        });
    }

    public void UseWordCheckPower()
    {
        ConfirmationDialog.Show("Êtes-vous sûr(e) de vouloir vérifier le mot sélectionné ?", () =>
        {
            SavingUtility.DecreaseNumberOfWordChecks();
            if (CrosswordGrid.Current.CheckSelectedWordCorrect())
            {
                InfoDialog.Show("Le mot est correct !");
                CrosswordGrid.Current.LockInSelectedWord();
            }
            else
            {
                InfoDialog.Show("Le mot est incorrect...");
            }

            UI.Instance.UpdatePowerUI();
        });
    }
}
