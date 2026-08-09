using UnityEngine;
using UnityEngine.UI;

public class LayoutForceRebuilder : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Start()
    {
        this.rectTransform = this.transform as RectTransform;
    }

    private void Update()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
    }
}
