using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Complete Ionic Bond (NaCl) sequence controller designed for Timeline Signals / UnityEvents.
/// The Editor builder auto-finds the objects inside Iconic_Bond.fbx.
/// </summary>
public class IonicBondModuleController : MonoBehaviour
{
    [Header("FBX Parts - auto assigned by builder")]
    public GameObject sodiumAtom;
    public GameObject chlorineAtom;
    public GameObject sodiumCation;
    public GameObject chlorideAnion;
    public GameObject crystalLattice;
    public Transform sodiumElectron;

    [Header("Transfer Target")]
    [Tooltip("Put this close to the outer shell of chlorine where the transferred electron should land.")]
    public Transform electronTarget;

    [Header("Timing")]
    public float electronHighlightTime = 0.75f;
    public float electronTransferTime = 1.25f;
    public float ionFormDelay = 0.25f;
    public float attractionTime = 1.1f;
    public float latticeBuildTime = 2.2f;

    [Header("Electron Flight")]
    public float arcHeight = 0.18f;
    public AnimationCurve transferEase = AnimationCurve.EaseInOut(0,0,1,1);
    public TrailRenderer electronTrail;

   


    [Header("FX")]
    public GameObject electronHighlightGlow;
    public ParticleSystem transferImpactFX;
   
   

    [Header("Playback")]
    public bool playOnStart = false;

    Transform electronOriginalParent;
    Vector3 electronOriginalLocalPosition;
    Quaternion electronOriginalLocalRotation;
    Vector3 cationStartPos;
    Quaternion cationStartRot;
    Vector3 anionStartPos;
    Quaternion anionStartRot;
    Coroutine running;
    List<GameObject> latticePieces = new List<GameObject>();

    void Awake()
    {
        CacheInitialState();
        if (playOnStart) PlayFullSequence();
        else ResetModule();
    }

    void CacheInitialState()
    {
        if (sodiumElectron)
        {
            electronOriginalParent = sodiumElectron.parent;
            electronOriginalLocalPosition = sodiumElectron.localPosition;
            electronOriginalLocalRotation = sodiumElectron.localRotation;
        }
        if (sodiumCation)
        {
            cationStartPos = sodiumCation.transform.localPosition;
            cationStartRot = sodiumCation.transform.localRotation;
        }
        if (chlorideAnion)
        {
            anionStartPos = chlorideAnion.transform.localPosition;
            anionStartRot = chlorideAnion.transform.localRotation;
        }
        CacheLatticePieces();
    }

    void CacheLatticePieces()
    {
        latticePieces.Clear();
        if (!crystalLattice) return;
        foreach (Transform t in crystalLattice.GetComponentsInChildren<Transform>(true))
        {
            if (t == crystalLattice.transform) continue;
            Renderer r = t.GetComponent<Renderer>();
            if (r) latticePieces.Add(t.gameObject);
        }
    }

    public void ResetModule()
    {
        StopRunning();

        SetActive(sodiumAtom, true);
        SetActive(chlorineAtom, true);
        SetActive(sodiumCation, false);
        SetActive(chlorideAnion, false);
        SetActive(crystalLattice, false);
        
        SetActive(electronHighlightGlow, false);

       
        StopParticle(transferImpactFX);
       

        if (sodiumElectron)
        {
            sodiumElectron.SetParent(electronOriginalParent, false);
            sodiumElectron.localPosition = electronOriginalLocalPosition;
            sodiumElectron.localRotation = electronOriginalLocalRotation;
            sodiumElectron.gameObject.SetActive(true);
        }
        if (electronTrail)
        {
            electronTrail.Clear();
            electronTrail.emitting = false;
        }
        if (sodiumCation)
        {
            sodiumCation.transform.localPosition = cationStartPos;
            sodiumCation.transform.localRotation = cationStartRot;
        }
        if (chlorideAnion)
        {
            chlorideAnion.transform.localPosition = anionStartPos;
            chlorideAnion.transform.localRotation = anionStartRot;
        }
    }

    public void ShowAtoms()
    {
        SetActive(sodiumAtom, true);
        SetActive(chlorineAtom, true);
        SetActive(sodiumCation, false);
        SetActive(chlorideAnion, false);
        SetActive(crystalLattice, false);
    }

