using UnityEngine;

public abstract class DefinitionGridSquare : GridSquare
{
    [SerializeField] private GameObject secondarySelected;

    public void SecondarySelect()
    {
        this.secondarySelected.SetActive(true);
    }

    public void Deselect()
    {
        this.secondarySelected.SetActive(false);
    }
}
