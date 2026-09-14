using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ModelModeManager : MonoBehaviour
{
    public static ModelModeManager instance;

    private enum ViewMode { Model3D, XRay, AR, Info, Quiz }

    private ViewMode currentMode = ViewMode.Model3D;
    private bool arRunning;

    public bool IsARMode => currentMode == ViewMode.AR;

    [Header("CAMERA BACKGROUND")]
    [SerializeField] private Material modelSkybox;

    [Header("X-RAY")]
    [SerializeField] private XRayModeController xRayController;

    [Header("Text Mode")]
    [SerializeField] private TMP_Text m_modeText;

    [Header("BUTTONS")]
    [SerializeField] private Button mode3DButton;
    [SerializeField] private Button xRayModeButton;
    [SerializeField] private Button arModeButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button quizButton;
    [SerializeField] private Button tutorialPanelFinishButton;
    [SerializeField] private Button infoCancleButton;
    [SerializeField] private Button quizCancleButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button header3DButton;

    [Header("UNITY EVENTS")]
    [SerializeField] private UnityEvent on3DMode;
    [SerializeField] private UnityEvent onXRayMode;
    [SerializeField] private UnityEvent onARMode;
    [SerializeField] private UnityEvent onInfoMode;
    [SerializeField] private UnityEvent onQuizMode;
    [SerializeField] private UnityEvent onInfoCancel;
    [SerializeField] private UnityEvent onQuizCancel;

    [Header("PANELS")]
    [SerializeField] private GameObject model3DPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject arModePanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject quizPanel;

    [Header("QUIZ CONTROL")]
    [SerializeField] private QuizManager quizManager;

    [Header("INFO VOICE")]
    [SerializeField] private AudioSource infoVoicePlayer;
    [SerializeField] private AudioClip infoVoiceClip;

    [Header("ANIMATION SLIDER")]
    [SerializeField] private GameObject animationSlider;

    [Header("MODEL ROOTS")]
    [SerializeField] private GameObject model3DObject;

    [Header("XR AR FOUNDATION")]
    [SerializeField] private ARSession arSession;
    [SerializeField] private Camera arCamera;
    [SerializeField] private ARCameraManager arCameraManager;
    [SerializeField] private ARCameraBackground arCameraBackground;
    [SerializeField] private TrackedPoseDriver trackedPoseDriver;   

    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;
    private float savedCameraFOV;
    private bool cameraViewSaved;

    private void Awake()
    {
        instance = this;

        FindCameraReferences();
        SaveModelCameraView();

        // Stop AR before displaying the initial 3D view.
        SetARMode(false);
    }

    private void Start()
    {
        CreateHeader3DButtonIfNeeded();
        RegisterButtonListeners(true);

        Open3DMode();
    }

    // =========================================================
    // 3D MODE
    // =========================================================

    public void Open3DMode()
    {
        currentMode = ViewMode.Model3D;

        StopInfoAndQuizVoice();

        SetARMode(false);
        SetSkyboxBackground();

        if (xRayController != null)
            xRayController.DisableXRay();

        SetModeText("3D object mode");

        ShowPanels(
            model: true,
            ar: false,
            info: false,
            quiz: false,
            tutorial: false);

        SetObjectActive(model3DObject, true);
        SetObjectActive(animationSlider, true);

        Reset3DXRayButtons();
        SetHeaderButtons(true, false);

        on3DMode?.Invoke();
    }

    // =========================================================
    // X-RAY MODE
    // =========================================================

    public void OpenXRayMode()
    {
        currentMode = ViewMode.XRay;

        StopInfoAndQuizVoice();

        SetARMode(false);
        SetBlackBackground();

        SetModeText("X-Ray mode");

        ShowPanels(
            model: true,
            ar: false,
            info: false,
            quiz: false,
            tutorial: false);

        SetObjectActive(model3DObject, true);
        SetObjectActive(animationSlider, true);

        if (xRayController != null)
            xRayController.EnableXRay();

        SetButtonActive(mode3DButton, true);
        SetButtonActive(xRayModeButton, false);

        SetHeaderButtons(true, false);

        onXRayMode?.Invoke();
    }

    // =========================================================
    // AR MODE
    // =========================================================

    public void OpenARMode()
    {
        if (IsARMode && arRunning)
            return;

        if (!ValidateARReferences())
            return;

        StopInfoAndQuizVoice();

        currentMode = ViewMode.AR;

        if (xRayController != null)
            xRayController.DisableXRay();

        SetModeText("AR mode");

        SetObjectActive(model3DObject, false);
        SetObjectActive(animationSlider, false);

        ShowPanels(
            model: false,
            ar: true,
            info: false,
            quiz: false,
            tutorial: true);

        // AR starts immediately.
        // The tutorial is only an overlay.
        SetARMode(true);

        SetHeaderButtons(false, true);

        onARMode?.Invoke();
    }

    public void ActiveSetARMode()
    {
        // Tutorial Finish must not restart AR
        // after the user has left AR mode.
        if (!IsARMode)
            return;

        SetARMode(true);

        SetObjectActive(tutorialPanel, false);
    }

    private void SetARMode(bool active)
    {
        if (active)
        {
            if (arRunning)
                return;

            SaveModelCameraView();

            arRunning = true;

            // Keep the shared Camera and XR Origin
            // GameObjects active.
            arCamera.gameObject.SetActive(true);
            arCamera.enabled = true;

            SetBlackBackground();

            arSession.enabled = true;
            arCameraManager.enabled = true;
            arCameraBackground.enabled = true;
            trackedPoseDriver.enabled = true;
        }
        else
        {
            arRunning = false;

            // Stop background rendering before restoring
            // the normal camera settings.
            if (arCameraBackground != null)
                arCameraBackground.enabled = false;

            if (arCameraManager != null)
                arCameraManager.enabled = false;

            if (trackedPoseDriver != null)
                trackedPoseDriver.enabled = false;

            if (arSession != null)
                arSession.enabled = false;

            RestoreModelCameraView();
        }
    }

    // =========================================================
    // INFO MODE
    // =========================================================

    public void OpenInfo()
    {
        currentMode = ViewMode.Info;

        StopInfoAndQuizVoice();

        SetARMode(false);
        SetSkyboxBackground();

        if (xRayController != null)
            xRayController.DisableXRay();

        SetModeText("Info mode");

        SetObjectActive(model3DObject, false);
        SetObjectActive(animationSlider, false);

        ShowPanels(
            model: false,
            ar: false,
            info: true,
            quiz: false,
            tutorial: false);

        Reset3DXRayButtons();
        SetHeaderButtons(false, false);

        if (infoVoicePlayer != null && infoVoiceClip != null)
        {
            infoVoicePlayer.clip = infoVoiceClip;
            infoVoicePlayer.Play();
        }

        onInfoMode?.Invoke();
    }

    public void CloseInfo()
    {
        Open3DMode();

        onInfoCancel?.Invoke();
    }

    // =========================================================
    // QUIZ MODE
    // =========================================================

    public void OpenQuiz()
    {
        currentMode = ViewMode.Quiz;

        StopInfoAndQuizVoice();

        SetARMode(false);
        SetSkyboxBackground();

        if (xRayController != null)
            xRayController.DisableXRay();

        SetModeText("Quiz mode");

        SetObjectActive(model3DObject, false);
        SetObjectActive(animationSlider, false);

        ShowPanels(
            model: false,
            ar: false,
            info: false,
            quiz: true,
            tutorial: false);

        Reset3DXRayButtons();
        SetHeaderButtons(false, false);

        if (quizManager != null)
            quizManager.StartQuiz();

        onQuizMode?.Invoke();
    }

    public void CloseQuiz()
    {
        Open3DMode();

        onQuizCancel?.Invoke();
    }

    // =========================================================
    // CAMERA REFERENCES
    // =========================================================

    private void FindCameraReferences()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
            return;

        if (arCameraManager == null)
        {
            arCameraManager =
                arCamera.GetComponent<ARCameraManager>();
        }

        if (arCameraBackground == null)
        {
            arCameraBackground =
                arCamera.GetComponent<ARCameraBackground>();
        }

        if (trackedPoseDriver == null)
        {
            trackedPoseDriver =
                arCamera.GetComponent<TrackedPoseDriver>();
        }

        if (arSession == null)
        {
            arSession = FindObjectOfType<ARSession>(true);
        }
    }

    private bool ValidateARReferences()
    {
        FindCameraReferences();

        if (arSession == null ||
            arCamera == null ||
            arCameraManager == null ||
            arCameraBackground == null ||
            trackedPoseDriver == null)
        {
            Debug.LogError(
                "ModelModeManager: Assign AR Session, Main Camera, " +
                "AR Camera Manager, AR Camera Background and " +
                "Tracked Pose Driver (Input System).",
                this);

            return false;
        }

        if (!arSession.gameObject.activeInHierarchy ||
            !arCamera.gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "ModelModeManager: Keep AR Session, XR Origin, " +
                "Camera Offset and Main Camera GameObjects active. " +
                "Only their AR components are toggled.",
                this);

            return false;
        }

        return true;
    }

    // =========================================================
    // SAVE / RESTORE NORMAL CAMERA VIEW
    // =========================================================

    private void SaveModelCameraView()
    {
        if (arCamera == null)
            return;

        savedCameraPosition = arCamera.transform.position;
        savedCameraRotation = arCamera.transform.rotation;
        savedCameraFOV = arCamera.fieldOfView;

        cameraViewSaved = true;
    }

    private void RestoreModelCameraView()
    {
        if (arCamera == null)
            return;

        arCamera.gameObject.SetActive(true);
        arCamera.enabled = true;

        if (cameraViewSaved)
        {
            arCamera.transform.SetPositionAndRotation(
                savedCameraPosition,
                savedCameraRotation);

            arCamera.fieldOfView = savedCameraFOV;
        }

        arCamera.ResetProjectionMatrix();
        arCamera.ResetWorldToCameraMatrix();
    }

    private void SetSkyboxBackground()
    {
        if (arCamera != null)
            arCamera.clearFlags = CameraClearFlags.Skybox;

        if (modelSkybox != null)
            RenderSettings.skybox = modelSkybox;
    }

    private void SetBlackBackground()
    {
        if (arCamera == null)
            return;

        arCamera.clearFlags = CameraClearFlags.SolidColor;
        arCamera.backgroundColor = Color.black;
    }

    // =========================================================
    // UI HELPERS
    // =========================================================

    private void ShowPanels(
        bool model,
        bool ar,
        bool info,
        bool quiz,
        bool tutorial)
    {
        SetObjectActive(model3DPanel, model);
        SetObjectActive(arModePanel, ar);
        SetObjectActive(infoPanel, info);
        SetObjectActive(quizPanel, quiz);
        SetObjectActive(tutorialPanel, tutorial);
    }

    private void SetModeText(string text)
    {
        if (m_modeText != null)
            m_modeText.text = text;
    }

    private static void SetObjectActive(
        GameObject target,
        bool active)
    {
        if (target != null && target.activeSelf != active)
            target.SetActive(active);
    }

    private static void SetButtonActive(
        Button button,
        bool active)
    {
        if (button != null)
            SetObjectActive(button.gameObject, active);
    }

    private void SetHeaderButtons(
        bool showBack,
        bool show3D)
    {
        SetButtonActive(backButton, showBack);
        SetButtonActive(header3DButton, show3D);
    }

    private void Reset3DXRayButtons()
    {
        SetButtonActive(mode3DButton, false);
        SetButtonActive(xRayModeButton, true);
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void StopInfoAndQuizVoice()
    {
        if (infoVoicePlayer != null)
        {
            infoVoicePlayer.Stop();
            infoVoicePlayer.clip = null;
        }

        if (quizManager != null)
            quizManager.StopQuizAudio();
    }

    // =========================================================
    // BUTTON LISTENERS
    // =========================================================

    private void RegisterButtonListeners(bool add)
    {
        SetListener(mode3DButton, Open3DMode, add);
        SetListener(xRayModeButton, OpenXRayMode, add);
        SetListener(arModeButton, OpenARMode, add);

        SetListener(infoButton, OpenInfo, add);
        SetListener(quizButton, OpenQuiz, add);

        SetListener(infoCancleButton, CloseInfo, add);
        SetListener(quizCancleButton, CloseQuiz, add);

        SetListener(
            tutorialPanelFinishButton,
            ActiveSetARMode,
            add);

        SetListener(
            header3DButton,
            ReloadCurrentScene,
            add);
    }

    private static void SetListener(
        Button button,
        UnityAction callback,
        bool add)
    {
        if (button == null)
            return;

        if (add)
            button.onClick.AddListener(callback);
        else
            button.onClick.RemoveListener(callback);
    }

    // =========================================================
    // HEADER 3D BUTTON
    // =========================================================

    private void CreateHeader3DButtonIfNeeded()
    {
        if (header3DButton != null ||
            mode3DButton == null ||
            backButton == null)
        {
            return;
        }

        header3DButton = Instantiate(
            mode3DButton,
            backButton.transform.parent);

        header3DButton.gameObject.name =
            "Header 3D Mode Button";

        // Replace the copied event, including copied
        // Inspector callbacks.
        header3DButton.onClick =
            new Button.ButtonClickedEvent();

        RectTransform headerRect =
            header3DButton.transform as RectTransform;

        RectTransform backRect =
            backButton.transform as RectTransform;

        if (headerRect != null && backRect != null)
        {
            headerRect.anchorMin = backRect.anchorMin;
            headerRect.anchorMax = backRect.anchorMax;
            headerRect.pivot = backRect.pivot;

            headerRect.anchoredPosition3D =
                backRect.anchoredPosition3D;

            headerRect.sizeDelta = backRect.sizeDelta;
            headerRect.localRotation = backRect.localRotation;
            headerRect.localScale = backRect.localScale;

            headerRect.SetSiblingIndex(
                backRect.GetSiblingIndex() + 1);
        }

        if (header3DButton.GetComponent<ButtonHoverFeedback>() == null)
        {
            header3DButton.gameObject
                .AddComponent<ButtonHoverFeedback>();
        }

        header3DButton.gameObject.SetActive(false);
    }

    private void ReloadCurrentScene()
    {
        StopInfoAndQuizVoice();

        Scene scene = SceneManager.GetActiveScene();

        // Preserve the original header button's
        // scene-reload behaviour.
        if (scene.buildIndex >= 0)
        {
            SceneManager.LoadScene(scene.buildIndex);
        }
        else
        {
            Debug.LogWarning(
                "Scene is not in the build scene list. " +
                "Returning to 3D without reloading.",
                this);

            Open3DMode();
        }
    }

    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        RegisterButtonListeners(false);

        if (instance == this)
            instance = null;
    }
}