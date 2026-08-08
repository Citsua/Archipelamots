using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class RandomBackground : MonoBehaviour
{
    [SerializeField] private Texture2D[] possibleBackgrounds;

    private Image image;
    private Image Image
    {
        get
        {
            if (this.image == null)
            {
                this.image = this.GetComponent<Image>();
            }

            return this.image;
        }
    }

    private void Start()
    {
        this.Randomize();
    }

    [Button]
    private void Randomize()
    {
        this.Image.material.SetTexture("_Texture", this.possibleBackgrounds[Random.Range(0, this.possibleBackgrounds.Length)]);
    }
}
