using UnityEngine;

public class DefinitionGridSquareFull : DefinitionGridSquare
{
    [SerializeField] private DefinitionSubSquare subSquareBottom;
    [SerializeField] private DefinitionSubSquare subSquareRight;

    public void Initialize(int r, int c, string definition, char arrow)
    {
        base.Initialize(r, c);
        DefinitionSubSquare subSquare = this.subSquareBottom;
        if (arrow == '→' || arrow == '⤵')
            subSquare = this.subSquareRight;
        subSquare.Initialize(this, definition, arrow);

        DefinitionSubSquare otherSubSquare = subSquare == this.subSquareRight ? this.subSquareBottom : this.subSquareRight;
        otherSubSquare.gameObject.SetActive(false);
    }
}
