using DG.Tweening;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private TweenInfo showTween;
    [SerializeField] private TweenInfo hideTween;
    [SerializeField] private float displayDuration = 6f;

    private CanvasGroup canvasGroup;

    public void Initialize(string text)
    {
        this.canvasGroup = this.GetComponent<CanvasGroup>();
        this.text.text = text;
        this.transform.localScale = Vector3.zero;
        this.transform.DOScale(1f, this.showTween);
        this.Invoke(nameof(this.Hide), this.displayDuration);
    }

    private void Hide()
    {
        this.canvasGroup.DOFade(0f, this.hideTween);
        Destroy(this.gameObject, this.hideTween.duration + 0.01f);
    }
}
