//using System.Collections;
//using TMPro;
//using UnityEngine;
//using UnityEngine.Events;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;

//public class ARImageContentTracker : MonoBehaviour
//{
//    [Header("AR MANAGERS")]
//    [SerializeField] private ARTrackedImageManager trackedImageManager;
//    [SerializeField] private ARAnchorManager anchorManager;


//    [Header("CONTENT")]
//    [SerializeField] private Transform contentRoot;


//    [Header("PLACEMENT")]
//    [SerializeField] private Vector3 positionOffset = Vector3.zero;

//    [Tooltip("Model ki final WORLD rotation. 0,0,0 rakho agar straight chahiye.")]
//    [SerializeField] private Vector3 rotationOffset = Vector3.zero;


//    [Header("VOICE OVER")]
//    [SerializeField] private AudioSource voiceSource;
//    [SerializeField] private AudioClip modelActiveVO;


//    [Header("EVENTS")]
//    [SerializeField] private UnityEvent onModelActivated;


//    private bool hasBeenPlaced;
//    private ARAnchor lockedAnchor;


//    // =========================================================
//    // AWAKE
//    // =========================================================

//    private void Awake()
//    {
//        if (trackedImageManager == null)
//            trackedImageManager = GetComponent<ARTrackedImageManager>();

//        if (anchorManager == null)
//            anchorManager = GetComponent<ARAnchorManager>();


//        // Start me model hide
//        if (contentRoot != null)
//            contentRoot.gameObject.SetActive(false);
//    }


//    // =========================================================
//    // UPDATE
//    // =========================================================

//    private void Update()
//    {
//        if (hasBeenPlaced)
//            return;


//        if (trackedImageManager == null ||
//            anchorManager == null ||
//            contentRoot == null)
//            return;


//        foreach (ARTrackedImage image in trackedImageManager.trackables)
//        {
//            if (image.trackingState == TrackingState.Tracking)
//            {
//                PlaceAndLockObject(image);
//                return;
//            }
//        }
//    }


//    // =========================================================
//    // PLACE AND LOCK
//    // =========================================================

//    private void PlaceAndLockObject(ARTrackedImage image)
//    {
//        if (hasBeenPlaced)
//            return;


//        hasBeenPlaced = true;


//        // =====================================================
//        // POSITION
//        // =====================================================

//        Vector3 finalPosition =
//            image.transform.TransformPoint(positionOffset);


//        // =====================================================
//        // ROTATION
//        //
//        // IMPORTANT:
//        // Image ki rotation use nahi kar rahe.
//        //
//        // Isliye agar tracked image X = -90 hai,
//        // tab bhi model 0,0,0 par rahega.
//        // =====================================================

//        Quaternion finalRotation =
//            Quaternion.Euler(rotationOffset);


//        // =====================================================
//        // CREATE ANCHOR
//        // =====================================================

//        GameObject anchorObject =
//            new GameObject("AR_Locked_Anchor");


//        anchorObject.transform.SetPositionAndRotation(
//            finalPosition,
//            finalRotation
//        );


//        lockedAnchor =
//            anchorObject.AddComponent<ARAnchor>();


//        // =====================================================
//        // SAVE SCALE
//        // =====================================================

//        Vector3 originalScale =
//            contentRoot.localScale;


//        // =====================================================
//        // PARENT MODEL
//        // =====================================================

//        contentRoot.SetParent(
//            anchorObject.transform,
//            false
//        );


//        // =====================================================
//        // RESET MODEL TRANSFORM
//        // =====================================================

//        contentRoot.localPosition =
//            Vector3.zero;

//        contentRoot.localRotation =
//            Quaternion.identity;

//        contentRoot.localScale =
//            originalScale;


//        // Extra safety
//        contentRoot.localEulerAngles =
//            Vector3.zero;


//        // =====================================================
//        // ACTIVATE MODEL
//        // =====================================================

//        contentRoot.gameObject.SetActive(true);


