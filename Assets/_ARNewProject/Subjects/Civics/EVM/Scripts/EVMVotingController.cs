//using System.Collections;
//using UnityEngine;

//public class EVMVotingController : MonoBehaviour
//{
//    [System.Serializable]
//    public class CandidateData
//    {
//        public string candidateName;

//        [Tooltip("VVPAT slip shown when this candidate is selected.")]
//        public GameObject vvpatSlip;

//        [Tooltip("Red LED shown beside this candidate.")]
//        public GameObject redLED;
//    }

//    [Header("Candidates")]
//    [Tooltip("0-6 = Candidate A-G, 7 = NOTA")]
//    [SerializeField] private CandidateData[] candidates = new CandidateData[8];

//    [Header("VVPAT Settings")]
//    [Tooltip("How long the slip stays visible before dropping.")]
//    [SerializeField] private float slipDisplayDuration = 3f;

//    [Tooltip("How far above its normal position the slip starts.")]
//    [SerializeField] private float slipStartOffset = 0.08f;

//    [Tooltip("How far below its normal position the slip drops.")]
//    [SerializeField] private float slipDropDistance = 0.12f;

//    [Tooltip("Time taken for slip to slide into view.")]
//    [SerializeField] private float slideInDuration = 0.5f;

//    [Tooltip("Time taken for slip to drop down.")]
//    [SerializeField] private float slideOutDuration = 0.5f;

//    [Header("Audio")]
//    [SerializeField] private AudioSource audioSource;
//    [SerializeField] private AudioClip voteBeep;

//    [Header("Debug")]
//    [SerializeField] private bool voteLocked = false;

//    private Coroutine votingCoroutine;

//    // Store original local positions of all slips
//    private Vector3[] slipOriginalPositions;


//    private void Awake()
//    {
//        CacheSlipPositions();
//    }


//    private void Start()
//    {
//        ResetVotingMachine();
//    }


//    // =========================================================
//    // CACHE ORIGINAL SLIP POSITIONS
//    // =========================================================

//    private void CacheSlipPositions()
//    {
//        slipOriginalPositions = new Vector3[candidates.Length];

//        for (int i = 0; i < candidates.Length; i++)
//        {
//            if (candidates[i] != null &&
//                candidates[i].vvpatSlip != null)
//            {
//                slipOriginalPositions[i] =
//                    candidates[i].vvpatSlip.transform.localPosition;
//            }
//        }
//    }


//    // =========================================================
//    // CAST VOTE
//    // =========================================================

//    public void CastVote(int candidateIndex)
//    {
//        if (voteLocked)
//        {
//            Debug.Log("Vote already cast. Voting is locked.");
//            return;
//        }

//        if (candidateIndex < 0 ||
//            candidateIndex >= candidates.Length)
//        {
//            Debug.LogWarning("Invalid candidate index: " + candidateIndex);
//            return;
//        }

//        CandidateData selectedCandidate = candidates[candidateIndex];

//        if (selectedCandidate == null)
//            return;


//        // Lock voting immediately
//        voteLocked = true;


//        // -----------------------------------------
//        // 1. BEEP IMMEDIATELY
//        // -----------------------------------------

//        PlayVoteBeep();


//        // -----------------------------------------
//        // 2. TURN ON RED LED
//        // -----------------------------------------

//        if (selectedCandidate.redLED != null)
//        {
//            selectedCandidate.redLED.SetActive(true);
//        }


//        // -----------------------------------------
//        // 3. START VVPAT SLIP
//        // -----------------------------------------

//        votingCoroutine = StartCoroutine(
//            VotingSequence(
//                selectedCandidate,
//                candidateIndex
//            )
//        );
//    }


//    // =========================================================
//    // VVPAT SEQUENCE
//    // =========================================================

//    private IEnumerator VotingSequence(
//        CandidateData selectedCandidate,
//        int candidateIndex)
//    {
//        GameObject slip = selectedCandidate.vvpatSlip;

