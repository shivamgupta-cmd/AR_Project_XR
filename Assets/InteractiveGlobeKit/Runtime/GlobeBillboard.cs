using UnityEngine;

namespace InteractiveGlobeKit
{
    public sealed class GlobeBillboard : MonoBehaviour
    {
        [SerializeField] private bool lockUpAxis = true;

        private void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam == null) return;
            Vector3 direction = transform.position - cam.transform.position;
            if (lockUpAxis) direction.y = 0f;
            if (direction.sqrMagnitude > .0001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
