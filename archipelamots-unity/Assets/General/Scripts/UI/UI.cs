using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance { get; private set; }

    [SerializeField] private TMP_Text letterRevealButtonNumber;
    [SerializeField] private Button letterRevealButton;

    [SerializeField] private TMP_Text wordCheckButtonNumber;
    [SerializeField] private Button wordCheckButton;

    [SerializeField] public RectTransform gridScaler;
    [SerializeField] public RectTransform gridLayout;
    [SerializeField][MinMaxSlider(0.1f, 10f, true)] private Vector2 gridZoomLimits;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float panSpeed;

    public ConnectionUI Connection {  get; private set; }
    public GridSelectorUI GridSelector { get; private set; }
    public NotificationLogUI NotificationLog { get; private set; }

    public bool Panning
    {
        get
        {
            return this.holdingLeftClick && this.hasMovedDuringPanning;
        }
    }

    private RectTransform canvas;
    private bool holdingLeftClick;
    private bool hasMovedDuringPanning;
    private Vector2 initialDragPosition;

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null)
            throw new System.Exception($"{this.GetType()} Singleton already exists in the scene");
        Instance = this;

        this.canvas = this.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        this.Connection = FindFirstObjectByType<ConnectionUI>(FindObjectsInactive.Include);
        this.GridSelector = FindFirstObjectByType<GridSelectorUI>(FindObjectsInactive.Include);
        this.NotificationLog = FindFirstObjectByType<NotificationLogUI>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        ConfirmationDialog.Initialize();
        InfoDialog.Initialize();
        this.GridSelector.Hide(true);
        this.letterRevealButton.onClick.AddListener(GameManager.Instance.UseLetterRevealPower);
        this.wordCheckButton.onClick.AddListener(GameManager.Instance.UseWordCheckPower);
        CrosswordGrid.Current.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!GameManager.Instance.GameStarted)
            return;

        if (Keyboard.current.tabKey.wasReleasedThisFrame)
        {
            this.GridSelector.Toggle();
        }

        float scrollWheel = Mouse.current.scroll.value.y;
        if (scrollWheel != 0)
        {
            // Ignore scrolling if the player is hovering the grid menu
            if (!Utility.DetectUIElementUnderMouse().Contains(this.GridSelector.gameObject))
            {
                float targetScroll = Mathf.Clamp(this.gridScaler.localScale.x + scrollWheel * this.zoomSpeed * Time.deltaTime, this.gridZoomLimits.x, this.gridZoomLimits.y);
                this.gridScaler.localScale = Vector3.one * targetScroll;
            }
        }

        if (this.holdingLeftClick)
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                this.holdingLeftClick = false;
            }
            else
            {
                Vector2 mouseDelta = Mouse.current.delta.value;
                if (this.hasMovedDuringPanning || mouseDelta.magnitude > 1f)
                {
                    this.hasMovedDuringPanning = true;
                    /*RectTransformUtility.ScreenPointToLocalPointInRectangle(this.gridLayout, Mouse.current.position.value, Camera.main, out Vector2 dragPosition);
                    this.gridScaler.anchoredPosition = dragPosition - this.initialDragPosition;*/
                    float xLimit = (this.canvas.sizeDelta.x) / 2f;
                    float yLimit = (this.canvas.sizeDelta.y) / 2f;
                    this.gridScaler.transform.localPosition = new Vector3(
                        Mathf.Clamp(this.gridScaler.transform.localPosition.x + mouseDelta.x * this.panSpeed, -xLimit, xLimit),
                        Mathf.Clamp(this.gridScaler.transform.localPosition.y + mouseDelta.y * this.panSpeed, -yLimit, yLimit),
                        0f);
                }    
            }
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                this.holdingLeftClick = true;
                this.hasMovedDuringPanning = false;
                //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.gridLayout, Mouse.current.position.value, Camera.main, out this.initialDragPosition);
            }
        }
    }

    public void UpdatePowerUI()
    {
        this.letterRevealButtonNumber.text = SavingUtility.GetNumberOfLetterReveals().ToString();
        this.wordCheckButtonNumber.text = SavingUtility.GetNumberOfWordChecks().ToString();
        this.letterRevealButton.interactable = this.CanUseLetterRevealPower();
        this.wordCheckButton.interactable = this.CanUseWordCheckPower();
    }

    public void ResetZoomAndPan()
    {
        this.gridScaler.transform.localPosition = Vector3.zero;
        this.gridScaler.localScale = Vector3.one;
    }

    private bool CanUseLetterRevealPower()
    {
        LetterGridSquare gridSquare = CrosswordGrid.Current.CurrentlySelected;
        return SavingUtility.GetNumberOfLetterReveals() > 0
            && gridSquare != null && !gridSquare.LockedIn;
    }

    private bool CanUseWordCheckPower()
    {
        return SavingUtility.GetNumberOfWordChecks() > 0
            && CrosswordGrid.Current.CurrentlySelected != null
            && !CrosswordGrid.Current.CheckSelectedWordLockedIn(out _)
            && CrosswordGrid.Current.CheckSelectedWordFull();
    }
}
