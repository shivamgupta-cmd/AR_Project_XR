using UnityEngine;

public class InfinityMovingPoint : MonoBehaviour
{
    public InfinityLineAnimation infinity;

    public float speed = 1.5f;
    public float zOffset = -0.02f;

    private float t;

    private void Update()
    {
        if (infinity == null)
            return;

        t += speed * Time.deltaTime;

        if (t >= Mathf.PI * 2f)
            t -= Mathf.PI * 2f;

        Vector3 position = infinity.GetPoint(t);

        position.z += zOffset;

        transform.localPosition = position;
    }
}