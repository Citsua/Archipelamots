using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RandomBackground : MonoBehaviour
{
    [SerializeField] private Material[] possibleBackgrounds;

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

    private void Update()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            this.Randomize();
        }
    }

    [Button]
    private void Randomize()
    {
        this.Image.material = this.possibleBackgrounds[Random.Range(0, this.possibleBackgrounds.Length)];
    }
}
