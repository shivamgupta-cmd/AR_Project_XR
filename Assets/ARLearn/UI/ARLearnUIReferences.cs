using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ARLearn.UI
{
    /// <summary>Inspector-visible references for every interactive AR Learn UI control.</summary>
    public sealed class ARLearnUIReferences : MonoBehaviour
    {
        [Header("Localization References")]
        public LanguageManager languageManager;
        public LanguageUI languageUI;

        [Header("All Clickable Controls")]
        public Button[] allButtons;

        [Header("Language Controls")]
        public Button languageDropdown;
        public Button[] languageButtons;

        [Header("Navigation")]
        public Button[] headerButtons;
        public Button[] bottomNavigationButtons;

        [Header("Model Controls")]
        public Button[] modelControlButtons;
        public Button[] sideToolButtons;
        public Button[] exploreMenuButtons;

        [Header("Content Controls")]
        public Button[] subjectButtons;
        public Button[] quizButtons;
        public Button[] settingsButtons;

        public void Refresh(Transform uiRoot)
        {
            if (languageManager == null) languageManager = FindObjectOfType<LanguageManager>();
            if (languageUI == null) languageUI = FindObjectOfType<LanguageUI>();


            allButtons = uiRoot.GetComponentsInChildren<Button>(true);

            languageDropdown = FindOne("LanguageDropdown");
            languageButtons = FindPrefix("Lang_");
            headerButtons = FindPrefix("Header");
            bottomNavigationButtons = FindPrefix("Nav_");
            modelControlButtons = FindContains("Button_", "Move", "Rotate", "Zoom", "Explore");
            sideToolButtons = FindPrefix("Tool_");
            exploreMenuButtons = FindExact("X-Ray", "Info", "Parts", "Quiz");
            subjectButtons = FindPrefix("Subject_", "Recent_");
            quizButtons = FindPrefix("Answer_", "Button_Next");
            settingsButtons = FindPrefix("Setting_");
        }

        private Button FindOne(string objectName)
        {
            foreach (var button in allButtons) if (button.name == objectName) return button;
            return null;
        }

        private Button[] FindPrefix(params string[] prefixes)
        {
            var result = new List<Button>();
            foreach (var button in allButtons)
                foreach (var prefix in prefixes)
                    if (button.name.StartsWith(prefix)) { result.Add(button); break; }
            return result.ToArray();
        }

        private Button[] FindExact(params string[] names)
        {
            var result = new List<Button>();
            foreach (var button in allButtons)
                foreach (var name in names)
                    if (button.name == name) { result.Add(button); break; }
            return result.ToArray();
        }

        private Button[] FindContains(string prefix, params string[] fragments)
        {
            var result = new List<Button>();
            foreach (var button in allButtons)
            {
                if (!button.name.StartsWith(prefix)) continue;
                foreach (var fragment in fragments)
                    if (button.name.Contains(fragment)) { result.Add(button); break; }
            }
            return result.ToArray();
        }
    }
}
