using UnityEngine;

public class PlantCellAlive : MonoBehaviour
{
    [Header("MAIN PARTS")]
    public Transform nucleus;
    public Transform vacuole;

    [Header("FLOATING PARTS")]
    public Transform[] mitochondria;
    public Transform[] chloroplasts;

    [Header("WOBBLE PARTS")]
    public Transform[] erParts;
    public Transform[] golgiParts;

    [Header("NUCLEUS SETTINGS")]
    public float nucleusPulseAmount = 0.025f;
    public float nucleusPulseSpeed = 2f;

    [Header("VACUOLE SETTINGS")]
    public float vacuolePulseAmount = 0.015f;
    public float vacuolePulseSpeed = 1.2f;

    [Header("MITOCHONDRIA SETTINGS")]
    public float mitochondriaMoveAmount = 0.01f;
    public float mitochondriaMoveSpeed = 1f;
    public float mitochondriaRotateAmount = 3f;

    [Header("CHLOROPLAST SETTINGS")]
    public float chloroplastRockAmount = 2f;
    public float chloroplastRockSpeed = 0.8f;

    [Header("ER / GOLGI SETTINGS")]
    public float wobbleAmount = 1.5f;
    public float wobbleSpeed = 0.7f;

    private Vector3 nucleusOriginalScale;
    private Vector3 vacuoleOriginalScale;

    private Vector3[] mitochondriaOriginalPositions;
    private Quaternion[] mitochondriaOriginalRotations;

    private Quaternion[] chloroplastOriginalRotations;

    private Quaternion[] erOriginalRotations;
    private Quaternion[] golgiOriginalRotations;

    private float[] mitochondriaOffsets;
    private float[] chloroplastOffsets;
    private float[] erOffsets;
    private float[] golgiOffsets;

    void Start()
    {
        // Nucleus
        if (nucleus != null)
            nucleusOriginalScale = nucleus.localScale;

        // Vacuole
        if (vacuole != null)
            vacuoleOriginalScale = vacuole.localScale;

        // Mitochondria
        mitochondriaOriginalPositions = new Vector3[mitochondria.Length];
        mitochondriaOriginalRotations = new Quaternion[mitochondria.Length];
        mitochondriaOffsets = new float[mitochondria.Length];

        for (int i = 0; i < mitochondria.Length; i++)
        {
            if (mitochondria[i] != null)
            {
                mitochondriaOriginalPositions[i] = mitochondria[i].localPosition;
                mitochondriaOriginalRotations[i] = mitochondria[i].localRotation;
                mitochondriaOffsets[i] = Random.Range(0f, 10f);
            }
        }

        // Chloroplasts
        chloroplastOriginalRotations = new Quaternion[chloroplasts.Length];
        chloroplastOffsets = new float[chloroplasts.Length];

        for (int i = 0; i < chloroplasts.Length; i++)
        {
            if (chloroplasts[i] != null)
            {
                chloroplastOriginalRotations[i] = chloroplasts[i].localRotation;
                chloroplastOffsets[i] = Random.Range(0f, 10f);
            }
        }

        // ER
        erOriginalRotations = new Quaternion[erParts.Length];
        erOffsets = new float[erParts.Length];

        for (int i = 0; i < erParts.Length; i++)
        {
            if (erParts[i] != null)
            {
                erOriginalRotations[i] = erParts[i].localRotation;
                erOffsets[i] = Random.Range(0f, 10f);
            }
        }

        // Golgi
        golgiOriginalRotations = new Quaternion[golgiParts.Length];
        golgiOffsets = new float[golgiParts.Length];

        for (int i = 0; i < golgiParts.Length; i++)
        {
            if (golgiParts[i] != null)
            {
                golgiOriginalRotations[i] = golgiParts[i].localRotation;
                golgiOffsets[i] = Random.Range(0f, 10f);
            }
        }
    }

    void Update()
    {
        AnimateNucleus();
        AnimateVacuole();
        AnimateMitochondria();
        AnimateChloroplasts();
        AnimateER();
        AnimateGolgi();
    }

    void AnimateNucleus()
    {
        if (nucleus == null)
            return;

        float pulse =
            1f +
            Mathf.Sin(Time.time * nucleusPulseSpeed)
            * nucleusPulseAmount;

        nucleus.localScale = nucleusOriginalScale * pulse;
    }

    void AnimateVacuole()
    {
        if (vacuole == null)
            return;

        float pulse =
            1f +
            Mathf.Sin(Time.time * vacuolePulseSpeed + 2f)
            * vacuolePulseAmount;

        vacuole.localScale = vacuoleOriginalScale * pulse;
    }

    void AnimateMitochondria()
    {
        for (int i = 0; i < mitochondria.Length; i++)
        {
            if (mitochondria[i] == null)
                continue;

            float time =
                Time.time * mitochondriaMoveSpeed
                + mitochondriaOffsets[i];

            float x = Mathf.Sin(time) * mitochondriaMoveAmount;

            float y =
                Mathf.Sin(time * 0.7f)
                * mitochondriaMoveAmount;

            float z =
                Mathf.Cos(time * 0.8f)
                * mitochondriaMoveAmount;

            mitochondria[i].localPosition =
                mitochondriaOriginalPositions[i]
                + new Vector3(x, y, z);

            float rotation =
                Mathf.Sin(time)
                * mitochondriaRotateAmount;

            mitochondria[i].localRotation =
                mitochondriaOriginalRotations[i]
                * Quaternion.Euler(
                    rotation,
                    rotation * 0.5f,
                    -rotation
                );
        }
    }

    void AnimateChloroplasts()
    {
        for (int i = 0; i < chloroplasts.Length; i++)
        {
            if (chloroplasts[i] == null)
                continue;

            float time =
                Time.time * chloroplastRockSpeed
                + chloroplastOffsets[i];

            float angle =
                Mathf.Sin(time)
                * chloroplastRockAmount;

            chloroplasts[i].localRotation =
                chloroplastOriginalRotations[i]
                * Quaternion.Euler(
                    angle,
                    angle * 0.3f,
                    -angle
                );
        }
    }

    void AnimateER()
    {
        for (int i = 0; i < erParts.Length; i++)
        {
            if (erParts[i] == null)
                continue;

            float time =
                Time.time * wobbleSpeed
                + erOffsets[i];

            float angle =
                Mathf.Sin(time)
                * wobbleAmount;

            erParts[i].localRotation =
                erOriginalRotations[i]
                * Quaternion.Euler(
                    0,
                    angle,
                    angle * 0.5f
                );
        }
    }

    void AnimateGolgi()
    {
        for (int i = 0; i < golgiParts.Length; i++)
        {
            if (golgiParts[i] == null)
                continue;

            float time =
                Time.time * (wobbleSpeed * 0.8f)
                + golgiOffsets[i];

            float angle =
                Mathf.Sin(time)
                * wobbleAmount;

            golgiParts[i].localRotation =
                golgiOriginalRotations[i]
                * Quaternion.Euler(
                    angle * 0.5f,
                    0,
                    angle
                );
        }
    }
}