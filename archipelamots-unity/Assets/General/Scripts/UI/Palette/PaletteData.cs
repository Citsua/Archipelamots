using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "New Palette Data", menuName = "Game/UI/Palette/Data")]
public class PaletteData : ScriptableObject
{
    public TMP_FontAsset font;
    public SerializedDictionary<PaletteColor, Color> palette = new SerializedDictionary<PaletteColor, Color>();

    private static PaletteData instance;
    public static PaletteData Instance
    {
        get
        {
            if (instance == null)
            {
                GetInstance();
            }

            return instance;
        }

        private set
        {
            instance = value;
        }
    }

    // Necessary for static variables to work correctly when domain reload is disabled
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static private void Init()
    {
        GetInstance();
    }

    private static void GetInstance()
    {
        var op = Addressables.LoadAssetAsync<PaletteData>("Palette Data.asset");
        Instance = op.WaitForCompletion();
    }

#if UNITY_EDITOR
    [Button]
    public void UpdateAll()
    {
        List<PaletteUI> paletteColors = new List<PaletteUI>();
        paletteColors.AddRange(CustomEditorUtility.FindAllScripts<PaletteUI>());
        paletteColors.AddRange(FindObjectsByType<PaletteUI>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        foreach (PaletteUI paletteUI in paletteColors)
        {
            paletteUI.ApplyColor(this);
            EditorUtility.SetDirty(paletteUI.gameObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(paletteUI.gameObject);
        }
    }
#endif
}
