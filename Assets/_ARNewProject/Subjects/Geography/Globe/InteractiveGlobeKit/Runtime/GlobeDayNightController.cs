using UnityEngine;

namespace InteractiveGlobeKit
{
    public sealed class GlobeDayNightController : MonoBehaviour
    {
        [SerializeField] private Transform rotatingEarth;
        [SerializeField] private Transform sun;
        [SerializeField] private Renderer terminatorRenderer;
        [SerializeField] private bool animate = true;
        [SerializeField] private float simulatedDaySeconds = 24f;

        public void Configure(Transform earth, Transform sunTransform, Renderer overlay)
        {
            rotatingEarth = earth;
            sun = sunTransform;
            terminatorRenderer = overlay;
        }

        public void SetAnimated(bool value) => animate = value;

        private void Update()
        {
            if (animate && rotatingEarth != null && Application.isPlaying)
                rotatingEarth.Rotate(Vector3.up, 360f / Mathf.Max(.1f, simulatedDaySeconds) * Time.deltaTime, Space.Self);

            if (sun != null && terminatorRenderer != null && terminatorRenderer.sharedMaterial != null)
            {
                Vector3 directionToSun = (sun.position - terminatorRenderer.transform.position).normalized;
                terminatorRenderer.sharedMaterial.SetVector("_SunDirection", directionToSun);
            }
        }
    }
}
