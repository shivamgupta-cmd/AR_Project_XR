using UnityEngine;

public class KeepTransformZero : MonoBehaviour
{
    void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}