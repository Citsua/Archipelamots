using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class GridSquare : MonoBehaviour
{
    public CrosswordGrid Grid { get; private set; }
    public int R { get; private set; }
    public int C { get; private set; }

    public virtual void Initialize(CrosswordGrid grid, int r, int c)
    {
        this.Grid = grid;
        this.R = r;
        this.C = c;
    }

    public abstract void Deselect();
}