    public void HighlightElectron()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(HighlightElectronRoutine());
    }

    IEnumerator HighlightElectronRoutine()
    {
        SetActive(electronHighlightGlow, true);
        float t = 0;
        while (t < electronHighlightTime)
        {
            t += Time.deltaTime;
            if (electronHighlightGlow && sodiumElectron)
            {
                electronHighlightGlow.transform.position = sodiumElectron.position;
                float pulse = 1f + Mathf.Sin(t * 10f) * 0.12f;
                electronHighlightGlow.transform.localScale = new Vector3(
    pulse * 0.09f,
    pulse * 0.09f,
    pulse * 0.09f
);
            }
            yield return null;
        }
        running = null;
    }

    public void TransferElectron()
    {
        if (!sodiumElectron || !electronTarget) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(TransferElectronRoutine());
    }

    IEnumerator TransferElectronRoutine()
    {
        SetActive(electronHighlightGlow, false);

        Vector3 start = sodiumElectron.position;

        // Detach from Sodium while travelling
        sodiumElectron.SetParent(transform, true);

        if (electronTrail)
        {
            electronTrail.Clear();
            electronTrail.emitting = true;
        }

        float t = 0f;

        while (t < electronTransferTime)
        {
            t += Time.deltaTime;

            float n = Mathf.Clamp01(t / electronTransferTime);
            float e = transferEase.Evaluate(n);

            Vector3 pos = Vector3.Lerp(
                start,
                electronTarget.position,
                e
            );

            // Arc movement
            pos += Vector3.up *
                   Mathf.Sin(e * Mathf.PI) *
                   arcHeight;

            sodiumElectron.position = pos;

            sodiumElectron.Rotate(
                Vector3.up,
                360f * Time.deltaTime,
                Space.World
            );

            yield return null;
        }

        // ==========================================
        // ELECTRON REACHED CHLORINE
        // ==========================================

        sodiumElectron.position = electronTarget.position;

        // Make electron child of Electron Target
        sodiumElectron.SetParent(electronTarget, true);

        // Keep it exactly at target position
        sodiumElectron.localPosition = Vector3.zero;

        // Optional - match target rotation
        sodiumElectron.localRotation = Quaternion.identity;

        // Stop trail
        if (electronTrail)
        {
            electronTrail.emitting = false;
        }

        // Impact FX
        if (transferImpactFX)
        {
            transferImpactFX.transform.position =
                electronTarget.position;

            transferImpactFX.Play();
        }

        running = null;
    }

  


   

  

    public void BuildCrystalLattice()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(BuildLatticeRoutine());
    }

    IEnumerator BuildLatticeRoutine()
    {
        SetActive(sodiumCation, false);
        SetActive(chlorideAnion, false);
       
        if (latticePieces.Count == 0) CacheLatticePieces();

        foreach (var p in latticePieces) if (p) p.SetActive(false);
        float delay = latticePieces.Count > 0 ? latticeBuildTime / latticePieces.Count : 0f;
        foreach (var p in latticePieces)
        {
            if (!p) continue;
            p.SetActive(true);
            p.transform.localScale = Vector3.zero;
            float d = Mathf.Min(0.12f, Mathf.Max(0.025f, delay));
            float t = 0;
            while (t < d)
            {
                t += Time.deltaTime;
                float n = Mathf.Clamp01(t / d);
                p.transform.localScale = Vector3.one * Mathf.SmoothStep(0,1,n);
                yield return null;
            }
            p.transform.localScale = Vector3.one;
        }
      
        running = null;
    }

    public void ShowFinalLattice()
    {
        StopRunning();
        SetActive(sodiumAtom, false);
        SetActive(chlorineAtom, false);
        SetActive(sodiumCation, false);
        SetActive(chlorideAnion, false);
       
        foreach (var p in latticePieces)
        {
            if (!p) continue;
            p.SetActive(true);
            p.transform.localScale = Vector3.one;
        }
    }

    public void PlayFullSequence()
    {
        StopRunning();
        running = StartCoroutine(FullSequenceRoutine());
    }

    IEnumerator FullSequenceRoutine()
    {
        ResetModule();
        yield return new WaitForSeconds(0.35f);
        ShowAtoms();
        yield return new WaitForSeconds(1f);
        yield return HighlightElectronRoutine();
        yield return TransferElectronRoutine();
        //yield return FormIonsRoutine();
        //yield return new WaitForSeconds(0.5f);
        //yield return AttractionRoutine();
        //yield return new WaitForSeconds(0.5f);
        //yield return BuildLatticeRoutine();
        running = null;
    }

    void StopRunning()
    {
        if (running != null)
        {
            StopCoroutine(running);
            running = null;
        }
    }

    static void SetActive(GameObject go, bool value) { if (go) go.SetActive(value); }
    static void StopParticle(ParticleSystem ps) { if (ps) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
}
