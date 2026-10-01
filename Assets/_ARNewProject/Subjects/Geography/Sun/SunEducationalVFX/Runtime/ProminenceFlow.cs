using UnityEngine;

namespace EducationalSun.VFX
{
    /// <summary>Moves the prominence texture along its saved LineRenderer path.</summary>
    [RequireComponent(typeof(LineRenderer))]
    public sealed class ProminenceFlow : MonoBehaviour
    {
        [SerializeField] private float flowSpeed = 0.35f;

        private LineRenderer line;
        private Material runtimeMaterial;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();

            if (line.sharedMaterial != null)
            {
                runtimeMaterial = new Material(line.sharedMaterial);
                line.material = runtimeMaterial;
            }
        }

        private void Update()
        {
            if (runtimeMaterial == null)
                return;

            Vector2 offset = new Vector2(-Time.time * flowSpeed, 0f);

            if (runtimeMaterial.HasProperty("_MainTex"))
                runtimeMaterial.SetTextureOffset("_MainTex", offset);

            if (runtimeMaterial.HasProperty("_BaseMap"))
                runtimeMaterial.SetTextureOffset("_BaseMap", offset);
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null)
                Destroy(runtimeMaterial);
        }
    }
}
