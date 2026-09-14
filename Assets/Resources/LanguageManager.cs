using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[ExecuteAlways]
public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    public enum Language
    {
        English,
        Hindi,
        Gujarati
    }

    public Language currentLanguage = Language.English;

    [Header("Language Fonts")]
    public TMP_FontAsset englishFont;
    public TMP_FontAsset hindiFont;
    public TMP_FontAsset gujaratiFont;

    public Dictionary<string, string[]> languageData =
        new Dictionary<string, string[]>();

    private List<LocalizedText> localizedTexts =
        new List<LocalizedText>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (Application.isPlaying)
                DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this && Application.isPlaying)
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("Language"))
        {
            currentLanguage = (Language)Mathf.Clamp(PlayerPrefs.GetInt("Language", 0), 0, 2);
        }

        InitFonts();
        LoadCSV();
    }

    private void OnEnable()
    {
        if (Instance == null) Instance = this;
        InitFonts();
        if (languageData.Count == 0) LoadCSV();
    }

    public void InitFonts()
    {
        if (englishFont == null)
            englishFont = Resources.Load<TMP_FontAsset>("rwizenEnglishSDF") ?? Resources.Load<TMP_FontAsset>("Fonts/rwizenEnglishSDF");

        if (hindiFont == null)
            hindiFont = Resources.Load<TMP_FontAsset>("rwizenHindiSDF") ?? Resources.Load<TMP_FontAsset>("Fonts/rwizenHindiSDF") ?? Resources.Load<TMP_FontAsset>("Tiro_Devanagari_Hindi/TiroDevanagariHindi-Regular SDF");

        if (gujaratiFont == null)
            gujaratiFont = Resources.Load<TMP_FontAsset>("rwizenGujaratiSDF") ?? Resources.Load<TMP_FontAsset>("Fonts/rwizenGujaratiSDF");

#if UNITY_EDITOR
        if (englishFont == null)
            englishFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/rwizenEnglishSDF.asset");
        if (hindiFont == null)
            hindiFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/rwizenHindiSDF.asset");
        if (gujaratiFont == null)
            gujaratiFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/rwizenGujaratiSDF.asset");
#endif
    }

    public void LoadCSV()
    {
        languageData.Clear();
        TextAsset csvFile = Resources.Load<TextAsset>("Language");
#if UNITY_EDITOR
        if (csvFile == null)
        {
            csvFile = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Language.csv");
        }
#endif
        if (csvFile == null)
        {
            Debug.LogWarning("Language.csv not found in Resources!");
            return;
        }

        string text = csvFile.text;
        string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] values = SplitCsvLine(lines[i]);

            if (values.Length >= 4)
            {
                string key = values[0].Trim();
                if (!languageData.ContainsKey(key))
                    languageData.Add(key, values);
                else
                    languageData[key] = values;
            }
        }
    }

    private static string[] SplitCsvLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder current = new StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(CleanCsvCell(current.ToString()));
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(CleanCsvCell(current.ToString()));
        return result.ToArray();
    }

    private static string CleanCsvCell(string raw)
    {
        string trimmed = raw.Trim();
        if (trimmed.StartsWith("\"") && trimmed.EndsWith("\"") && trimmed.Length >= 2)
        {
            trimmed = trimmed.Substring(1, trimmed.Length - 2).Replace("\"\"", "\"");
        }
        return trimmed.Replace("\\n", "\n");
    }

    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (languageData.Count == 0)
            LoadCSV();

        if (!languageData.ContainsKey(key))
            return key;

        string[] row = languageData[key];

        switch (currentLanguage)
        {
            case Language.English:
                return row.Length > 1 ? row[1] : key;

            case Language.Hindi:
                return row.Length > 2 ? row[2] : key;

            case Language.Gujarati:
                return row.Length > 3 ? row[3] : key;
        }

        return key;
    }

    public TMP_FontAsset GetFont()
    {
        switch (currentLanguage)
        {
            case Language.Hindi:
                return hindiFont != null ? hindiFont : englishFont;

            case Language.Gujarati:
                return gujaratiFont != null ? gujaratiFont : englishFont;

            default:
                return englishFont;
        }
    }

    public void ChangeLanguage(Language lang)
    {
        currentLanguage = lang;

        PlayerPrefs.SetInt("Language", (int)lang);
        PlayerPrefs.Save();

        InitFonts();
        if (languageData.Count == 0)
            LoadCSV();

        var allLoc = FindObjectsOfType<LocalizedText>(true);
        foreach (var loc in allLoc)
        {
            if (loc != null && !localizedTexts.Contains(loc))
                localizedTexts.Add(loc);
        }

        for (int i = localizedTexts.Count - 1; i >= 0; i--)
        {
            if (localizedTexts[i] != null)
                localizedTexts[i].UpdateText();
            else
                localizedTexts.RemoveAt(i);
        }
    }

    public void Register(LocalizedText text)
    {
        if (text != null && !localizedTexts.Contains(text))
            localizedTexts.Add(text);
    }

    public void UnRegister(LocalizedText text)
    {
        localizedTexts.Remove(text);
    }
}