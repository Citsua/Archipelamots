using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterGridSquare : GridSquare, IPointerClickHandler
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private GameObject mainSelected;
    [SerializeField] private GameObject secondarySelected;
    [SerializeField] private GameObject lockedIn;

    public bool LockedIn { get; private set; } = false;
    public char Character { get; private set; }

    public override void Initialize(CrosswordGrid grid, int r, int c)
    {
        base.Initialize(grid, r, c);
        this.text.text = string.Empty;
        this.mainSelected.SetActive(false);
        this.secondarySelected.SetActive(false);
        this.lockedIn.SetActive(false);
    }

    public void OnClick()
    {
        if (this.Grid.LastClicked == this)
        {
            this.Grid.SwitchDirection();
        }

        this.Grid.LastClicked = this;
        this.Grid.Select(this);
    }

    public void OnRightClick()
    {
        this.Erase();
    }

    public void Set(char letter, bool save = true)
    {
        if (this.LockedIn)
            return;

        this.Character = letter;
        this.text.text = letter.ToString();

        if (save)
            SavingUtility.SaveGridCharacter(this, this.Character);

        UI.Instance.UpdatePowerUI();
    }

    public void Erase(bool save = true)
    {
        if (this.LockedIn)
            return;

        this.Character = '\0';
        this.text.text = string.Empty;

        if (save)
            SavingUtility.SaveGridCharacter(this, this.Character);

        UI.Instance.UpdatePowerUI();
    }

    public void LockIn(bool save = true)
    {
        if (this.LockedIn)
            return;

        this.Set(YAMLLoader.Instance.Grids[this.Grid.GridNb].grid[this.R][this.C], false);

        this.LockedIn = true;
        this.lockedIn.SetActive(true);

        if (save)
            SavingUtility.SaveGridCharacter(this, this.text.text[0]);

        this.Grid.CheckJustFinishedWord(this);
        this.Grid.CheckGridFinished();
        UI.Instance.UpdatePowerUI();
    }

    public void MainSelect()
    {
        this.mainSelected.SetActive(true);
        this.secondarySelected.SetActive(false);
    }

    public void SecondarySelect()
    {
        this.mainSelected.SetActive(false);
        this.secondarySelected.SetActive(true);
    }

    public override void Deselect()
    {
        this.mainSelected.SetActive(false);
        this.secondarySelected.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UI.Instance.Panning)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
            this.OnClick();
        else if (eventData.button == PointerEventData.InputButton.Right)
            this.OnRightClick();
    }
}
