using UnityEngine;

[ExecuteAlways]
public class LanguageUI : MonoBehaviour
{
    private LanguageManager GetManager()
    {
        if (LanguageManager.Instance == null)
            LanguageManager.Instance = FindObjectOfType<LanguageManager>();
        return LanguageManager.Instance;
    }

    public void English()
    {
        var manager = GetManager();
        if (manager != null)
            manager.ChangeLanguage(LanguageManager.Language.English);
    }

    public void Hindi()
    {
        var manager = GetManager();
        if (manager != null)
            manager.ChangeLanguage(LanguageManager.Language.Hindi);
    }

    public void Gujarati()
    {
        var manager = GetManager();
        if (manager != null)
            manager.ChangeLanguage(LanguageManager.Language.Gujarati);
    }

    public void SetLanguageByIndex(int index)
    {
        if (index == 0) English();
        else if (index == 1) Hindi();
        else if (index == 2) Gujarati();
    }

    public void SetLanguageByName(string languageName)
    {
        if (string.Equals(languageName, "English", System.StringComparison.OrdinalIgnoreCase)) English();
        else if (string.Equals(languageName, "Hindi", System.StringComparison.OrdinalIgnoreCase)) Hindi();
        else if (string.Equals(languageName, "Gujarati", System.StringComparison.OrdinalIgnoreCase)) Gujarati();
    }
}