using UnityEngine;

public class DefinitionGridSquareFull : DefinitionGridSquare
{
    [SerializeField] private DefinitionSubSquare subSquare;

    public void Initialize(CrosswordGrid grid, int r, int c, YAML.DefCellInfo defInfo)
    {
        base.Initialize(grid, r, c);
        this.subSquare.Initialize(this, defInfo);
    }
}
