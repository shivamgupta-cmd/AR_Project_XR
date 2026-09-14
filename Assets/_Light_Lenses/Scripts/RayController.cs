using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class RayController : MonoBehaviour
{
    public enum LensType
    {
        GlassPlate,
        Convex,
        Concave
    }

    [Header("Mode")]
    public LensType lensType;

    [Header("Points")]
    public Transform startPoint;
    public Transform lensPoint;
    public Transform endPoint;

    [Header("Dotted Ray")]
    public LineRenderer dottedLR;
    public Transform dotsPoint;

    [Header("Convex Focus")]
    public Transform focusPoint;
    public GameObject focusPointObj;

    [Header("Concave Virtual Focus")]
    public Transform virtualFocus;
    public GameObject virtualFocusPoint;

    [Header("Events")]
    public UnityEvent OnGlassPlateComplete;
    public UnityEvent OnConvexComplete;
    public UnityEvent OnConcaveComplete;

    [Header("Animation")]
    private float moveSpeed = 0.5f;


    private LineRenderer lr;

    void Start()
    {
        if (focusPointObj != null)
            focusPointObj.SetActive(false);

        if (virtualFocusPoint != null)
            virtualFocusPoint.SetActive(false);

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 4;

        if (dottedLR != null)
            dottedLR.positionCount = 3;

        PlayRay();
    }

    public void PlayRay()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateRay());
    }

    IEnumerator AnimateRay()
    {
        Vector3 p0 = startPoint.position;
        Vector3 p1 = lensPoint.position;
        Vector3 p2;
        Vector3 p3 = endPoint.position;

        if (lensType == LensType.GlassPlate)
        {
            Vector3 dir = (p1 - p0).normalized;
            p2 = p1 + dir * 0.2f;
        }
        else if (lensType == LensType.Convex)
        {
            p2 = focusPoint.position;
        }
        else
        {
            p2 = endPoint.position;

            if (dottedLR != null)
            {
                dottedLR.enabled = false;

                dottedLR.SetPosition(0, lensPoint.position);
                dottedLR.SetPosition(1, lensPoint.position);
                dottedLR.SetPosition(2, lensPoint.position);
            }
        }

        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;

            float value = Mathf.SmoothStep(0, 1, t);

            if (value <= 0.33f)
            {
                float tt = value / 0.33f;

                lr.SetPosition(0, p0);
                lr.SetPosition(1, Vector3.Lerp(p0, p1, tt));

                Vector3 current = lr.GetPosition(1);

                lr.SetPosition(2, current);
                lr.SetPosition(3, current);
            }
            else if (value <= 0.66f)
            {
                float tt = (value - 0.33f) / 0.33f;

                lr.SetPosition(0, p0);
                lr.SetPosition(1, p1);
                lr.SetPosition(2, Vector3.Lerp(p1, p2, tt));

                Vector3 current = lr.GetPosition(2);

                lr.SetPosition(3, current);
            }
            else
            {

                float tt = (value - 0.66f) / 0.34f;

                if (lensType == LensType.Concave)
                {
                    lr.SetPosition(0, p0);
                    lr.SetPosition(1, p1);
                    lr.SetPosition(2, p2);
                    lr.SetPosition(3, Vector3.Lerp(p2, p3, tt));
                }
                else
                {
                    lr.SetPosition(0, p0);
                    lr.SetPosition(1, p1);
                    lr.SetPosition(2, p2);
                    lr.SetPosition(3, Vector3.Lerp(p2, p3, tt));
                }
                   // StartCoroutine(AnimateDottedRay());
            }

            yield return null;
        }

        if (lensType == LensType.GlassPlate)
        {
            OnGlassPlateComplete?.Invoke();
        }
        if (lensType == LensType.Convex)
        {
            ShowFocusPoint();
            OnConvexComplete?.Invoke();
        }
        if (lensType == LensType.Concave)
        {
            yield return StartCoroutine(AnimateDottedRay());

            ShowVirtualFocusPoint();

            OnConcaveComplete?.Invoke();
        }

    }

    public void ShowFocusPoint()
    {
        if (focusPointObj != null)
            focusPointObj.SetActive(true);
    }

    public void ShowVirtualFocusPoint()
    {
        if (virtualFocusPoint != null)
            virtualFocusPoint.SetActive(true);
    }

    IEnumerator AnimateDottedRay()
    {
        if (dottedLR == null)
            yield break;

        dottedLR.enabled = true;

        Vector3 p0 = lensPoint.position;
        Vector3 p1 = virtualFocus.position;
        Vector3 p2 = dotsPoint.position;

        dottedLR.SetPosition(0, p0);
        dottedLR.SetPosition(1, p0);
        dottedLR.SetPosition(2, p0);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;

            float value = Mathf.SmoothStep(0, 1, t);

            if (value < 0.5f)
            {
                float tt = value / 0.5f;

                dottedLR.SetPosition(1, Vector3.Lerp(p0, p1, tt));
                dottedLR.SetPosition(2, dottedLR.GetPosition(1));
            }
            else
            {
                float tt = (value - 0.5f) / 0.5f;

                dottedLR.SetPosition(1, p1);
                dottedLR.SetPosition(2, Vector3.Lerp(p1, p2, tt));
            }

            yield return null;
        }

        dottedLR.SetPosition(0, p0);
        dottedLR.SetPosition(1, p1);
        dottedLR.SetPosition(2, p2);
       
    }

}