//        Debug.Log(
//            "AR Model Activated | Rotation = " +
//            contentRoot.eulerAngles
//        );


//        // =====================================================
//        // PLAY VO
//        // =====================================================

//        PlayModelActiveVO();


//        // =====================================================
//        // CALL EVENT
//        // =====================================================

//        if (onModelActivated != null)
//            onModelActivated.Invoke();


//        // =====================================================
//        // STOP IMAGE TRACKING
//        // =====================================================

//        if (trackedImageManager != null)
//            trackedImageManager.enabled = false;
//    }


//    // =========================================================
//    // PLAY VO
//    // =========================================================

//    private void PlayModelActiveVO()
//    {
//        if (voiceSource == null || modelActiveVO == null)
//            return;


//        voiceSource.Stop();

//        voiceSource.clip =
//            modelActiveVO;

//        voiceSource.Play();


//        Debug.Log("AR Model Active VO Played");
//    }
//}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARImageContentTracker : MonoBehaviour
{
    [Header("AR MANAGERS")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;

    [Header("CONTENT")]
    [SerializeField] private Transform contentRoot;

    [Header("PLACEMENT")]
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    [Tooltip("Final WORLD rotation")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Header("VOICE OVER")]
    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private AudioClip modelActiveVO;

    [Header("EVENTS")]
    [SerializeField] private UnityEvent onModelActivated;

    private bool hasBeenPlaced;
    private ARAnchor lockedAnchor;

    private void Awake()
    {
        if (trackedImageManager == null)
            trackedImageManager = GetComponent<ARTrackedImageManager>();

        if (contentRoot != null)
            contentRoot.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(
        ARTrackedImagesChangedEventArgs args)
    {
        if (hasBeenPlaced)
            return;

        // Newly detected images
        foreach (ARTrackedImage image in args.added)
        {
            if (TryPlace(image))
                return;
        }

        // Images that were detected but became fully tracked later
        foreach (ARTrackedImage image in args.updated)
        {
            if (TryPlace(image))
                return;
        }
    }

    private bool TryPlace(ARTrackedImage image)
    {
        if (hasBeenPlaced)
            return false;

        if (image == null)
            return false;

        if (image.trackingState != TrackingState.Tracking)
            return false;

        Debug.Log(
            "IMAGE DETECTED: " +
            image.referenceImage.name
        );

        PlaceAndLockObject(image);

        return true;
    }

    private void PlaceAndLockObject(ARTrackedImage image)
    {
        if (hasBeenPlaced || contentRoot == null)
            return;

        hasBeenPlaced = true;

        // Position based on detected image
        Vector3 finalPosition =
            image.transform.TransformPoint(positionOffset);

        // World rotation
        Quaternion finalRotation =
            Quaternion.Euler(rotationOffset);

        GameObject anchorObject =
            new GameObject("AR_Locked_Anchor");

        anchorObject.transform.SetPositionAndRotation(
            finalPosition,
            finalRotation
        );

        lockedAnchor =
            anchorObject.AddComponent<ARAnchor>();

        Vector3 originalScale =
            contentRoot.localScale;

        contentRoot.SetParent(
            anchorObject.transform,
            false
        );

        contentRoot.localPosition =
            Vector3.zero;

        contentRoot.localRotation =
            Quaternion.identity;

        contentRoot.localScale =
            originalScale;

        contentRoot.gameObject.SetActive(true);

        Debug.Log(
            "AR Model Activated | Image = " +
            image.referenceImage.name
        );

        PlayModelActiveVO();

        onModelActivated?.Invoke();

        // One-time scanning
        if (trackedImageManager != null)
            trackedImageManager.enabled = false;
    }

    private void PlayModelActiveVO()
    {
        if (voiceSource == null ||
            modelActiveVO == null)
            return;

        voiceSource.Stop();
        voiceSource.clip = modelActiveVO;
        voiceSource.Play();
    }
}