using TMPro;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class LocalizedText : MonoBehaviour
{
    public string key;

    private TMP_Text textObj;

    private void Awake()
    {
        EnsureTextComponent();
    }

    private void OnEnable()
    {
        EnsureTextComponent();
        RegisterAndRefresh();
    }

    private void Start()
    {
        EnsureTextComponent();
        RegisterAndRefresh();
    }

    private void OnDisable()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.UnRegister(this);
    }

    private void OnDestroy()
    {
        if (LanguageManager.Instance != null)
            LanguageManager.Instance.UnRegister(this);
    }

    private void EnsureTextComponent()
    {
        if (textObj == null)
            textObj = GetComponent<TMP_Text>();
    }

    public void RegisterAndRefresh()
    {
        if (LanguageManager.Instance == null)
            LanguageManager.Instance = FindObjectOfType<LanguageManager>();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.Register(this);
            UpdateText();
        }
    }

    public void UpdateText()
    {
        EnsureTextComponent();
        if (textObj == null)
            return;

        if (LanguageManager.Instance == null)
            LanguageManager.Instance = FindObjectOfType<LanguageManager>();

        if (LanguageManager.Instance != null)
        {
            LanguageManager.Instance.InitFonts();
            var font = LanguageManager.Instance.GetFont();
            if (font != null)
                textObj.font = font;

            if (!string.IsNullOrEmpty(key))
            {
                string localized = LanguageManager.Instance.GetText(key);
                if (!string.IsNullOrEmpty(localized) && localized != key)
                {
                    textObj.text = localized;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EnsureTextComponent();
        UpdateText();
    }
#endif
}