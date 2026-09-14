using ARLearn.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

namespace ARLearn.Editor
{
    public static class ARLearnSceneGenerator
    {
        [MenuItem("AR Learn/Create Reference UI Scene")]
        public static void Generate()
        {
            const string folder = "Assets/Scenes";
            const string path = folder + "/ARLearn_UI_Showcase.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ARLearn_UI_Showcase";
            var root = new GameObject("AR Learn UI Showcase");
            var showcase = root.AddComponent<ARLearnShowcase>();
            showcase.Build();
            EditorSceneManager.SaveScene(scene, path);
            AddToBuildSettings(path);
            Selection.activeGameObject = root;
            Debug.Log("AR Learn showcase scene generated: " + path);
        }

        public static void GenerateBatch() { Generate(); EditorApplication.Exit(0); }

        public static void TestLocalizationBatch()
        {
            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var root = new GameObject("Localization Runtime Test");
                var showcase = root.AddComponent<ARLearnShowcase>();
                showcase.Build();
                
                var lm = LanguageManager.Instance ?? GameObject.FindObjectOfType<LanguageManager>();
                if (lm == null) throw new Exception("LanguageManager Instance is missing!");

                LanguageManager.Language[] languages = {
                    LanguageManager.Language.English,
                    LanguageManager.Language.Hindi,
                    LanguageManager.Language.Gujarati
                };
                
                string[] expectedHeartText = { "Human Heart", "ekuo ân;", "માનવ હૃદય" };
                var refs = showcase.References;

                if (refs.languageButtons == null || refs.languageButtons.Length != 3)
                    throw new Exception("Expected 3 language buttons, found: " + (refs.languageButtons?.Length ?? 0));

                for (int i = 0; i < languages.Length; i++)
                {
                    refs.languageButtons[i].onClick.Invoke();
                    if (lm.currentLanguage != languages[i])
                        throw new Exception("Language state failed: " + languages[i]);

                    string translated = lm.GetText("HUMAN_HEART");
                    if (translated != expectedHeartText[i])
                        throw new Exception($"GetText(HUMAN_HEART) failed for {languages[i]}: got '{translated}', expected '{expectedHeartText[i]}'");

                    bool foundInUi = false;
                    foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                    {
                        if (text.text.Contains(expectedHeartText[i]))
                        {
                            foundInUi = true;
                            break;
                        }
                    }

                    if (!foundInUi)
                        throw new Exception("Localized UI text missing for " + languages[i] + " (" + expectedHeartText[i] + ")");

                    Debug.Log("[ARLearn Test] PASS language=" + languages[i] + " -> " + translated);
                }

                if (refs.allButtons == null || refs.allButtons.Length < 30)
                    throw new Exception("Button reference test failed.");

                Debug.Log("[ARLearn Test] PASS buttons=" + refs.allButtons.Length + ", languages=3");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void AddToBuildSettings(string path)
        {
            var old = EditorBuildSettings.scenes;
            foreach (var scene in old) if (scene.path == path) return;
            var next = new EditorBuildSettingsScene[old.Length + 1];
            next[0] = new EditorBuildSettingsScene(path, true);
            for (int i = 0; i < old.Length; i++) next[i + 1] = old[i];
            EditorBuildSettings.scenes = next;
        }
    }
}
