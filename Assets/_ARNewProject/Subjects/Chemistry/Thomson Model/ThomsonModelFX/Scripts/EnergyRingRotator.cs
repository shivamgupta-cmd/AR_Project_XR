using UnityEngine;
public class EnergyRingRotator : MonoBehaviour
{
    public Vector3 axis = Vector3.up;
    public float speed = 25f;
    public float pulseAmount = 0.04f;
    public float pulseSpeed = 1.5f;
    Vector3 startScale;
    void Awake(){ startScale = transform.localScale; }
    void Update(){ transform.Rotate(axis.normalized, speed * Time.deltaTime, Space.Self); transform.localScale = startScale * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount); }
}
