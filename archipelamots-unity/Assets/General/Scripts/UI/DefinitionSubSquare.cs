using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class DefinitionSubSquare : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image notRevealedBackground;
    [SerializeField] private Image revealedBackground;

    [SerializeField] private SerializedDictionary<char, Image> arrows = new SerializedDictionary<char, Image>();

    public DefinitionGridSquare ParentSquare { get; private set; }
    public int StartingR { get; private set; }
    public int StartingC { get; private set; }
    public Direction Direction { get; private set; }
    private Image activeArrow;

    public void Initialize(DefinitionGridSquare parentSquare, YAML.DefCellInfo defCellInfo)
    {
        this.ParentSquare = parentSquare;
        YAML.Definition definition = YAMLLoader.Instance.Grids[this.ParentSquare.Grid.GridNb].GetDefinition(defCellInfo.word, out int index);
        bool revealed = definition.revealed || ServerConnector.Instance.HasItem($"Definition n°{index + 1} from Grid n°{this.ParentSquare.Grid.GridNb + 1}");
        this.text.text = revealed ? $"N°{index + 1} : {definition.definition}" : $"DEFINITION N°{index + 1}";
        this.notRevealedBackground.gameObject.SetActive(!revealed);

        // Hide other arrows
        foreach (var item in this.arrows)
        {
            item.Value.gameObject.SetActive(false);
        }

        // The arrows were annoying me on the preview grid so I just deleted them
        if (this.ParentSquare.Grid is not PreviewGrid)
        {
            this.activeArrow = this.arrows[defCellInfo.arrow];
            this.activeArrow.gameObject.SetActive(true);
            this.activeArrow.transform.SetParent(UI.Instance.gridScaler, true);
            this.activeArrow.color = revealed ? this.revealedBackground.color : this.notRevealedBackground.color;
        }
           

        switch (defCellInfo.arrow)
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
            case '⤓':
                this.Direction = Direction.Vertical;
                this.StartingC = this.ParentSquare.C - 1;
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
            case '↱':
                this.Direction = Direction.Horizontal;
                this.StartingC = this.ParentSquare.C;
                this.StartingR = this.ParentSquare.R - 1;
                break;
            default:
                Debug.LogError(defCellInfo.arrow + " : " + definition.word + ", " + this.ParentSquare.Grid.GridNb);
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UI.Instance.Panning)
            return;

        if (eventData.button == PointerEventData.InputButton.Left)
            this.OnClick();
    }

    public void OnClick()
    {
        this.ParentSquare.Grid.LastClicked = this.ParentSquare;
        this.ParentSquare.Grid.Select(this);
    }

    private void OnDestroy()
    {
        if (this.activeArrow != null)
        {
            Destroy(this.activeArrow.gameObject);
        }
    }
}
