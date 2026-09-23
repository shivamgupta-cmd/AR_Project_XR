using UnityEngine;

namespace InteractiveGlobeKit
{
    public sealed class GlobeSeasonsDemonstration : MonoBehaviour
    {
        [SerializeField] private Transform earth;
        [SerializeField] private Transform orbitCenter;
        [SerializeField] private float orbitDuration = 30f;
        [SerializeField] private bool animate = true;
        private Quaternion fixedAxisDirection;

        private void Start()
        {
            if (earth != null) fixedAxisDirection = earth.rotation;
        }

        private void Update()
        {
            if (!animate || earth == null || orbitCenter == null) return;
            earth.RotateAround(orbitCenter.position, Vector3.up, 360f / Mathf.Max(.1f, orbitDuration) * Time.deltaTime);
            earth.rotation = fixedAxisDirection;
        }
    }
}
