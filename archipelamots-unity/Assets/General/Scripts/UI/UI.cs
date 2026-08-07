using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    [SerializeField] private TMP_Text letterRevealButtonNumber;
    [SerializeField] private Button letterRevealButton;

    [SerializeField] private TMP_Text wordCheckButtonNumber;
    [SerializeField] private Button wordCheckButton;

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

    private void Start()
    {
        ConfirmationDialog.Hide();
        InfoDialog.Hide();
        this.letterRevealButton.onClick.AddListener(GameManager.Instance.UseLetterRevealPower);
        this.wordCheckButton.onClick.AddListener(GameManager.Instance.UseWordCheckPower);
    }

    public void UpdatePowerUI()
    {
        this.letterRevealButtonNumber.text = SavingUtility.GetNumberOfLetterReveals().ToString();
        this.wordCheckButtonNumber.text = SavingUtility.GetNumberOfWordChecks().ToString();
        this.letterRevealButton.interactable = this.CanUseLetterRevealPower();
        this.wordCheckButton.interactable = this.CanUseWordCheckPower();
    }

    private bool CanUseLetterRevealPower()
    {
        LetterGridSquare gridSquare = CrosswordGrid.Current.CurrentlySelected;
        return SavingUtility.GetNumberOfLetterReveals() > 0
            && gridSquare != null && !gridSquare.LockedIn;
    }

    private bool CanUseWordCheckPower()
    {
        return SavingUtility.GetNumberOfWordChecks() > 0
            && CrosswordGrid.Current.CurrentlySelected != null
            && !CrosswordGrid.Current.CheckSelectedWordLockedIn(out _)
            && CrosswordGrid.Current.CheckSelectedWordFull();
    }
}