//        if (slip == null)
//            yield break;


//        Transform slipTransform = slip.transform;

//        Vector3 normalPosition =
//            slipOriginalPositions[candidateIndex];


//        // =====================================================
//        // START POSITION - ABOVE WINDOW
//        // =====================================================

//        Vector3 startPosition =
//            normalPosition + Vector3.up * slipStartOffset;


//        slipTransform.localPosition = startPosition;

//        slip.SetActive(true);


//        // =====================================================
//        // SLIDE FROM TOP INTO VVPAT WINDOW
//        // =====================================================

//        yield return MoveSlip(
//            slipTransform,
//            startPosition,
//            normalPosition,
//            slideInDuration
//        );


//        // =====================================================
//        // KEEP SLIP VISIBLE
//        // =====================================================

//        yield return new WaitForSeconds(
//            slipDisplayDuration
//        );


//        // =====================================================
//        // DROP SLIP DOWN
//        // =====================================================

//        Vector3 dropPosition =
//            normalPosition +
//            Vector3.down * slipDropDistance;


//        yield return MoveSlip(
//            slipTransform,
//            normalPosition,
//            dropPosition,
//            slideOutDuration
//        );


//        // =====================================================
//        // HIDE
//        // =====================================================

//        slip.SetActive(false);


//        // Restore original position
//        slipTransform.localPosition =
//            normalPosition;


//        votingCoroutine = null;

//        Debug.Log("VVPAT sequence completed.");
//    }


//    // =========================================================
//    // MOVE SLIP
//    // =========================================================

//    private IEnumerator MoveSlip(
//        Transform target,
//        Vector3 from,
//        Vector3 to,
//        float duration)
//    {
//        if (duration <= 0f)
//        {
//            target.localPosition = to;
//            yield break;
//        }

//        float timer = 0f;

//        while (timer < duration)
//        {
//            timer += Time.deltaTime;

//            float t = Mathf.Clamp01(
//                timer / duration
//            );

//            // Smooth movement
//            t = Mathf.SmoothStep(0f, 1f, t);

//            target.localPosition =
//                Vector3.Lerp(from, to, t);

//            yield return null;
//        }

//        target.localPosition = to;
//    }


//    // =========================================================
//    // AUDIO
//    // =========================================================

//    private void PlayVoteBeep()
//    {
//        if (audioSource != null &&
//            voteBeep != null)
//        {
//            audioSource.PlayOneShot(voteBeep);
//        }
//    }


//    // =========================================================
//    // RESET
//    // =========================================================

//    [ContextMenu("Reset Voting Machine")]
//    public void ResetVotingMachine()
//    {
//        if (votingCoroutine != null)
//        {
//            StopCoroutine(votingCoroutine);
//            votingCoroutine = null;
//        }

//        voteLocked = false;


//        for (int i = 0; i < candidates.Length; i++)
//        {
//            CandidateData candidate = candidates[i];

//            if (candidate == null)
//                continue;


//            // Slip reset
//            if (candidate.vvpatSlip != null)
//            {
//                candidate.vvpatSlip.SetActive(false);

//                if (slipOriginalPositions != null &&
//                    i < slipOriginalPositions.Length)
//                {
//                    candidate.vvpatSlip.transform.localPosition =
//                        slipOriginalPositions[i];
//                }
//            }


//            // LED reset
//            if (candidate.redLED != null)
//            {
//                candidate.redLED.SetActive(false);
//            }
//        }

//        Debug.Log("EVM Reset - Ready for voting.");
//    }
//}


using System.Collections;
using UnityEngine;

public class EVMVotingController : MonoBehaviour
{
    [System.Serializable]
    public class CandidateData
    {
        public string candidateName;

        [Tooltip("VVPAT slip shown when this candidate is selected.")]
        public GameObject vvpatSlip;

        [Tooltip("Red LED shown beside this candidate.")]
        public GameObject redLED;
    }

