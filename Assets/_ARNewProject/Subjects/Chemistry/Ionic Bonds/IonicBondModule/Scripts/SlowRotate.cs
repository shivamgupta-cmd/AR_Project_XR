using UnityEngine;
public class SlowRotate : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float speed = 18f;
    public bool rotate = true;
    void Update() { if (rotate) transform.Rotate(axis.normalized, speed * Time.deltaTime, Space.Self); }
    public void StartRotation() => rotate = true;
    public void StopRotation() => rotate = false;
}
