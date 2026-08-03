using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DefinitionSubSquare : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image arrow;

    public DefinitionGridSquare ParentSquare { get; private set; }
    public int StartingR { get; private set; }
    public int StartingC { get; private set; }
    public Direction Direction { get; private set; }

    public void Initialize(DefinitionGridSquare parentSquare, string definition, char arrow)
    {
        this.ParentSquare = parentSquare;
        this.text.text = definition;
        this.arrow.sprite = CrosswordGrid.Instance.arrowSprites.Find(x => x.character == arrow).sprite;

        switch (arrow)
        {
            case '→':
                this.Direction = Direction.Horizontal;
                this.StartingC = this.ParentSquare.C + 1;
                this.StartingR = this.ParentSquare.R;
                break;
            case '⤵':
                this.Direction = Direction.Vertical;
                this.StartingC = this.ParentSquare.C + 1;
                this.StartingR = this.ParentSquare.R;
                break;
            case '↳':
                this.Direction = Direction.Horizontal;
                this.StartingC = this.ParentSquare.C;
                this.StartingR = this.ParentSquare.R + 1;
                break;
            case '↓':
                this.Direction = Direction.Vertical;
                this.StartingC = this.ParentSquare.C;
                this.StartingR = this.ParentSquare.R + 1;
                break;
        }

        this.arrow.transform.SetParent(CrosswordGrid.Instance.transform, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            this.OnClick();
    }

    public void OnClick()
    {
        CrosswordGrid.Instance.LastClicked = this.ParentSquare;
        CrosswordGrid.Instance.Select(this);
    }
}
