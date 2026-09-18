#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class QuizAutoSetupWindow : EditorWindow
{
    [Serializable]
    private class QuizImportData
    {
        // General information/description for this project/topic.
        // This will be placed in the TMP text assigned to "Info Text".
        public string info;

        public QuizImportQuestion[] questions;
    }

    [Serializable]
    private class QuizImportQuestion
    {
        public string questionText;
        public string[] options;
        public int correctAnswerIndex;
        public string questionVoice;
        public string correctAnswerVoice;
    }

    private QuizManager quizManager;

    // Optional information panel TMP reference.
    // Drag the TextMeshProUGUI/TMP_Text that should display the project's info.
    private TMP_Text infoText;

    private TextAsset jsonFile;

    [TextArea(15, 40)]
    private string jsonText = "";

    private Vector2 scroll;

    [MenuItem("Tools/Quiz/Auto Setup + Question Importer")]
    public static void Open()
    {
        GetWindow<QuizAutoSetupWindow>("Quiz Auto Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("QUIZ AUTO SETUP", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "This tool can automatically assign the references used by your existing QuizManager " +
            "and can also import all questions/options/audio names from JSON.",
            MessageType.Info);

        quizManager = (QuizManager)EditorGUILayout.ObjectField(
            "Quiz Manager", quizManager, typeof(QuizManager), true);

        infoText = (TMP_Text)EditorGUILayout.ObjectField(
            "Info Text", infoText, typeof(TMP_Text), true);

        EditorGUILayout.HelpBox(
            "Info Text is optional. Drag the TextMeshPro text from your Info Panel here. " +
            "The top-level 'info' value from the selected JSON will be placed into this text automatically.",
            MessageType.None);

        EditorGUILayout.Space(8);

        using (new EditorGUI.DisabledScope(quizManager == null))
        {
            if (GUILayout.Button("1. AUTO ASSIGN ALL REFERENCES", GUILayout.Height(36)))
            {
                AutoAssignReferences();
            }

            if (GUILayout.Button("2. VALIDATE REFERENCES", GUILayout.Height(30)))
            {
                ValidateReferences();
            }
        }

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("QUESTIONS", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        jsonFile = (TextAsset)EditorGUILayout.ObjectField(
            "JSON File (Optional)", jsonFile, typeof(TextAsset), false);
        if (EditorGUI.EndChangeCheck() && jsonFile != null)
        {
            jsonText = jsonFile.text;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(260));
        jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        using (new EditorGUI.DisabledScope(
            quizManager == null || string.IsNullOrWhiteSpace(jsonText)))
        {
            if (GUILayout.Button("3. IMPORT QUESTIONS + AUDIO", GUILayout.Height(36)))
            {
                ImportQuestions();
            }
        }

        EditorGUILayout.Space(6);

        if (GUILayout.Button("Insert Example Thomson JSON"))
        {
            jsonText = ExampleJson();
            GUI.FocusControl(null);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Recommended hierarchy names:\n" +
            "ModelModeManager\nQuizPanel\nGameOverPanel\nQuestionText\nInfoText\n" +
            "Option1 / Option2 / Option3 / Option4\nScoreText\n" +
            "Green / Red / Default\nQuizAudioSource\nWrongAnswer\n" +
            "AssessmentCompleteVO\nQuitButton\nPlayAgainButton",
            MessageType.None);
    }

    private void AutoAssignReferences()
    {
        if (quizManager == null)
            return;

        Undo.RecordObject(quizManager, "Auto Assign Quiz References");

        // ---------------- MODEL MODE MANAGER ----------------
        if (quizManager.modelModeManager == null)
        {
            quizManager.modelModeManager =
                FindObjectByTypeAnywhere<ModelModeManager>(
                    "ModelModeManager",
                    "Model Mode Manager");
        }

        // ---------------- PANELS ----------------
        if (quizManager.m_quizPanel == null)
            quizManager.m_quizPanel = FindGameObject(
                "QuizPanel",
                "Quiz Panel");

        if (quizManager.m_gameOverPanel == null)
            quizManager.m_gameOverPanel = FindGameObject(
                "GameOverPanel",
                "Game Over Panel");

        // ---------------- TEXT ----------------
        if (quizManager.questionText == null)
            quizManager.questionText = FindComponent<TMP_Text>(
                "QuestionText",
                "Question Text");

        if (quizManager.scoreText == null)
            quizManager.scoreText = FindComponent<TMP_Text>(
                "ScoreText",
                "Score Text");

        // ---------------- INFO TEXT ----------------
        // This reference belongs to this Editor tool rather than QuizManager,
        // so it does not require any change to your existing QuizManager.cs.
        if (infoText == null)
            infoText = FindComponent<TMP_Text>(
                "InfoText",
                "Info Text",
                "InformationText",
                "Information Text",
                "DescriptionText",
                "Description Text");

        // ---------------- OPTION BUTTONS ----------------
        if (quizManager.optionTexts == null || quizManager.optionTexts.Length != 4 ||
            HasMissing(quizManager.optionTexts))
        {
            Button[] options = FindOptionButtons();
            if (options.Length == 4)
                quizManager.optionTexts = options;
            else
                Debug.LogWarning(
                    $"Quiz Auto Setup: Found {options.Length} option buttons. Expected 4.");
        }

        // ---------------- SPRITES ----------------
        if (quizManager.m_green == null)
            quizManager.m_green = FindSprite(
                "Green",
                "Correct",
                "QuizGreen");

        if (quizManager.m_red == null)
            quizManager.m_red = FindSprite(
                "Red",
                "Wrong",
                "QuizRed");

        if (quizManager.m_default == null)
            quizManager.m_default = FindSprite(
                "Default",
                "QuizDefault",
                "Normal");

        // ---------------- AUDIO SOURCE ----------------
        if (quizManager.audioSource == null)
            quizManager.audioSource = FindComponent<AudioSource>(
                "QuizAudioSource",
                "Quiz Audio Source");

        // ---------------- AUDIO CLIPS ----------------
        if (quizManager.wrongAnswerVoice == null)
            quizManager.wrongAnswerVoice = FindAudioClip(
                "WrongAnswer",
                "Wrong Answer",
                "WrongAnswerVO");

        if (quizManager.assessmentCompleteVoice == null)
            quizManager.assessmentCompleteVoice = FindAudioClip(
                "AssessmentCompleteVO",
                "AssessmentComplete",
                "Assessment Complete");

        // ---------------- BUTTONS ----------------
        if (quizManager.m_quitButton == null)
            quizManager.m_quitButton = FindComponent<Button>(
                "QuitButton",
                "Quit Button");

        if (quizManager.m_playAgainButton == null)
            quizManager.m_playAgainButton = FindComponent<Button>(
                "PlayAgainButton",
                "Play Again Button",
                "AssessmentButton");

        EditorUtility.SetDirty(quizManager);

        if (quizManager.gameObject.scene.IsValid())
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                quizManager.gameObject.scene);

        Selection.activeGameObject = quizManager.gameObject;

        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (quizManager == null)
            return;

        List<string> missing = new List<string>();

        if (quizManager.modelModeManager == null) missing.Add("Model Mode Manager");
        if (quizManager.m_quizPanel == null) missing.Add("Quiz Panel");
        if (quizManager.m_gameOverPanel == null) missing.Add("Game Over Panel");
        if (quizManager.questionText == null) missing.Add("Question Text");

        if (quizManager.optionTexts == null || quizManager.optionTexts.Length != 4)
            missing.Add("Option Texts / Buttons (need exactly 4)");
        else
        {
            for (int i = 0; i < quizManager.optionTexts.Length; i++)
                if (quizManager.optionTexts[i] == null)
                    missing.Add($"Option Button {i + 1}");
        }

        if (quizManager.scoreText == null) missing.Add("Score Text");

        // Info Text is optional because some quiz scenes may not use an Info Panel.
        if (infoText == null)
            Debug.LogWarning("Quiz Auto Setup: Info Text is not assigned. JSON 'info' will not be placed anywhere.");

        if (quizManager.m_green == null) missing.Add("Green Sprite");
        if (quizManager.m_red == null) missing.Add("Red Sprite");
        if (quizManager.m_default == null) missing.Add("Default Sprite");
        if (quizManager.audioSource == null) missing.Add("Quiz Audio Source");
        if (quizManager.wrongAnswerVoice == null) missing.Add("Wrong Answer Voice");
        if (quizManager.assessmentCompleteVoice == null)
            missing.Add("Assessment Complete Voice");
        if (quizManager.m_quitButton == null) missing.Add("Quit Button");
        if (quizManager.m_playAgainButton == null) missing.Add("Play Again Button");

        if (missing.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Quiz Auto Setup",
                "All QuizManager references are assigned.",
                "Great");
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Quiz Auto Setup",
                "These references still need attention:\n\n• " +
                string.Join("\n• ", missing),
                "OK");
        }
    }

    private void ImportQuestions()
    {
        try
        {
            QuizImportData data = JsonUtility.FromJson<QuizImportData>(jsonText);

            if (data == null || data.questions == null || data.questions.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "Quiz Auto Setup",
                    "No questions found in JSON.",
                    "OK");
                return;
            }

            Undo.RecordObject(quizManager, "Import Quiz Questions");

            List<Question> imported = new List<Question>();

            for (int i = 0; i < data.questions.Length; i++)
            {
                QuizImportQuestion src = data.questions[i];

                if (string.IsNullOrWhiteSpace(src.questionText))
                    continue;

                if (src.options == null || src.options.Length != 4)
                {
                    Debug.LogWarning(
                        $"Quiz Auto Setup: Question {i + 1} skipped. Exactly 4 options are required.");
                    continue;
                }

                Question q = new Question
                {
                    questionText = src.questionText,
                    options = src.options,
                    correctAnswerIndex = Mathf.Clamp(src.correctAnswerIndex, 0, 3),
                    questionVoice = FindAudioClip(src.questionVoice),
                    correctAnswerVoice = FindAudioClip(src.correctAnswerVoice)
                };

                imported.Add(q);
            }

            quizManager.questions = imported.ToArray();

            // ---------------------------------------------------------
            // PROJECT / TOPIC INFO
            // ---------------------------------------------------------
            if (infoText != null)
            {
                Undo.RecordObject(infoText, "Import Quiz Info Text");
                infoText.text = data.info ?? "";
                EditorUtility.SetDirty(infoText);
            }
            else if (!string.IsNullOrWhiteSpace(data.info))
            {
                Debug.LogWarning(
                    "Quiz Auto Setup: JSON contains 'info', but no Info Text TMP reference is assigned.");
            }

            EditorUtility.SetDirty(quizManager);

            if (quizManager.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    quizManager.gameObject.scene);

            EditorUtility.DisplayDialog(
                "Quiz Auto Setup",
                $"Imported {imported.Count} questions." +
                (infoText != null ? "\nInfo text was also updated." : ""),
                "Done");
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            EditorUtility.DisplayDialog(
                "Quiz Auto Setup",
                "JSON import failed. Check Console for details.",
                "OK");
        }
    }

    // =========================================================
    // FINDERS
    // =========================================================

    private static T FindObjectByTypeAnywhere<T>(params string[] preferredNames)
        where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();

        foreach (string wanted in preferredNames)
        {
            foreach (T item in all)
            {
                if (!IsSceneObject(item.gameObject))
                    continue;

                if (NameMatches(item.gameObject.name, wanted))
                    return item;
            }
        }

        foreach (T item in all)
        {
            if (IsSceneObject(item.gameObject))
                return item;
        }

        return null;
    }

    private static GameObject FindGameObject(params string[] names)
    {
        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (string wanted in names)
        {
            foreach (Transform t in all)
            {
                if (!IsSceneObject(t.gameObject))
                    continue;

                if (NameMatches(t.name, wanted))
                    return t.gameObject;
            }
        }

        return null;
    }

    private static T FindComponent<T>(params string[] names) where T : Component
    {
        T[] all = Resources.FindObjectsOfTypeAll<T>();

        foreach (string wanted in names)
        {
            foreach (T c in all)
            {
                if (!IsSceneObject(c.gameObject))
                    continue;

                if (NameMatches(c.gameObject.name, wanted))
                    return c;
            }
        }

        return null;
    }

    private static Button[] FindOptionButtons()
    {
        Button[] all = Resources.FindObjectsOfTypeAll<Button>();
        List<Button> found = new List<Button>();

        string[] exactNames =
        {
            "Option1", "Option2", "Option3", "Option4",
            "Option 1", "Option 2", "Option 3", "Option 4",
            "OptionButton1", "OptionButton2", "OptionButton3", "OptionButton4"
        };

        foreach (string wanted in exactNames)
        {
            foreach (Button b in all)
            {
                if (!IsSceneObject(b.gameObject))
                    continue;

                if (NameMatches(b.gameObject.name, wanted) && !found.Contains(b))
                {
                    found.Add(b);
                    break;
                }
            }

            if (found.Count == 4)
                return found.ToArray();
        }

        // Fallback: collect buttons whose names contain "option"
        found.Clear();

        foreach (Button b in all)
        {
            if (!IsSceneObject(b.gameObject))
                continue;

            string n = Normalize(b.gameObject.name);

            if (n.Contains("option") &&
                !n.Contains("quit") &&
                !n.Contains("playagain"))
            {
                found.Add(b);
            }
        }

        found.Sort((a, b) =>
            string.Compare(a.gameObject.name, b.gameObject.name,
                StringComparison.OrdinalIgnoreCase));

        if (found.Count > 4)
            found.RemoveRange(4, found.Count - 4);

        return found.ToArray();
    }

    private static Sprite FindSprite(params string[] names)
    {
        foreach (string wanted in names)
        {
            string[] guids = AssetDatabase.FindAssets($"{wanted} t:Sprite");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                if (s != null && NameMatches(s.name, wanted))
                    return s;
            }
        }

        return null;
    }

    private static AudioClip FindAudioClip(params string[] names)
    {
        foreach (string wantedRaw in names)
        {
            if (string.IsNullOrWhiteSpace(wantedRaw))
                continue;

            string wanted = Path.GetFileNameWithoutExtension(wantedRaw.Trim());
            string[] guids = AssetDatabase.FindAssets($"{wanted} t:AudioClip");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

                if (clip != null &&
                    NameMatches(Path.GetFileNameWithoutExtension(path), wanted))
                    return clip;
            }

            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<AudioClip>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        return null;
    }

    private static bool HasMissing(Button[] array)
    {
        if (array == null)
            return true;

        foreach (Button b in array)
            if (b == null)
                return true;

        return false;
    }

    private static bool IsSceneObject(GameObject go)
    {
        return go != null &&
               go.scene.IsValid() &&
               !EditorUtility.IsPersistent(go);
    }

    private static bool NameMatches(string a, string b)
    {
        return Normalize(a) == Normalize(b);
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        return value
            .Replace(" ", "")
            .Replace("_", "")
            .Replace("-", "")
            .ToLowerInvariant();
    }

    private static string ExampleJson()
    {
        return @"{
  ""info"": ""The Thomson atomic model describes the atom as a sphere of positive charge with negatively charged electrons embedded throughout it. It was an important early model that showed atoms contain smaller charged particles."",
  ""questions"": [
    {
      ""questionText"": ""Which statement best describes Thomson's atomic model?"",
      ""options"": [
        ""Electrons are embedded in a positively charged sphere"",
        ""Electrons move in fixed shells around a nucleus"",
        ""The atom contains only positive charge"",
        ""All positive charge is concentrated in a nucleus""
      ],
      ""correctAnswerIndex"": 0,
      ""questionVoice"": ""Q1_Thomson"",
      ""correctAnswerVoice"": ""Q1_Correct""
    },
    {
      ""questionText"": ""What charge does an electron carry?"",
      ""options"": [
        ""Negative charge"",
        ""Positive charge"",
        ""No charge"",
        ""Both positive and negative charge""
      ],
      ""correctAnswerIndex"": 0,
      ""questionVoice"": ""Q2_ElectronCharge"",
      ""correctAnswerVoice"": ""Q2_Correct""
    }
  ]
}";
    }
}
#endif