    [Header("Candidates")]
    [Tooltip("0-6 = Candidate A-G, 7 = NOTA")]
    [SerializeField] private CandidateData[] candidates = new CandidateData[8];


    [Header("VVPAT Settings")]

    [Tooltip("How long the slip stays visible.")]
    [SerializeField] private float slipDisplayDuration = 7f;

    [Tooltip("How far above its normal position the slip starts.")]
    [SerializeField] private float slipStartOffset = 0.08f;

    [Tooltip("How far below its normal position the slip drops.")]
    [SerializeField] private float slipDropDistance = 0.12f;

    [Tooltip("Time taken for slip to slide into view.")]
    [SerializeField] private float slideInDuration = 0.5f;

    [Tooltip("Time taken for slip to drop down.")]
    [SerializeField] private float slideOutDuration = 0.5f;


    [Header("Audio")]

    [SerializeField] private AudioSource audioSource;

    [Tooltip("Beep audio played immediately after button press.")]
    [SerializeField] private AudioClip voteBeep;

    [Tooltip("VO played after the beep finishes.")]
    [SerializeField] private AudioClip afterBeepAudio;

    [Tooltip("Final completion VO.")]
    [SerializeField] private AudioClip completedAudio;

    [Tooltip("Delay after slip disappears before Complete VO.")]
    [SerializeField] private float completedAudioDelay = 2f;


    [Header("Debug")]
    [SerializeField] private bool voteLocked = false;


    private Coroutine votingCoroutine;

    private Vector3[] slipOriginalPositions;


    private void Awake()
    {
        CacheSlipPositions();
    }


    private void Start()
    {
        ResetVotingMachine();
    }


    // =========================================================
    // CACHE ORIGINAL SLIP POSITIONS
    // =========================================================

    private void CacheSlipPositions()
    {
        slipOriginalPositions =
            new Vector3[candidates.Length];

        for (int i = 0; i < candidates.Length; i++)
        {
            if (candidates[i] != null &&
                candidates[i].vvpatSlip != null)
            {
                slipOriginalPositions[i] =
                    candidates[i]
                    .vvpatSlip
                    .transform
                    .localPosition;
            }
        }
    }


    // =========================================================
    // CAST VOTE
    // =========================================================

    public void CastVote(int candidateIndex)
    {
        if (voteLocked)
        {
            Debug.Log(
                "Vote already cast. Voting is locked."
            );

            return;
        }


        if (candidateIndex < 0 ||
            candidateIndex >= candidates.Length)
        {
            Debug.LogWarning(
                "Invalid candidate index: " +
                candidateIndex
            );

            return;
        }


        CandidateData selectedCandidate =
            candidates[candidateIndex];


        if (selectedCandidate == null)
            return;


        // Lock immediately
        voteLocked = true;


        // Start complete sequence
        votingCoroutine = StartCoroutine(
            VotingSequence(
                selectedCandidate,
                candidateIndex
            )
        );
    }


    // =========================================================
    // COMPLETE VOTING SEQUENCE
    // =========================================================

    private IEnumerator VotingSequence(
        CandidateData selectedCandidate,
        int candidateIndex)
    {
        // =====================================================
        // BUTTON PRESSED
        // LED ON IMMEDIATELY
        // =====================================================

        if (selectedCandidate.redLED != null)
        {
            selectedCandidate.redLED.SetActive(true);
        }


        // =====================================================
        // BEEP STARTS IMMEDIATELY
        // =====================================================

        if (audioSource != null &&
            voteBeep != null)
        {
            audioSource.PlayOneShot(voteBeep);
        }


        // =====================================================
        // SLIP ANIMATION STARTS AT SAME TIME
        // =====================================================

        GameObject slip =
            selectedCandidate.vvpatSlip;


        if (slip == null)
        {
            Debug.LogWarning(
                "No VVPAT slip assigned."
            );

            yield break;
        }


        Transform slipTransform =
            slip.transform;


        Vector3 normalPosition =
            slipOriginalPositions[candidateIndex];


        Vector3 startPosition =
            normalPosition +
            Vector3.up * slipStartOffset;


        slipTransform.localPosition =
            startPosition;


        slip.SetActive(true);


        // Slide slip into window
        yield return MoveSlip(
            slipTransform,
            startPosition,
            normalPosition,
            slideInDuration
        );


        // =====================================================
        // WAIT UNTIL BEEP IS COMPLETELY FINISHED
        // =====================================================

        // Slip has already entered the VVPAT window.
        // If beep is longer than slide animation,
        // wait only for the remaining beep duration.

        float remainingBeepTime = 0f;


        if (voteBeep != null)
        {
            remainingBeepTime =
                voteBeep.length -
                slideInDuration;
        }


        if (remainingBeepTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingBeepTime
            );
        }


