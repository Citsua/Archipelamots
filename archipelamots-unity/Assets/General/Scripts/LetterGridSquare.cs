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

    public override void Initialize(int r, int c)
    {
        base.Initialize(r, c);
        this.text.text = string.Empty;
        this.mainSelected.SetActive(false);
        this.secondarySelected.SetActive(false);
    }

    public void OnClick()
    {
        if (CrosswordGrid.Instance.LastClicked == this)
        {
            CrosswordGrid.Instance.SwitchDirection();
        }

        CrosswordGrid.Instance.LastClicked = this;
        CrosswordGrid.Instance.Select(this);
    }

    public void OnRightClick()
    {
        this.Erase();
    }

    public void Set(char letter)
    {
        if (this.LockedIn)
            return;

        this.text.text = letter.ToString();
    }

    public void Erase()
    {
        if (this.LockedIn)
            return;

        this.text.text = string.Empty;
    }

    public void LockIn()
    {
        this.LockedIn = true;
        this.lockedIn.SetActive(true);
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
        if (eventData.button == PointerEventData.InputButton.Left)
            this.OnClick();
        else if (eventData.button == PointerEventData.InputButton.Right)
            this.OnRightClick();
    }
}
