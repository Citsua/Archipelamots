using UnityEngine;

public class DefinitionGridSquareShared : DefinitionGridSquare
{
    [SerializeField] private DefinitionSubSquare subSquareTop;
    [SerializeField] private DefinitionSubSquare subSquareBottom;

    public void Initialize(int r, int c, Archipelamots.DefCellInfo defInfo1, Archipelamots.DefCellInfo defInfo2)
    {
        base.Initialize(r, c);
        DefinitionSubSquare firstSubSquare = this.subSquareBottom;
        if (defInfo1.arrow == '→' || defInfo1.arrow == '⤵') // These arrows have to be on the top part
            firstSubSquare = this.subSquareTop;
        DefinitionSubSquare secondSubSquare = firstSubSquare == this.subSquareTop ? this.subSquareBottom : this.subSquareTop;
        firstSubSquare.Initialize(this, defInfo1);
        secondSubSquare.Initialize(this, defInfo2);
    }
}
