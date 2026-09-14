using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARLearn.UI
{
    [ExecuteAlways]
    public sealed class ARLearnShowcase : MonoBehaviour
    {
        private const int CurrentUiVersion = 10;
        private static readonly Vector2 ReferenceResolution = new Vector2(1080, 1920);
        private const float PhoneScaleAt1080x1920 = 2.18f;
        [SerializeField] private int selectedScreen;
        [SerializeField, HideInInspector] private int builtUiVersion;
        private readonly List<GameObject> screens = new List<GameObject>();
        private readonly List<Button> tabs = new List<Button>();
        private int quizQuestion = 2;
        private readonly string[] quizQuestions = {
            "Which chamber receives oxygen-rich blood from the lungs?",
            "Which part of the heart pumps oxygen-rich blood to the body?",
            "Which vessel carries blood away from the heart?",
            "How many chambers does the human heart have?",
            "Which side of the heart pumps blood to the lungs?"
        };
        private readonly string[,] quizAnswers = {
            { "A) Right Atrium", "B) Left Atrium", "C) Right Ventricle", "D) Aorta" },
            { "A) Right Atrium", "B) Right Ventricle", "C) Left Ventricle", "D) Left Atrium" },
            { "A) Vein", "B) Artery", "C) Capillary", "D) Atrium" },
            { "A) Two", "B) Three", "C) Four", "D) Five" },
            { "A) Left side", "B) Right side", "C) Both sides", "D) Neither side" }
        };
        private readonly int[] correctQuizAnswers = { 1, 2, 1, 2, 1 };
        private bool audioEnabled = true;
        private bool musicEnabled = true;
        private GameObject languageMenu;
        [SerializeField] private ARLearnUIReferences uiReferences;
        [SerializeField] private LanguageManager languageManager;
        [SerializeField] private LanguageUI languageUI;

        private static readonly Color Ink = new Color32(18, 18, 25, 255);
        private static readonly Color Purple = new Color32(91, 45, 155, 255);
        private static readonly Color Violet = new Color32(126, 63, 207, 255);
        private static readonly Color Card = new Color32(27, 29, 36, 245);
        private static readonly Color Soft = new Color32(244, 244, 248, 255);

        private void Awake()
        {
            EnsureLocalizationServices();
            if (transform.childCount == 0)
            {
                Build();
            }
            else
            {
                EnsureAllTextsLocalized();
            }
            WireButtons();
            WireScreenActions();
            Show(selectedScreen);
        }

        private void Start()
        {
            EnsureLocalizationServices();
            EnsureAllTextsLocalized();
            WireButtons();
            WireScreenActions();
        }

        private void OnEnable()
        {
            EnsureLocalizationServices();
            EnsureAllTextsLocalized();
            WireButtons();
            WireScreenActions();
        }

        public void EnsureLocalizationServices()
        {
            if (languageManager == null) languageManager = LanguageManager.Instance;
            if (languageManager == null) languageManager = FindObjectOfType<LanguageManager>();
            if (languageManager == null)
            {
                var lmObj = GameObject.Find("LanguageManager");
                if (lmObj == null) lmObj = new GameObject("LanguageManager");
                languageManager = lmObj.GetComponent<LanguageManager>() ?? lmObj.AddComponent<LanguageManager>();
            }
            LanguageManager.Instance = languageManager;

            if (languageUI == null) languageUI = FindObjectOfType<LanguageUI>();
            if (languageUI == null && languageManager != null)
            {
                languageUI = languageManager.GetComponent<LanguageUI>() ?? languageManager.gameObject.AddComponent<LanguageUI>();
            }

            if (languageManager != null)
            {
                languageManager.InitFonts();
                if (languageManager.languageData.Count == 0)
                    languageManager.LoadCSV();
            }
        }

        [ContextMenu("Ensure Localized Text Components")]
        public void EnsureAllTextsLocalized()
        {
            EnsureLocalizationServices();
            foreach (var t in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                var loc = t.GetComponent<LocalizedText>();
                if (loc == null)
                {
                    loc = t.gameObject.AddComponent<LocalizedText>();
                    loc.key = GuessLocalizationKey(t);
                }
                else if (string.IsNullOrEmpty(loc.key))
                {
                    loc.key = GuessLocalizationKey(t);
                }
                loc.RegisterAndRefresh();
            }
        }

        private string GuessLocalizationKey(TextMeshProUGUI t)
        {
            var p = t.transform.parent != null ? t.transform.parent.gameObject.name : "";
            var n = t.gameObject.name;
            var text = t.text ?? "";

            if (p == "LanguageDropdown" || n == "LanguageDropdown") return "LANGUAGE";
            if (p == "Lang_0" || n == "Lang_0") return "ENGLISH";
            if (p == "Lang_1" || n == "Lang_1") return "HINDI";
            if (p == "Lang_2" || n == "Lang_2") return "GUJARATI";

            if (p.StartsWith("Subject_"))
            {
                int idx = ParseSuffix(p);
                string[] subKeys = { "SUB_ANATOMY", "SUB_BIOLOGY", "SUB_CHEMISTRY", "SUB_PHYSICS", "SUB_HISTORY", "SUB_CIVICS", "SUB_GEOGRAPHY", "SUB_EVS" };
                if (idx >= 0 && idx < subKeys.Length) return subKeys[idx];
            }
            if (p.StartsWith("Recent_"))
            {
                int idx = ParseSuffix(p);
                string[] recKeys = { "RECENT_HEART", "RECENT_SOLAR", "RECENT_TAJ" };
                if (idx >= 0 && idx < recKeys.Length) return recKeys[idx];
            }
            if (p.StartsWith("Tool_"))
            {
                int idx = ParseSuffix(p);
                string[] toolKeys = { "TOOL_3D_VIEW", "TOOL_AUDIO", "TOOL_RESET" };
                if (idx >= 0 && idx < toolKeys.Length) return toolKeys[idx];
            }
            if (p.StartsWith("Answer_"))
            {
                int idx = ParseSuffix(p);
                string[] ansKeys = { "QUIZ_A1", "QUIZ_A2", "QUIZ_A3", "QUIZ_A4" };
                if (idx >= 0 && idx < ansKeys.Length) return ansKeys[idx];
            }
            if (p.StartsWith("LibraryTab_"))
            {
                int idx = ParseSuffix(p);
                string[] tabKeys = { "TAB_MODELS", "TAB_BOOKMARKS", "TAB_HISTORY" };
                if (idx >= 0 && idx < tabKeys.Length) return tabKeys[idx];
            }
            if (p.StartsWith("LibraryRow_"))
            {
                int idx = ParseSuffix(p);
                string[] rowKeys = { "LIB_HEART", "LIB_PLANT", "LIB_SOLAR", "LIB_TAJ" };
                if (idx >= 0 && idx < rowKeys.Length) return rowKeys[idx];
            }
            if (p.StartsWith("Setting_"))
            {
                int idx = ParseSuffix(p);
                string[] setKeys = { "SETTING_LANG", "SETTING_AUDIO", "SETTING_BGM", "SETTING_CACHE", "SETTING_RATE", "SETTING_SHARE", "SETTING_PRIVACY", "SETTING_ABOUT" };
                if (idx >= 0 && idx < setKeys.Length) return setKeys[idx];
            }
            if (p.StartsWith("Nav_"))
            {
                int idx = ParseSuffix(p);
                string[] navKeys = { "NAV_HOME", "NAV_LIBRARY", "NAV_BOOKMARKS", "NAV_PROFILE" };
                if (idx >= 0 && idx < navKeys.Length) return navKeys[idx];
            }
            if (p.StartsWith("Tab_"))
            {
                int idx = ParseSuffix(p);
                string[] tabsK = { "TAB_HOME", "TAB_SCAN", "TAB_MODEL", "TAB_EXPLORE", "TAB_XRAY", "TAB_PARTS", "TAB_INFO", "TAB_QUIZ", "TAB_AUDIO", "TAB_RESET", "TAB_LIBRARY", "TAB_SETTINGS" };
                if (idx >= 0 && idx < tabsK.Length) return tabsK[idx];
            }

            if (p == "Aorta") return "PART_AORTA";
            if (p == "Pulmonary Artery") return "PART_PULMONARY";
            if (p == "Left Atrium") return "PART_LEFT_ATRIUM";
            if (p == "Left Ventricle") return "PART_LEFT_VENTRICLE";
            if (p == "ScanHint") return "SCAN_HINT";
            if (p == "ResetToast") return "RESET_TOAST";
            if (p == "AudioPlayer") return "AUDIO_SUBTITLE";
            if (p.Contains("Move")) return "CTRL_MOVE";
            if (p.Contains("Rotate")) return "CTRL_ROTATE";
            if (p.Contains("Zoom")) return "CTRL_ZOOM";
            if (p.Contains("Explore")) return "CTRL_EXPLORE";
            if (p.Contains("Next")) return "BTN_NEXT";

            if (text.Contains("AR LEARN")) return "HEADER_TITLE";
            if (text.Contains("Learn • Explore")) return "HEADER_SUBTITLE";
            if (text.Contains("Recently Viewed")) return "RECENTLY_VIEWED";
            if (text.Contains("Human Heart") || text.Contains("HUMAN HEART")) return "HUMAN_HEART";
            if (text.Contains("About Human Heart")) return "INFO_TITLE";
            if (text.Contains("muscular organ")) return "INFO_DESC";
            if (text.Contains("Question 2 of 5")) return "QUIZ_Q_PROGRESS";
            if (text.Contains("Which part of the heart")) return "QUIZ_Q2";
            if (text.Contains("Correct!")) return "CORRECT";
            if (text.Contains("oxygen-rich blood")) return "QUIZ_EXPLANATION";
            if (text.Contains("My Library")) return "HEADER_LIBRARY";
            if (text.Contains("Settings")) return "BTN_SETTINGS";

            return string.Empty;
        }

        [ContextMenu("Rebuild AR Learn UI")]
        public void Build()
        {
            builtUiVersion = CurrentUiVersion;
            EnsureLocalizationServices();

            while (transform.childCount > 0)
            {
                var child = transform.GetChild(0).gameObject;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }

            EnsureEventSystem();
            var canvas = CreateCanvas();
            var backdrop = Box("Backdrop", canvas.transform, new Color32(5, 7, 13, 255));
            Stretch(backdrop.GetComponent<RectTransform>());
            var phone = Box("Phone", backdrop.transform, new Color32(10, 10, 13, 255));
            Set(phone, .5f, .5f, 430, 850, 0, -10);
            phone.transform.localScale = Vector3.one * PhoneScaleAt1080x1920;
            Outline(phone, new Color32(80, 82, 92, 255), 3);

            var viewport = Box("ScreenViewport", phone.transform, Color.white);
            Set(viewport, .5f, .5f, 410, 780, 0, 8);
            var mask = viewport.AddComponent<RectMask2D>();
            mask.padding = new Vector4(0, 0, 0, 0);

            screens.Clear();
            screens.Add(BuildHome(viewport.transform));
            screens.Add(BuildScan(viewport.transform));
            screens.Add(BuildModel(viewport.transform, "MODEL", false, false, false, false));
            screens.Add(BuildModel(viewport.transform, "EXPLORE", true, false, false, false));
            screens.Add(BuildModel(viewport.transform, "X-RAY", true, true, false, false));
            screens.Add(BuildModel(viewport.transform, "PARTS", true, false, true, false));
            screens.Add(BuildModel(viewport.transform, "INFO", true, false, false, true));
            screens.Add(BuildQuiz(viewport.transform));
            screens.Add(BuildAudio(viewport.transform));
            screens.Add(BuildReset(viewport.transform));
            screens.Add(BuildLibrary(viewport.transform));
            screens.Add(BuildSettings(viewport.transform));

            var selector = Box("ScreenSelector", backdrop.transform, new Color32(15, 16, 23, 245));
            Set(selector, .5f, 0, 760, 54, 0, 20);
            var layout = selector.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 7, 7); layout.spacing = 5;
            layout.childControlWidth = true; layout.childForceExpandWidth = true;
            tabs.Clear();
            string[] labels = { "Home", "Scan", "Model", "Explore", "X-Ray", "Parts", "Info", "Quiz", "Audio", "Reset", "Library", "Settings" };
            string[] tabKeys = { "TAB_HOME", "TAB_SCAN", "TAB_MODEL", "TAB_EXPLORE", "TAB_XRAY", "TAB_PARTS", "TAB_INFO", "TAB_QUIZ", "TAB_AUDIO", "TAB_RESET", "TAB_LIBRARY", "TAB_SETTINGS" };
            for (int i = 0; i < labels.Length; i++)
            {
                var tab = Button(labels[i], tabKeys[i], selector.transform, Purple, 12);
                tab.name = "Tab_" + i;
                tabs.Add(tab);
            }
            WireButtons();
            WireScreenActions();
            Show(Mathf.Clamp(selectedScreen, 0, screens.Count - 1));
            CacheInspectorReferences();
        }

        private void CacheInspectorReferences()
        {
            if (uiReferences == null) uiReferences = GetComponent<ARLearnUIReferences>();
            if (uiReferences == null) uiReferences = gameObject.AddComponent<ARLearnUIReferences>();
            uiReferences.languageManager = languageManager;
            uiReferences.languageUI = languageUI;
            uiReferences.Refresh(transform);
        }

        private void BuildLanguageDropdown(Transform parent)
        {
            var open = Button("🌐  Language ▾", "LANGUAGE", parent, Purple, 15); open.name = "LanguageDropdown";
            Set(open.gameObject, 1, 1, 145, 38, -10, -68);
            languageMenu = Box("LanguageMenu", parent, new Color32(24, 25, 34, 252));
            Set(languageMenu, 1, 1, 165, 160, -10, -110); Outline(languageMenu, Violet, 2);
            string[] languages = { "English", "Hindi", "Gujarati" };
            string[] langKeys = { "ENGLISH", "HINDI", "GUJARATI" };
            for (int i = 0; i < languages.Length; i++)
            {
                var choice = Button(languages[i], langKeys[i], languageMenu.transform, i == 0 ? Violet : new Color32(52, 53, 64, 255), 15);
                choice.name = "Lang_" + i; Set(choice.gameObject, .5f, 1, 145, 38, 0, -8 - i * 46);
            }
            languageMenu.SetActive(false);
        }

        private Canvas CreateCanvas()
        {
            var go = new GameObject("AR Learn Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            scaler.referencePixelsPerUnit = 100;
            return canvas;
        }

        private GameObject BuildHome(Transform parent)
        {
            var root = Screen("01_Home_Subjects", parent, Soft);
            Header(root.transform, "☰", "AR LEARN", "HEADER_TITLE", "⚙", false);
            Label("Learn • Explore • Understand", "HEADER_SUBTITLE", root.transform, 15, Ink, TextAlignmentOptions.Center, 0, 642, 390, 28);
            string[] subjects = { "⚕  ANATOMY\nHuman Body", "⚛  BIOLOGY\nBeautiful", "⚗  CHEMISTRY\nExperiment", "✹  PHYSICS\nWonders", "▥  HISTORY\nCivilization", "♧  CIVICS\nCommunity", "◎  GEOGRAPHY\nOur World", "◒  EVS\nNature" };
            string[] subKeys = { "SUB_ANATOMY", "SUB_BIOLOGY", "SUB_CHEMISTRY", "SUB_PHYSICS", "SUB_HISTORY", "SUB_CIVICS", "SUB_GEOGRAPHY", "SUB_EVS" };
            Color[] colors = { new Color32(17,98,184,255), new Color32(38,133,35,255), new Color32(119,68,182,255), new Color32(222,112,0,255), new Color32(159,105,40,255), new Color32(13,119,129,255), new Color32(21,104,184,255), new Color32(37,126,31,255) };
            for (int i = 0; i < 8; i++)
            {
                var tile = Box("Subject_" + i, root.transform, colors[i]); tile.AddComponent<Button>();
                Set(tile, 0, 1, 178, 86, 19 + (i % 2) * 193, -130 - (i / 2) * 96);
                TextInto(tile, subjects[i], subKeys[i], 16, Color.white, TextAlignmentOptions.Center);
            }
            Label("Recently Viewed", "RECENTLY_VIEWED", root.transform, 16, Ink, TextAlignmentOptions.Left, 20, 255, 370, 28);
            string[] recent = { "♥\nHuman Heart", "☀\nThe Solar System", "♜\nTaj Mahal" };
            string[] recentKeys = { "RECENT_HEART", "RECENT_SOLAR", "RECENT_TAJ" };
            for (int i = 0; i < 3; i++) { var c = Box("Recent_" + i, root.transform, Color.white); c.AddComponent<Button>(); Set(c, 0, 1, 116, 95, 19 + i * 126, -563); TextInto(c, recent[i], recentKeys[i], 14, Ink, TextAlignmentOptions.Center); Outline(c, new Color32(210,210,216,255), 1); }
            BottomNav(root.transform, 0);
            BuildLanguageDropdown(root.transform);
            return root;
        }

        private GameObject BuildScan(Transform parent)
        {
            var root = Screen("02_Scan_Page", parent, new Color32(25, 19, 15, 255)); Header(root.transform, "‹", "Human Heart", "HUMAN_HEART", "⌂", true);
            Label("THE HUMAN HEART", "TITLE_HUMAN_HEART", root.transform, 22, new Color32(109,43,32,255), TextAlignmentOptions.Center, 55, 515, 300, 45);
            var page = Box("BookPage", root.transform, new Color32(200, 174, 135, 255)); Set(page, .5f, .5f, 290, 470, 15, -10); Outline(page, new Color32(104,70,46,255), 5);
            Label("♥", null, page.transform, 125, new Color32(136,37,29,255), TextAlignmentOptions.Center, 0, 0, 220, 230);
            var scan = Box("ScanGlow", root.transform, new Color32(142, 53, 214, 95)); Set(scan, .5f, .5f, 340, 4, 0, 0);
            var hint = Box("ScanHint", root.transform, new Color32(28,29,35,245)); Set(hint, .5f, 0, 285, 74, 0, 86); TextInto(hint, "Align the page inside\nthe frame to scan.", "SCAN_HINT", 17, Color.white, TextAlignmentOptions.Center); Outline(hint, Violet, 2);
            SideTools(root.transform); Controls(root.transform); return root;
        }

        private GameObject BuildModel(Transform parent, string name, bool menu, bool xray, bool parts, bool info)
        {
            var root = Screen("0_Model_" + name, parent, new Color32(22, 17, 13, 255)); Header(root.transform, "‹", "Human Heart", "HUMAN_HEART", "⌂", true); SideTools(root.transform);
            CreateHeart(root.transform, xray);
            if (parts)
            {
                string[] tags = { "Aorta", "Pulmonary Artery", "Left Atrium", "Left Ventricle" };
                string[] tagKeys = { "PART_AORTA", "PART_PULMONARY", "PART_LEFT_ATRIUM", "PART_LEFT_VENTRICLE" };
                for (int i = 0; i < tags.Length; i++) { var t = Box(tags[i], root.transform, i == 0 ? new Color32(170,48,52,255) : new Color32(74,61,159,255)); Set(t, 1, .5f, 118, 30, -12, 175 - i * 55); TextInto(t, tags[i], tagKeys[i], 11, Color.white, TextAlignmentOptions.Center); }
            }
            if (info)
            {
                var p = Box("InformationCard", root.transform, Card); Set(p, 1, .5f, 270, 330, -16, 45); Outline(p, new Color32(78,81,91,255), 2);
                Label("About Human Heart", "INFO_TITLE", p.transform, 20, Color.white, TextAlignmentOptions.Left, 18, 270, 230, 35);
                Label("The heart is a muscular organ that pumps blood throughout the body.\n\nIt has 4 chambers — 2 atria and 2 ventricles. It works continuously to supply oxygen and nutrients.", "INFO_DESC", p.transform, 15, Color.white, TextAlignmentOptions.TopLeft, 18, 52, 230, 205);
            }
            if (menu) ExploreMenu(root.transform, name);
            Controls(root.transform); return root;
        }

        private GameObject BuildQuiz(Transform parent)
        {
            var root = Screen("08_Quiz", parent, new Color32(22,17,13,255)); Header(root.transform, "‹", "Human Heart", "HUMAN_HEART", "⌂", true); SideTools(root.transform);
            var card = Box("QuizCard", root.transform, Card); Set(card, .5f, .5f, 350, 475, 16, 45); Outline(card, Violet, 2);
            Label("Question 2 of 5", "QUIZ_Q_PROGRESS", card.transform, 13, new Color32(205,205,215,255), TextAlignmentOptions.Left, 18, 420, 300, 26);
            Label("Which part of the heart\npumps oxygen-rich\nblood to the body?", "QUIZ_Q2", card.transform, 19, Color.white, TextAlignmentOptions.TopLeft, 18, 305, 300, 100);
            string[] answers = { "A) Right Atrium", "B) Right Ventricle", "C) Left Ventricle     ✓", "D) Left Atrium" };
            string[] answerKeys = { "QUIZ_A1", "QUIZ_A2", "QUIZ_A3", "QUIZ_A4" };
            for (int i=0;i<4;i++){ var a=Box("Answer_"+i,card.transform,i==2?new Color32(25,105,38,255):new Color32(42,44,51,255)); a.AddComponent<Button>(); Set(a,.5f,1,310,42,0,-220-i*52); TextInto(a,answers[i],answerKeys[i],15,Color.white,TextAlignmentOptions.Left); }
            var result=Box("Correct",root.transform,new Color32(25,27,33,250)); Set(result,.5f,0,350,130,16,108); Label("Correct!", "CORRECT", result.transform,18,new Color32(198,236,48,255),TextAlignmentOptions.Left,16,85,310,28); Label("Left Ventricle pumps oxygen-rich blood.", "QUIZ_EXPLANATION", result.transform,13,new Color32(198,236,48,255),TextAlignmentOptions.Left,16,51,310,30); var next=Button("Next", "BTN_NEXT", result.transform,Violet,16); Set(next.gameObject,.5f,0,310,38,0,9); Controls(root.transform); return root;
        }

        private GameObject BuildAudio(Transform parent)
        {
            var root=BuildModel(parent,"AUDIO",false,false,false,false); root.name="09_Audio"; var p=Box("AudioPlayer",root.transform,new Color32(39,29,54,245)); p.AddComponent<Button>(); ConfigureButton(p.GetComponent<Button>()); Set(p,.5f,0,350,125,0,112); Label("▶  The heart pumps blood\n     throughout the body...", "AUDIO_SUBTITLE", p.transform,15,Color.white,TextAlignmentOptions.Left,18,67,315,48); Label("◀◀      ━━━━━●━━      ▶▶", null, p.transform,15,Color.white,TextAlignmentOptions.Center,12,14,325,30); return root;
        }

        private GameObject BuildReset(Transform parent){ var r=BuildModel(parent,"RESET",false,false,false,false); r.name="10_Reset_Default"; var toast=Box("ResetToast",r.transform,new Color32(78,39,122,245)); Set(toast,.5f,.5f,250,50,0,-180); TextInto(toast,"↻  Model reset to default", "RESET_TOAST", 14,Color.white,TextAlignmentOptions.Center); return r; }

        private GameObject BuildLibrary(Transform parent)
        {
            var root=Screen("11_My_Library",parent,Soft); Header(root.transform,"‹","My Library","HEADER_LIBRARY","⌂",false); var tabsBar=Box("LibraryTabs",root.transform,Color.white); Set(tabsBar,.5f,1,370,42,0,-78); string[] libraryTabs={"Models","Bookmarks","History"}; string[] libraryTabKeys={"TAB_MODELS","TAB_BOOKMARKS","TAB_HISTORY"}; for(int i=0;i<3;i++){var tab=Button(libraryTabs[i],libraryTabKeys[i],tabsBar.transform,i==0?Purple:new Color32(225,225,232,255),13);tab.name="LibraryTab_"+i;if(i>0)tab.GetComponentInChildren<TextMeshProUGUI>().color=Ink;Set(tab.gameObject,0,0,116,34,5+i*123,4);}
            string[] rows={"♥   Human Heart\n      Biology  •  12 May 2024","◉   Plant Cell\n      Biology  •  10 May 2024","☀   The Solar System\n      Physics  •  08 May 2024","♜   Taj Mahal\n      History  •  05 May 2024"};
            string[] rowKeys={"LIB_HEART","LIB_PLANT","LIB_SOLAR","LIB_TAJ"};
            for(int i=0;i<4;i++){var row=Box("LibraryRow_"+i,root.transform,Color.white); row.AddComponent<Button>(); Set(row,.5f,1,370,92,0,-135-i*102); TextInto(row,rows[i]+"                 ⋮",rowKeys[i],15,Ink,TextAlignmentOptions.Left); Outline(row,new Color32(225,225,230,255),1);} return root;
        }

        private GameObject BuildSettings(Transform parent)
        {
            var root=Screen("12_Settings",parent,Soft); Label("Settings", "BTN_SETTINGS", root.transform,25,Ink,TextAlignmentOptions.Center,0,708,410,42);
            string[] rows={"◎  Language                         English  ›","🔊  Audio                                  ●  ›","♫  Background Music               ●  ›","▣  Clear Cache                     45.2 MB  ›","☆  Rate Us                                      ›","⌯  Share App                                  ›","▢  Privacy Policy                            ›","ⓘ  About Us                                    ›"};
            string[] rowKeys={"SETTING_LANG","SETTING_AUDIO","SETTING_BGM","SETTING_CACHE","SETTING_RATE","SETTING_SHARE","SETTING_PRIVACY","SETTING_ABOUT"};
            for(int i=0;i<rows.Length;i++){var row=Box("Setting_"+i,root.transform,Color.white); row.AddComponent<Button>(); Set(row,.5f,1,370,54,0,-95-i*60); TextInto(row,rows[i],rowKeys[i],14,Ink,TextAlignmentOptions.Left);} return root;
        }

        private void Header(Transform p,string left,string title,string titleKey,string right,bool dark)
        {
            Color bg=dark?new Color32(245,245,243,255):Color.clear; var h=Box("Header",p,bg); Set(h,.5f,1,390,55,0,-12); var l=Label(left,null,h.transform,31,Ink,TextAlignmentOptions.Center,8,7,45,42); l.name=left=="☰"?"HeaderMenu":"HeaderBack"; l.gameObject.AddComponent<Button>(); ConfigureButton(l.GetComponent<Button>()); Label(title,titleKey,h.transform,dark?19:28,dark?Ink:new Color32(58,31,105,255),TextAlignmentOptions.Center,60,7,270,42); var r=Label(right,null,h.transform,24,Ink,TextAlignmentOptions.Center,337,7,45,42); r.name=right=="⚙"?"HeaderSettings":"HeaderHome"; r.gameObject.AddComponent<Button>(); ConfigureButton(r.GetComponent<Button>());
        }
        private void SideTools(Transform p){string[] t={"◇\n3D View","🔊\nAudio","↻\nReset"}; string[] k={"TOOL_3D_VIEW","TOOL_AUDIO","TOOL_RESET"}; for(int i=0;i<3;i++){var b=Box("Tool_"+i,p,new Color32(16,17,21,230)); b.AddComponent<Button>(); ConfigureButton(b.GetComponent<Button>()); Set(b,0,1,55,68,8,-92-i*72); TextInto(b,t[i],k[i],11,Color.white,TextAlignmentOptions.Center); Outline(b,new Color32(75,76,84,255),1);}}
        private void Controls(Transform p){var bar=Box("BottomControls",p,new Color32(20,21,27,245)); Set(bar,.5f,0,390,92,0,8); string[] x={"✥\nMove","↻\nRotate","⊕\nZoom","◉\nExplore"}; string[] k={"CTRL_MOVE","CTRL_ROTATE","CTRL_ZOOM","CTRL_EXPLORE"}; Color[] c={new Color32(23,94,172,255),new Color32(56,135,24,255),new Color32(205,111,0,255),Purple}; for(int i=0;i<4;i++){var b=Button(x[i],k[i],bar.transform,c[i],13); Set(b.gameObject,0,0,78,72,12+i*94,10);}}
        private void ExploreMenu(Transform p,string active){var m=Box("ExploreMenu",p,new Color32(28,21,38,250)); Set(m,1,.5f,180,315,-10,-15); Outline(m,Violet,2); Label("◉  Explore","EXP_HEADER",m.transform,17,Color.white,TextAlignmentOptions.Center,8,270,164,38); string[] a={"X-Ray\nSee inside","Info\nLearn more","Parts\nView all parts","Quiz\nTest yourself"}; string[] k={"EXP_XRAY","EXP_INFO","EXP_PARTS","EXP_QUIZ"}; for(int i=0;i<4;i++){var row=Box(a[i].Split('\n')[0],m.transform,active.StartsWith(a[i].Split('\n')[0],StringComparison.OrdinalIgnoreCase)?Violet:new Color32(43,43,51,255)); row.AddComponent<Button>(); Set(row,.5f,1,156,50,0,-58-i*57); TextInto(row,a[i],k[i],13,Color.white,TextAlignmentOptions.Left);}}
        private void CreateHeart(Transform p,bool xray){Color red=xray?new Color32(75,132,191,225):new Color32(184,38,37,255); var heart=Label("♥",null,p,245,red,TextAlignmentOptions.Center,55,220,330,360); heart.name="HeartModel"; Label("╲│╱",null,p,70,xray?new Color32(137,184,222,255):new Color32(55,78,181,255),TextAlignmentOptions.Center,130,430,180,100);}
        private void BottomNav(Transform p,int selected){var n=Box("BottomNavigation",p,Color.white); Set(n,.5f,0,410,66,0,0); string[] labels={"⌂\nHome","▤\nLibrary","☆\nBookmarks","♙\nProfile"}; string[] k={"NAV_HOME","NAV_LIBRARY","NAV_BOOKMARKS","NAV_PROFILE"}; for(int i=0;i<4;i++){var b=Button(labels[i],k[i],n.transform,Color.clear,11); b.name="Nav_"+i; b.GetComponentInChildren<TextMeshProUGUI>().color=new Color32(45,31,75,255); Set(b.gameObject,0,0,92,58,7+i*100,4);}}

        private GameObject Screen(string name,Transform p,Color color){var go=Box(name,p,color); Stretch(go.GetComponent<RectTransform>()); return go;}
        private GameObject Box(string name,Transform p,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(p,false); go.GetComponent<Image>().color=color; return go;}
        private Button Button(string text,string locKey,Transform p,Color color,int size)
        {
            var go=Box("Button_"+text.Replace("\n","_"),p,color);
            var b=go.AddComponent<Button>();
            ConfigureButton(b);
            TextInto(go,text,locKey,size,Color.white,TextAlignmentOptions.Center);
            return b;
        }
        private static void ConfigureButton(Button button){button.interactable=true;button.transition=Selectable.Transition.ColorTint;var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color32(225,215,255,255);colors.pressedColor=new Color32(180,150,230,255);colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color32(120,120,120,130);colors.colorMultiplier=1;colors.fadeDuration=.08f;button.colors=colors;button.navigation=new Navigation{mode=Navigation.Mode.Automatic};}
        private TextMeshProUGUI Label(string text,string locKey,Transform p,int size,Color color,TextAlignmentOptions align,float x,float y,float w,float h)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));
            go.transform.SetParent(p,false);
            Set(go,0,0,w,h,x,y);
            var t=go.GetComponent<TextMeshProUGUI>();
            t.text=text;t.fontSize=size;t.color=color;t.alignment=align;t.enableWordWrapping=true;
            if(!string.IsNullOrEmpty(locKey))
            {
                var loc=go.AddComponent<LocalizedText>();
                loc.key=locKey;
            }
            return t;
        }
        private void TextInto(GameObject parent,string text,string locKey,int size,Color color,TextAlignmentOptions align)
        {
            var t=Label(text,locKey,parent.transform,size,color,align,8,5,0,0);
            var r=t.rectTransform;
            r.anchorMin=Vector2.zero;
            r.anchorMax=Vector2.one;
            r.offsetMin=new Vector2(9,5);
            r.offsetMax=new Vector2(-9,-5);
        }
        private void Outline(GameObject go,Color color,float distance){var o=go.AddComponent<Outline>();o.effectColor=color;o.effectDistance=new Vector2(distance,-distance);}
        private static void Set(GameObject go,float ax,float ay,float w,float h,float x,float y){var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(ax,ay);r.pivot=new Vector2(ax,ay);r.sizeDelta=new Vector2(w,h);r.anchoredPosition=new Vector2(x,y);}
        private static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
        private void WireButtons(){screens.Clear();tabs.Clear(); var viewport=transform.Find("AR Learn Canvas/Backdrop/Phone/ScreenViewport");if(viewport!=null)foreach(Transform c in viewport)screens.Add(c.gameObject);var selector=transform.Find("AR Learn Canvas/Backdrop/ScreenSelector");if(selector!=null)for(int i=0;i<selector.childCount;i++){var b=selector.GetChild(i).GetComponent<Button>();if(b==null)continue;tabs.Add(b);int index=i;b.onClick.RemoveAllListeners();b.onClick.AddListener(()=>Show(index));}}

        private void WireScreenActions()
        {
            EnsureLocalizationServices();
            foreach (var b in GetComponentsInChildren<Button>(true))
            {
                ConfigureButton(b);
                if (b.name.StartsWith("Tab_")) continue;
                b.onClick.RemoveAllListeners();
                string n = b.name;
                if (n == "LanguageDropdown") b.onClick.AddListener(ToggleLanguageMenu);
                else if (n == "Lang_0" || n.Contains("English")) b.onClick.AddListener(() => SelectLanguage(LanguageManager.Language.English));
                else if (n == "Lang_1" || n.Contains("Hindi")) b.onClick.AddListener(() => SelectLanguage(LanguageManager.Language.Hindi));
                else if (n == "Lang_2" || n.Contains("Gujarati")) b.onClick.AddListener(() => SelectLanguage(LanguageManager.Language.Gujarati));
                else if (n == "HeaderMenu") b.onClick.AddListener(ToggleHomeMenu);
                else if (n == "HeaderSettings") b.onClick.AddListener(() => Show(11));
                else if (n == "HeaderHome") b.onClick.AddListener(() => Show(0));
                else if (n == "HeaderBack") b.onClick.AddListener(() => Show(Mathf.Max(0, selectedScreen - 1)));
                else if (n.StartsWith("Subject_") || n.StartsWith("Recent_")) b.onClick.AddListener(() => Show(1));
                else if (n == "X-Ray") b.onClick.AddListener(() => ShowAndCloseExplore(4));
                else if (n == "Parts") b.onClick.AddListener(() => ShowAndCloseExplore(5));
                else if (n == "Info") b.onClick.AddListener(() => ShowAndCloseExplore(6));
                else if (n == "Quiz") b.onClick.AddListener(() => Show(7));
                else if (n.StartsWith("Answer_")) { int answer = ParseSuffix(n); b.onClick.AddListener(() => SelectAnswer(answer)); }
                else if (n.Contains("Next")) b.onClick.AddListener(NextQuizQuestion);
                else if (n.Contains("Move")) b.onClick.AddListener(() => TransformHeart(new Vector3(18, 0, 0), 0, 1));
                else if (n.Contains("Rotate")) b.onClick.AddListener(() => TransformHeart(Vector3.zero, 25, 1));
                else if (n.Contains("Zoom")) b.onClick.AddListener(() => TransformHeart(Vector3.zero, 0, 1.12f));
                else if (n.Contains("Explore")) b.onClick.AddListener(ToggleExploreDropdown);
                else if (n.StartsWith("Tool_2")) b.onClick.AddListener(ResetHeart);
                else if (n.StartsWith("Tool_1")) b.onClick.AddListener(() => Show(8));
                else if (n.StartsWith("Tool_0")) b.onClick.AddListener(() => Show(2));
                else if (n == "AudioPlayer") b.onClick.AddListener(ToggleAudioPlayer);
                else if (n == "Nav_0") b.onClick.AddListener(() => Show(0));
                else if (n == "Nav_1" || n == "Nav_2") b.onClick.AddListener(() => Show(10));
                else if (n == "Nav_3") b.onClick.AddListener(() => Show(11));
                else if (n.StartsWith("LibraryRow_")) b.onClick.AddListener(() => Show(2));
                else if (n.StartsWith("LibraryTab_")) { int libraryTab=ParseSuffix(n); b.onClick.AddListener(() => SelectLibraryTab(libraryTab)); }
                else if (n.StartsWith("Setting_")) { int setting = ParseSuffix(n); b.onClick.AddListener(() => ToggleSetting(setting, b)); }
            }
        }

        private static int ParseSuffix(string value){int split=value.LastIndexOf('_');int result;return split>=0&&int.TryParse(value.Substring(split+1),out result)?result:0;}
        private void TransformHeart(Vector3 move,float rotate,float scale){var heart=screens[selectedScreen].transform.Find("HeartModel");if(heart==null){ShowToast(rotate!=0?"Scan target rotated":scale>1?"Scan target zoomed":"Scan target moved");return;}heart.localPosition+=move;heart.Rotate(0,0,rotate);heart.localScale*=scale;ShowToast(rotate!=0?"Model rotated":scale>1?"Model zoomed":"Model moved");}
        private void ResetHeart(){var heart=screens[selectedScreen].transform.Find("HeartModel");if(heart!=null){heart.localPosition=new Vector3(55,220,0);heart.localRotation=Quaternion.identity;heart.localScale=Vector3.one;}ShowToast("Model reset to default");}
        private void SelectAnswer(int answer){var card=screens[7].transform.Find("QuizCard");if(card==null)return;int correct=correctQuizAnswers[Mathf.Clamp(quizQuestion-1,0,correctQuizAnswers.Length-1)];for(int i=0;i<4;i++){var option=card.Find("Answer_"+i);if(option==null)continue;var image=option.GetComponent<Image>();image.color=i==answer?(answer==correct?new Color32(25,130,42,255):new Color32(145,45,48,255)):new Color32(42,44,51,255);}if(answer==correct){ShowToast("Correct answer! Loading next question...");if(Application.isPlaying)StartCoroutine(ChangeQuestionAfterCorrect());else NextQuizQuestion();}else ShowToast("Try again");}
        private IEnumerator ChangeQuestionAfterCorrect(){yield return new WaitForSeconds(.8f);NextQuizQuestion();}
        private void NextQuizQuestion(){quizQuestion=quizQuestion%5+1;UpdateQuizContent();ShowToast("Next question loaded");}
        private void UpdateQuizContent()
        {
            if(screens.Count<=7)return;var card=screens[7].transform.Find("QuizCard");if(card==null)return;int questionIndex=Mathf.Clamp(quizQuestion-1,0,quizQuestions.Length-1);var directLabels=new List<TextMeshProUGUI>();for(int i=0;i<card.childCount;i++){var label=card.GetChild(i).GetComponent<TextMeshProUGUI>();if(label!=null)directLabels.Add(label);}if(directLabels.Count>0)directLabels[0].text="Question "+quizQuestion+" of 5";if(directLabels.Count>1)directLabels[1].text=quizQuestions[questionIndex];for(int i=0;i<4;i++){var option=card.Find("Answer_"+i);if(option==null)continue;var label=option.GetComponentInChildren<TextMeshProUGUI>();if(label!=null)label.text=quizAnswers[questionIndex,i];var image=option.GetComponent<Image>();if(image!=null)image.color=new Color32(42,44,51,255);}
        }
        private void ToggleSetting(int setting,Button button)
        {
            if(setting==0)
            {
                var nextLang = (LanguageManager.Language)(((int)(LanguageManager.Instance != null ? LanguageManager.Instance.currentLanguage : LanguageManager.Language.English) + 1) % 3);
                SelectLanguage(nextLang);
                return;
            }
            if(setting==1)audioEnabled=!audioEnabled;
            if(setting==2)musicEnabled=!musicEnabled;
            var text=button.GetComponentInChildren<TextMeshProUGUI>();
            if(text!=null&&(setting==1||setting==2))
            {
                bool on=setting==1?audioEnabled:musicEnabled;
                text.text=text.text.Replace(on?"○":"●",on?"●":"○");
            }
            ShowToast(setting==1?(audioEnabled?"Audio enabled":"Audio disabled"):setting==2?(musicEnabled?"Music enabled":"Music disabled"):"Setting opened");
        }
        private void ToggleAudioPlayer(){audioEnabled=!audioEnabled;var player=screens.Count>8?screens[8].transform.Find("AudioPlayer"):null;if(player!=null){var label=player.Find("Label");if(label!=null){var text=label.GetComponent<TextMeshProUGUI>();text.text=(audioEnabled?"▶":"Ⅱ")+text.text.Substring(1);}}ShowToast(audioEnabled?"Audio playing":"Audio paused");}
        private void ToggleExploreDropdown()
        {
            if(screens.Count<=3)return;
            if(selectedScreen!=3){Show(3);var opened=screens[3].transform.Find("ExploreMenu");if(opened!=null)opened.gameObject.SetActive(true);return;}
            var menu=screens[3].transform.Find("ExploreMenu");if(menu!=null)menu.gameObject.SetActive(!menu.gameObject.activeSelf);
        }
        private void ShowAndCloseExplore(int screenIndex)
        {
            Show(screenIndex);
            if(screenIndex>=0&&screenIndex<screens.Count)
            {
                var menu=screens[screenIndex].transform.Find("ExploreMenu");
                if(menu!=null)menu.gameObject.SetActive(false);
            }
        }
        private void SelectLibraryTab(int index){var tabs=screens.Count>10?screens[10].transform.Find("LibraryTabs"):null;if(tabs!=null)for(int i=0;i<tabs.childCount;i++){var image=tabs.GetChild(i).GetComponent<Image>();var text=tabs.GetChild(i).GetComponentInChildren<TextMeshProUGUI>();if(image!=null)image.color=i==index?Violet:new Color32(225,225,232,255);if(text!=null)text.color=i==index?Color.white:Ink;}ShowToast(index==0?"Models selected":index==1?"Bookmarks selected":"History selected");}
        private void ToggleHomeMenu()
        {
            var phone=transform.Find("AR Learn Canvas/Backdrop/Phone");if(phone==null)return;
            var existing=phone.Find("HomeMenuDrawer");if(existing!=null){if(Application.isPlaying)Destroy(existing.gameObject);else DestroyImmediate(existing.gameObject);return;}
            var drawer=Box("HomeMenuDrawer",phone,new Color32(25,26,35,252));Set(drawer,0,1,230,300,8,-65);Outline(drawer,Violet,2);drawer.transform.SetAsLastSibling();
            string[] items={"⌂  Home","▤  My Library","⚙  Settings","🌐  Language"};
            for(int i=0;i<items.Length;i++)
            {
                var button=Button(items[i],null,drawer.transform,i==0?Purple:new Color32(48,49,60,255),16);
                Set(button.gameObject,.5f,1,204,50,0,-18-i*62);
                int action=i;
                button.onClick.AddListener(()=>{
                    if(action==0) Show(0);
                    else if(action==1) Show(10);
                    else if(action==2) Show(11);
                    else {
                        var nextLang = (LanguageManager.Language)(((int)(LanguageManager.Instance != null ? LanguageManager.Instance.currentLanguage : LanguageManager.Language.English) + 1) % 3);
                        SelectLanguage(nextLang);
                    }
                    if(drawer!=null){if(Application.isPlaying)Destroy(drawer);else DestroyImmediate(drawer);}
                });
            }
        }
        private void ShowToast(string message){var old=transform.Find("AR Learn Canvas/Backdrop/ActionToast");if(old!=null){if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}var parent=transform.Find("AR Learn Canvas/Backdrop");if(parent==null)return;var toast=Box("ActionToast",parent,new Color32(87,42,145,245));Set(toast,.5f,0,330,48,0,82);TextInto(toast,message,null,15,Color.white,TextAlignmentOptions.Center);if(Application.isPlaying)Destroy(toast,1.4f);}

        private void ToggleLanguageMenu(){if(languageMenu==null)languageMenu=transform.Find("AR Learn Canvas/Backdrop/Phone/ScreenViewport/01_Home_Subjects/LanguageMenu")?.gameObject;if(languageMenu!=null){languageMenu.SetActive(!languageMenu.activeSelf);languageMenu.transform.SetAsLastSibling();}}
        
        public void SelectLanguage(LanguageManager.Language language)
        {
            EnsureLocalizationServices();
            if (languageUI != null)
            {
                if (language == LanguageManager.Language.English) languageUI.English();
                else if (language == LanguageManager.Language.Hindi) languageUI.Hindi();
                else if (language == LanguageManager.Language.Gujarati) languageUI.Gujarati();
            }
            else if (LanguageManager.Instance != null)
            {
                LanguageManager.Instance.ChangeLanguage(language);
            }

            if (languageMenu != null) languageMenu.SetActive(false);
            ShowToast("Language: " + language);
        }

        [ContextMenu("Set Language -> English")]
        public void ContextSetEnglish() => SelectLanguage(LanguageManager.Language.English);

        [ContextMenu("Set Language -> Hindi")]
        public void ContextSetHindi() => SelectLanguage(LanguageManager.Language.Hindi);

        [ContextMenu("Set Language -> Gujarati")]
        public void ContextSetGujarati() => SelectLanguage(LanguageManager.Language.Gujarati);

        public string CurrentLanguage
        {
            get
            {
                if (languageManager == null) languageManager = LanguageManager.Instance;
                return languageManager != null ? languageManager.currentLanguage.ToString() : "English";
            }
        }

        public ARLearnUIReferences References { get { if(uiReferences==null)CacheInspectorReferences();return uiReferences; } }
        public void Show(int index){if(screens.Count==0)WireButtons();selectedScreen=Mathf.Clamp(index,0,Mathf.Max(0,screens.Count-1));for(int i=0;i<screens.Count;i++)screens[i].SetActive(i==selectedScreen);for(int i=0;i<tabs.Count;i++){var image=tabs[i].GetComponent<Image>();if(image!=null)image.color=i==selectedScreen?Violet:new Color32(55,57,67,255);}}
        private static void EnsureEventSystem(){if(FindObjectOfType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));}
    }
}
