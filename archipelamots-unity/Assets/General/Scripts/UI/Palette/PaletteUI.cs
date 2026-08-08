using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaletteUI : MonoBehaviour
{
    public PaletteColor color;
    public bool overrideFont;
    public bool overrideColor;
    public bool overrideOpacity;

    public void ApplyColor(PaletteData data)
    {
        if (this.color == null)
            return;

        if (this.TryGetComponent(out Image image))
        {
            if (!this.overrideColor)
            {
                image.color = this.Color(data, image.color);
            }
        }
        if (this.TryGetComponent(out TMP_Text text))
        {
            if (!this.overrideColor)
            {
                text.color = this.Color(data, text.color);
            }
            if (!this.overrideFont)
            {
                text.font = data.font;
            }
        }
    }

    private Color Color(PaletteData data, Color startingColor)
    {
        return new Color(data.palette[this.color].r, data.palette[this.color].g, data.palette[this.color].b, this.overrideOpacity ? startingColor.a : data.palette[this.color].a);
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        this.ApplyColor(PaletteData.Instance);
    }
#endif
}