        // =====================================================
        // AFTER BEEP VO STARTS
        // =====================================================

        if (audioSource != null &&
            afterBeepAudio != null)
        {
            audioSource.PlayOneShot(
                afterBeepAudio
            );
        }


        // =====================================================
        // SLIP REMAINS VISIBLE FOR 7 SECONDS
        //
        // AFTER BEEP VO PLAYS DURING THESE 7 SECONDS
        // =====================================================

        yield return new WaitForSeconds(
            slipDisplayDuration
        );


        // =====================================================
        // SLIP DROPS
        // =====================================================

        Vector3 dropPosition =
            normalPosition +
            Vector3.down * slipDropDistance;


        yield return MoveSlip(
            slipTransform,
            normalPosition,
            dropPosition,
            slideOutDuration
        );


        // =====================================================
        // SLIP DISAPPEARS
        // =====================================================

        slip.SetActive(false);


        // Restore original position
        slipTransform.localPosition =
            normalPosition;


        // =====================================================
        // WAIT BEFORE COMPLETE VO
        // =====================================================

        yield return new WaitForSeconds(
            completedAudioDelay
        );


        // =====================================================
        // COMPLETE VO
        // =====================================================

        if (audioSource != null &&
            completedAudio != null)
        {
            audioSource.PlayOneShot(
                completedAudio
            );
        }


        votingCoroutine = null;


        Debug.Log(
            "EVM voting sequence completed."
        );
    }


    // =========================================================
    // MOVE SLIP
    // =========================================================

    private IEnumerator MoveSlip(
        Transform target,
        Vector3 from,
        Vector3 to,
        float duration)
    {
        if (duration <= 0f)
        {
            target.localPosition = to;
            yield break;
        }


        float timer = 0f;


        while (timer < duration)
        {
            timer += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    timer / duration
                );


            t = Mathf.SmoothStep(
                0f,
                1f,
                t
            );


            target.localPosition =
                Vector3.Lerp(
                    from,
                    to,
                    t
                );


            yield return null;
        }


        target.localPosition = to;
    }


    // =========================================================
    // RESET
    // =========================================================

    [ContextMenu("Reset Voting Machine")]
    public void ResetVotingMachine()
    {
        if (votingCoroutine != null)
        {
            StopCoroutine(votingCoroutine);

            votingCoroutine = null;
        }


        if (audioSource != null)
        {
            audioSource.Stop();
        }


        voteLocked = false;


        for (int i = 0;
             i < candidates.Length;
             i++)
        {
            CandidateData candidate =
                candidates[i];


            if (candidate == null)
                continue;


            // Reset Slip
            if (candidate.vvpatSlip != null)
            {
                candidate.vvpatSlip.SetActive(
                    false
                );


                if (slipOriginalPositions != null &&
                    i < slipOriginalPositions.Length)
                {
                    candidate
                        .vvpatSlip
                        .transform
                        .localPosition =
                        slipOriginalPositions[i];
                }
            }


            // Reset LED
            if (candidate.redLED != null)
            {
                candidate.redLED.SetActive(
                    false
                );
            }
        }


        Debug.Log(
            "EVM Reset - Ready for voting."
        );
    }
}