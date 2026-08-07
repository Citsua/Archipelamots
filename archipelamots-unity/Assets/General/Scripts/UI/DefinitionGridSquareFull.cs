using UnityEngine;

public class DefinitionGridSquareFull : DefinitionGridSquare
{
    [SerializeField] private DefinitionSubSquare subSquareBottom;
    [SerializeField] private DefinitionSubSquare subSquareRight;

    public void Initialize(CrosswordGrid grid, int r, int c, YAML.DefCellInfo defInfo)
    {
        base.Initialize(grid, r, c);
        DefinitionSubSquare subSquare = this.subSquareBottom;
        if (defInfo.arrow == '→' || defInfo.arrow == '⤵')
            subSquare = this.subSquareRight;
        subSquare.Initialize(this, defInfo);

        DefinitionSubSquare otherSubSquare = subSquare == this.subSquareRight ? this.subSquareBottom : this.subSquareRight;
        otherSubSquare.gameObject.SetActive(false);
    }
}
