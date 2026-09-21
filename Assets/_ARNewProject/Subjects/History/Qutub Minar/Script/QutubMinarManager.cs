using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class QutubMinarManager : MonoBehaviour
{
    [Header("Camera")]
    public GameObject Camera;

    [Header("Buttons")]
    public Button[] labels;

    [Header("Lines")]
    public GameObject[] Lines;

    [Header("Audio")]
    public AudioClip[] audioClips;

    [Tooltip("Conclusion Voice Over")]
    public AudioClip conclusionAudio;

    public AudioSource audioSource;

    [Header("Camera Settings")]
    public float cameraSpeed = 2f;

    [Header("Manual Camera Positions")]
    public Transform Base;
    public Transform Top;
    public Transform shaft;
    public Transform Balconies;
    public Transform Carvings;

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private bool isPlaying = false;

    // Track completed sections
    private bool[] sectionCompleted;


    // =========================
    // START
    // =========================

    void Start()
    {
        sectionCompleted = new bool[5];
    }


    // =========================
    // BUTTON CONTROL
    // =========================

    void DisableBtn()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
                labels[i].interactable = false;
        }
    }

    void EnableBtn()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
                labels[i].interactable = true;
        }
    }


    // =========================
    // LINES
    // =========================

    void DisableLines()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] != null)
                Lines[i].SetActive(false);
        }
    }

    void EnableLines()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] != null)
                Lines[i].SetActive(true);
        }
    }


    // =========================
    // BASE
    // =========================

    public void BasePosition()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(Base, 4)
        );
    }


    // =========================
    // TOP
    // =========================

    public void TopPosition()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(Top, 0)
        );
    }


    // =========================
    // SHAFT
    // =========================

    public void ShaftPosition()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(shaft, 1)
        );
    }


    // =========================
    // BALCONIES
    // =========================

    public void BalconiesPosition()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(Balconies, 3)
        );
    }


    // =========================
    // CARVINGS
    // =========================

    public void CarvingsPosition()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(Carvings, 2)
        );
    }


    // =========================
    // MAIN HANDLER
    // =========================

    IEnumerator CameraPartHandler(
        Transform targetPosition,
        int audioIndex
    )
    {
        if (targetPosition == null)
            yield break;

        isPlaying = true;

        // Disable all buttons
        DisableBtn();

        // Save current camera position
        previousPosition = Camera.transform.position;
        previousRotation = Camera.transform.rotation;

        // Move camera
        yield return StartCoroutine(
            MoveCamera(targetPosition)
        );

        // =========================
        // PLAY PART AUDIO
        // =========================

        if (audioSource != null &&
            audioClips != null &&
            audioIndex >= 0 &&
            audioIndex < audioClips.Length &&
            audioClips[audioIndex] != null)
        {
            audioSource.clip = audioClips[audioIndex];
            audioSource.Play();

            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }

        // Mark section completed
        sectionCompleted[audioIndex] = true;

        // Return camera
        yield return StartCoroutine(
            ReturnCamera()
        );

        isPlaying = false;

        // =========================
        // CHECK ALL COMPLETED
        // =========================

        if (AllSectionsCompleted())
        {
            StartCoroutine(PlayConclusion());
        }
        else
        {
            EnableBtn();
        }
    }


    // =========================
    // CHECK ALL SECTIONS
    // =========================

    bool AllSectionsCompleted()
    {
        for (int i = 0; i < sectionCompleted.Length; i++)
        {
            if (!sectionCompleted[i])
                return false;
        }

        return true;
    }


    // =========================
    // CONCLUSION VO
    // =========================

    IEnumerator PlayConclusion()
    {
        isPlaying = true;

        // Disable buttons
        DisableBtn();

        // Make sure lines are visible
        EnableLines();

        // Play conclusion VO
        if (audioSource != null &&
            conclusionAudio != null)
        {
            audioSource.clip = conclusionAudio;
            audioSource.Play();

            // Wait until conclusion finishes
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }

        // Enable buttons after conclusion
        EnableBtn();

        isPlaying = false;
    }


    // =========================
    // MOVE CAMERA
    // =========================

    IEnumerator MoveCamera(Transform target)
    {
        if (target == null)
            yield break;

        while (
            Vector3.Distance(
                Camera.transform.position,
                target.position
            ) > 0.01f
            ||
            Quaternion.Angle(
                Camera.transform.rotation,
                target.rotation
            ) > 0.5f
        )
        {
            Camera.transform.position =
                Vector3.MoveTowards(
                    Camera.transform.position,
                    target.position,
                    cameraSpeed * Time.deltaTime
                );

            Camera.transform.rotation =
                Quaternion.RotateTowards(
                    Camera.transform.rotation,
                    target.rotation,
                    cameraSpeed * 100f * Time.deltaTime
                );

            yield return null;
        }

        Camera.transform.position = target.position;
        Camera.transform.rotation = target.rotation;

        DisableLines();
    }


    // =========================
    // RETURN CAMERA
    // =========================

    IEnumerator ReturnCamera()
    {
        EnableLines();

        while (
            Vector3.Distance(
                Camera.transform.position,
                previousPosition
            ) > 0.01f
            ||
            Quaternion.Angle(
                Camera.transform.rotation,
                previousRotation
            ) > 0.5f
        )
        {
            Camera.transform.position =
                Vector3.MoveTowards(
                    Camera.transform.position,
                    previousPosition,
                    cameraSpeed * Time.deltaTime
                );

            Camera.transform.rotation =
                Quaternion.RotateTowards(
                    Camera.transform.rotation,
                    previousRotation,
                    cameraSpeed * 100f * Time.deltaTime
                );

            yield return null;
        }

        Camera.transform.position = previousPosition;
        Camera.transform.rotation = previousRotation;
    }
}