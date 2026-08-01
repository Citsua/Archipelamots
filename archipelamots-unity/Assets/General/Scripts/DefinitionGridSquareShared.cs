using UnityEngine;

public class DefinitionGridSquareShared : DefinitionGridSquare
{
    [SerializeField] private DefinitionSubSquare subSquareTop;
    [SerializeField] private DefinitionSubSquare subSquareBottom;

    public void Initialize(int r, int c, string definition1, char arrow1, string definition2, char arrow2)
    {
        base.Initialize(r, c);
        DefinitionSubSquare firstSubSquare = this.subSquareBottom;
        if (arrow1 == '→' || arrow1 == '⤵') // These arrows have to be on the top part
            firstSubSquare = this.subSquareTop;
        DefinitionSubSquare secondSubSquare = firstSubSquare == this.subSquareTop ? this.subSquareBottom : this.subSquareTop;
        firstSubSquare.Initialize(this, definition1, arrow1);
        secondSubSquare.Initialize(this, definition2, arrow2);
    }
}
