using UnityEngine;

public class DefinitionGridSquareFull : DefinitionGridSquare
{
    [SerializeField] private DefinitionSubSquare subSquareBottom;
    [SerializeField] private DefinitionSubSquare subSquareRight;

    public void Initialize(int r, int c, Archipelamots.DefCellInfo defInfo)
    {
        base.Initialize(r, c);
        DefinitionSubSquare subSquare = this.subSquareBottom;
        if (defInfo.arrow == '→' || defInfo.arrow == '⤵')
            subSquare = this.subSquareRight;
        subSquare.Initialize(this, defInfo);

        DefinitionSubSquare otherSubSquare = subSquare == this.subSquareRight ? this.subSquareBottom : this.subSquareRight;
        otherSubSquare.gameObject.SetActive(false);
    }
}
