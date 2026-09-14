using UnityEngine;

public class WorldSpaceLabel : MonoBehaviour
{
    [Header("Target Planet")]
    [SerializeField] private Transform targetObject;

    private Vector3 startLocalPosition;
    private Quaternion startWorldRotation;

    private void Start()
    {
        if (targetObject == null)
        {
            targetObject = transform.parent;
        }

        if (targetObject == null)
        {
            Debug.LogWarning(name + " : Target Object missing!");
            return;
        }

        // Canvas ki starting position ko
        // planet ke LOCAL space me save karo
        startLocalPosition =
            targetObject.InverseTransformPoint(transform.position);

        // Canvas starting me jitna seedha set hai
        // wahi world rotation save karo
        startWorldRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (targetObject == null)
            return;

        // Object rotate/move karega to
        // Canvas bhi uske around rotate/move karega
        transform.position =
            targetObject.TransformPoint(startLocalPosition);

        // Lekin Canvas khud tedha nahi hoga
        // Text seedha rahega
        transform.rotation = startWorldRotation;
    }
}