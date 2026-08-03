using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class GridSquare : MonoBehaviour
{
    public int R { get; private set; }
    public int C { get; private set; }

    public virtual void Initialize(int r, int c)
    {
        this.R = r;
        this.C = c;
    }

    public abstract void Deselect();
}
