using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SanchiStuppaManager : MonoBehaviour
{
    // =========================================================
    // CAMERA
    // =========================================================

    [Header("Camera")]
    public GameObject Camera;

    [Header("Camera Settings")]
    public float cameraSpeed = 2f;


    // =========================================================
    // BUTTONS
    //
    // 0 = Chattra
    // 1 = Harmika
    // 2 = Dome (Anda)
    // 3 = Medhi
    // 4 = Vedika
    // 5 = Toranas
    // 6 = Torana Carvings
    // =========================================================

    [Header("Buttons")]
    public Button[] labels;


    // =========================================================
    // LINES
    // =========================================================

    [Header("Lines")]
    public GameObject[] Lines;


    // =========================================================
    // AUDIO
    //
    // Same order as buttons
    // =========================================================

    [Header("Part Audio")]
    public AudioClip[] audioClips;

    [Header("Conclusion Audio")]
    public AudioClip BeforeconclusionAudio;
    public AudioClip conclusionAudio;

    public AudioSource audioSource;


    // =========================================================
    // CAMERA TARGETS
    // =========================================================

    [Header("Camera Targets")]

    public Transform Chattra;
    public Transform Harmika;
    public Transform DomeAnda;
    public Transform Medhi;
    public Transform Vedika;
    public Transform Toranas;
    public Transform ToranaCarvings;


    // =========================================================
    // CAMERA RETURN DATA
    // =========================================================

    private Vector3 previousPosition;
    private Quaternion previousRotation;


    // =========================================================
    // STATE
    // =========================================================

    private bool isPlaying = false;

    // 7 parts
    private bool[] sectionCompleted;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        sectionCompleted = new bool[7];

        //DisableLines();

        //EnableBtn();
    }


    // =========================================================
    // BUTTON CONTROL
    // =========================================================

    void DisableBtn()
    {
        if (labels == null)
            return;

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
                labels[i].interactable = false;
        }
    }


    void EnableBtn()
    {
        if (labels == null)
            return;

        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null)
                labels[i].interactable = true;
        }
    }


    // =========================================================
    // LINES
    // =========================================================

    void DisableLines()
    {
        if (Lines == null)
            return;

        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] != null)
                Lines[i].SetActive(false);
        }
    }


    void EnableLines()
    {
        if (Lines == null)
            return;

        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] != null)
                Lines[i].SetActive(true);
        }
    }


    // =========================================================
    // 1. CHATTRА
    // =========================================================

    public void ChattraPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                Chattra,
                0
            )
        );
    }


    // =========================================================
    // 2. HARMIKA
    // =========================================================

    public void HarmikaPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                Harmika,
                1
            )
        );
    }


    // =========================================================
    // 3. DOME / ANDA
    // =========================================================

    public void DomeAndaPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                DomeAnda,
                2
            )
        );
    }


    // =========================================================
    // 4. MEDHI
    // =========================================================

    public void MedhiPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                Medhi,
                3
            )
        );
    }


    // =========================================================
    // 5. VEDIKA
    // =========================================================

    public void VedikaPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                Vedika,
                4
            )
        );
    }


    // =========================================================
    // 6. TORANAS
    // =========================================================

    public void ToranasPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                Toranas,
                5
            )
        );
    }


    // =========================================================
    // 7. TORANA CARVINGS
    // =========================================================

    public void ToranaCarvingsPosition()
    {
        if (isPlaying)
            return;

        StartCoroutine(
            CameraPartHandler(
                ToranaCarvings,
                6
            )
        );
    }


    // =========================================================
    // MAIN CAMERA + AUDIO HANDLER
    // =========================================================

    IEnumerator CameraPartHandler(
        Transform targetPosition,
        int audioIndex
    )
    {
        if (targetPosition == null)
        {
            Debug.LogWarning(
                "Camera target is missing for index: " + audioIndex
            );

            yield break;
        }

        isPlaying = true;

        // Disable all buttons while playing
        DisableBtn();


        // Save current camera position
        if (Camera != null)
        {
            previousPosition =
                Camera.transform.position;

            //previousRotation =
            //    Camera.transform.rotation;
        }


        // Move camera to selected part
        if (Camera != null)
        {
            yield return StartCoroutine(
                MoveCamera(targetPosition)
            );
        }


        // Play selected part VO
        yield return StartCoroutine(
            PlayPartAudio(audioIndex)
        );


        // Mark section completed
        if (audioIndex >= 0 &&
            audioIndex < sectionCompleted.Length)
        {
            sectionCompleted[audioIndex] = true;
        }


        // Return camera
        if (Camera != null)
        {
            yield return StartCoroutine(
                ReturnCamera()
            );
        }


        isPlaying = false;


        // Check if all 7 parts are completed
        if (AllSectionsCompleted())
        {
            StartCoroutine(
                PlayConclusion()
            );
        }
        else
        {
            EnableBtn();
        }
    }


    // =========================================================
    // PLAY PART AUDIO
    // =========================================================

    IEnumerator PlayPartAudio(int index)
    {
        if (audioSource == null)
            yield break;

        if (audioClips == null)
            yield break;

        if (index < 0 ||
            index >= audioClips.Length)
        {
            yield break;
        }

        if (audioClips[index] == null)
        {
            Debug.LogWarning(
                "Audio clip missing at index: " + index
            );

            yield break;
        }


        audioSource.clip =
            audioClips[index];

        audioSource.Play();


        // Wait until VO finishes
        yield return new WaitWhile(
            () => audioSource.isPlaying
        );
    }


    // =========================================================
    // MOVE CAMERA
    // =========================================================

    IEnumerator MoveCamera(Transform target)
    {
        if (Camera == null ||
            target == null)
        {
            yield break;
        }


        // Hide lines while camera moves
        DisableLines();


        while (
            Vector3.Distance(
                Camera.transform.position,
                target.position
            ) > 0.01f
            
            //Quaternion.Angle(
            //    Camera.transform.rotation,
            //    target.rotation
            //) > 0.5f
        )
        {
            Camera.transform.position =
                Vector3.MoveTowards(
                    Camera.transform.position,
                    target.position,
                    cameraSpeed *
                    Time.deltaTime
                );


            //Camera.transform.rotation =
            //    Quaternion.RotateTowards(
            //        Camera.transform.rotation,
            //        target.rotation,
            //        cameraSpeed *
            //        100f *
            //        Time.deltaTime
            //    );


            yield return null;
        }


        // Final position
        Camera.transform.position =
            target.position;

        //Camera.transform.rotation =
        //    target.rotation;
    }


    // =========================================================
    // RETURN CAMERA
    // =========================================================

    IEnumerator ReturnCamera()
    {
        if (Camera == null)
            yield break;


        while (
            Vector3.Distance(
                Camera.transform.position,
                previousPosition
            ) > 0.01f
            
            //Quaternion.Angle(
            //    Camera.transform.rotation,
            //    previousRotation
            //) > 0.5f
        )
        {
            Camera.transform.position =
                Vector3.MoveTowards(
                    Camera.transform.position,
                    previousPosition,
                    cameraSpeed *
                    Time.deltaTime
                );


            //Camera.transform.rotation =
            //    Quaternion.RotateTowards(
            //        Camera.transform.rotation,
            //        previousRotation,
            //        cameraSpeed *
            //        100f *
            //        Time.deltaTime
            //    );


            yield return null;
        }


        // Final position
        Camera.transform.position =
            previousPosition;

        //Camera.transform.rotation =
        //    previousRotation;


        // Show lines again
        EnableLines();
    }


    // =========================================================
    // CHECK ALL 7 PARTS
    // =========================================================

    bool AllSectionsCompleted()
    {
        for (int i = 0;
            i < sectionCompleted.Length;
            i++)
        {
            if (!sectionCompleted[i])
                return false;
        }

        return true;
    }


    // =========================================================
    // CONCLUSION VO
    // =========================================================

    IEnumerator PlayConclusion()
    {
        isPlaying = true;

        // Disable buttons during conclusion
        DisableBtn();

        // Make sure lines are visible
        EnableLines();


        // Play conclusion VO
        if (audioSource != null &&
            conclusionAudio != null)
        {

            audioSource.clip = BeforeconclusionAudio;
            audioSource.Play();
            yield return new WaitWhile(
               () => audioSource.isPlaying
            );

            audioSource.clip =
                conclusionAudio;

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
}