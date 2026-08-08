using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GridSelectorUI : MonoBehaviour
{
    [SerializeField] private PreviewGrid previewGridPrefab;
    [SerializeField] private RectTransform previewGridsAnchor;
    [SerializeField] private RectTransform mainGridTransform;
    [SerializeField] private TweenInfo showTween;
    [SerializeField] private TweenInfo hideTween;

    public bool Visible { get; private set; }

    private RectTransform rectTransform;
    private RectTransform RectTransform
    {
        get
        {
            if (this.rectTransform == null)
            {
                this.rectTransform = this.GetComponent<RectTransform>();
            }

            return this.rectTransform;
        }
    }

    public void Toggle()
    {
        if (this.Visible)
        {
            this.Hide();
        }
        else
        {
            this.Show();
        }
    }

    public void Show()
    {
        this.Visible = true;
        this.Initialize();
        this.RectTransform.DOKill();
        this.mainGridTransform.DOKill();
        this.RectTransform.DOAnchorPosX(0f, this.showTween);
        this.mainGridTransform.DOOffsetLeft(this.rectTransform.sizeDelta.x, this.showTween);
    }

    public void Hide(bool instantaneous = false)
    {
        this.Visible = false;
        this.RectTransform.DOKill();
        this.mainGridTransform.DOKill();
        if (instantaneous)
        {
            this.RectTransform.anchoredPosition = new Vector2(-this.rectTransform.sizeDelta.x, this.RectTransform.anchoredPosition.y);
            this.mainGridTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            this.RectTransform.DOAnchorPosX(-this.rectTransform.sizeDelta.x, this.hideTween);
            this.mainGridTransform.DOOffsetLeft(0f, this.hideTween);
        }
    }

    public void Initialize()
    {
        this.previewGridsAnchor.KillAllChildren();
        for (int i = 0; i < YAMLLoader.Instance.YAML.Archipelamots.total_nb_of_grids; i++)
        {
            PreviewGrid grid = Instantiate(this.previewGridPrefab, this.previewGridsAnchor);
            grid.Initialize(i);
        }
        this.Invoke(nameof(this.RebuildLayout), 0.05f);
    }

    private void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.previewGridsAnchor);
    }
}
