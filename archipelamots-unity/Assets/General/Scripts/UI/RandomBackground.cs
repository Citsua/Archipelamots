using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RandomBackground : MonoBehaviour
{
    [SerializeField] private Material[] possibleBackgrounds;

    private int currentBackground;

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
        this.possibleBackgrounds.Shuffle();
        this.Randomize();
    }

    private void Update()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            this.SwitchToNext();
        }
    }

    [Button]
    private void Randomize()
    {
        int random = Random.Range(0, this.possibleBackgrounds.Length);
        this.currentBackground = random;
        this.Image.material = this.possibleBackgrounds[random];
    }

    [Button]
    private void SwitchToNext()
    {
        this.currentBackground = (this.currentBackground + 1) % this.possibleBackgrounds.Length;
        this.Image.material = this.possibleBackgrounds[this.currentBackground];
    }
}
