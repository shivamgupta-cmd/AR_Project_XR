using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AshokaPillerManagerScript : MonoBehaviour
{
    public Camera Camera;

    [Header("Buttons")]
    public Button[] labels;

    [Header("Lines")]
    public GameObject[] Lines;


    [Header("Audio")]
    public AudioClip[] audioClips;
    public AudioSource audioSource;

    [Header("Camera Settings")]
    public float cameraSpeed = 2f;

    [Header("Manual Camera Positions")]
    public Transform lionCapitalPosition;
    public Transform abacusPosition;
    public Transform shaftPosition;
    public Transform basePosition;

    private Vector3 previousPosition;
    private Quaternion previousRotation;

    private bool isPlaying = false;
    public ObjectRotator ObjectRotator;

    // Disable all buttons
    void DisableBtn()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].interactable = false;
        }
    }


    // Enable all buttons
    void EnableBtn()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            labels[i].interactable = true;
        }
    }
    void DisableLines()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i].SetActive(false);
        }
    }


    // Enable all buttons
    void EnableLines()
    {
        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i].SetActive(true);
        }
    }


    // =========================
    // LION CAPITAL
    // =========================

    public void LineCapital()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(
                lionCapitalPosition,
                0
            )
        );
    }


    // =========================
    // ABACUS
    // =========================

    public void Abacus()
    {
        if (isPlaying) return;
        ObjectRotator.StartRotation();
        StartCoroutine(
            CameraPartHandler(
                abacusPosition,
                1
            )
        );
    }


    // =========================
    // SHAFT
    // =========================

    public void Shaft()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(
                shaftPosition,
                2
            )
        );
    }


    // =========================
    // BASE
    // =========================

    public void Base()
    {
        if (isPlaying) return;

        StartCoroutine(
            CameraPartHandler(
                basePosition,
                3
            )
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
        isPlaying = true;

        // Disable all buttons
        DisableBtn();


        // Save current camera position
        previousPosition = Camera.transform.position;
        previousRotation = Camera.transform.rotation;


        // Move camera to selected part
        yield return StartCoroutine(
            MoveCamera(targetPosition)
        );


        // Play audio
        if (audioSource != null &&
            audioClips != null &&
            audioIndex >= 0 &&
            audioIndex < audioClips.Length &&
            audioClips[audioIndex] != null)
        {
            audioSource.clip = audioClips[audioIndex];
            audioSource.Play();

            // Wait until audio finishes
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }


        // Return camera to previous position
        yield return StartCoroutine(
            ReturnCamera()
        );


        // Enable all buttons
        EnableBtn();
        if(audioIndex == 1)
        {
            ObjectRotator.StopRotation();
        }
        isPlaying = false;
    }


    // =========================
    // MOVE TO TARGET
    // =========================

    IEnumerator MoveCamera(Transform target)
    {
        if (target == null)
            yield break;

        while (
            Vector3.Distance(
                Camera.transform.position,
                target.position
            ) > 0.01f ||
            Quaternion.Angle(
                Camera.transform.rotation,
                target.rotation
            ) > 0.5f
        )
        {
            Camera.transform.position = Vector3.MoveTowards(
                Camera.transform.position,
                target.position,
                cameraSpeed * Time.deltaTime
            );

            Camera.transform.rotation = Quaternion.RotateTowards(
                Camera.transform.rotation,
                target.rotation,
                cameraSpeed * 100f * Time.deltaTime
            );

            yield return null;
        }

        // Exact final position
        Camera.transform.position = target.position;
        Camera.transform.rotation = target.rotation;
        DisableLines();
    }


    // =========================
    // RETURN TO PREVIOUS
    // =========================

    IEnumerator ReturnCamera()
    {
        EnableLines();
        while (
            Vector3.Distance(
                Camera.transform.position,
                previousPosition
            ) > 0.01f ||
            Quaternion.Angle(
                Camera.transform.rotation,
                previousRotation
            ) > 0.5f
        )
        {
            Camera.transform.position = Vector3.MoveTowards(
                Camera.transform.position,
                previousPosition,
                cameraSpeed * Time.deltaTime
            );

            Camera.transform.rotation = Quaternion.RotateTowards(
                Camera.transform.rotation,
                previousRotation,
                cameraSpeed * 100f * Time.deltaTime
            );

            yield return null;
        }

        // Exact final position
        Camera.transform.position = previousPosition;
        Camera.transform.rotation = previousRotation;
    }
}