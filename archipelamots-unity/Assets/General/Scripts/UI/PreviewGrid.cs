using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreviewGrid : CrosswordGrid
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject locked;

    protected override void Awake()
    {

    }

    public override void Initialize(int gridNb)
    {
        base.Initialize(gridNb);
        bool locked = !ServerConnector.Instance.HasItem($"Grid n°{gridNb + 1}");
        this.locked.SetActive(locked);
        this.button.interactable = !locked && Current.GridNb != this.GridNb;
        this.button.onClick.AddListener(this.OnClick);
    }

    private void OnClick()
    {
        Current.Reinitialize(this.GridNb);
        UI.Instance.GridSelector.Initialize();
    }
}
