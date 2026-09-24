using UnityEngine;
using UnityEngine.Events;

namespace GlobeRegionHighlight
{
    public enum GlobeRegion
    {
        None = 0,
        Asia = 1,
        Africa = 2,
        NorthAmerica = 3,
        SouthAmerica = 4,
        Europe = 5,
        AustraliaOceania = 6,
        Antarctica = 7,
        PacificOcean = 8,
        AtlanticOcean = 9,
        IndianOcean = 10,
        ArcticOcean = 11,
        SouthernOcean = 12
    }

    /// <summary>
    /// Controls the selected region on the Educational Globe material.
    /// All named public methods can be connected directly to Unity UI buttons.
    /// </summary>
    public sealed class GlobeRegionHighlighter : MonoBehaviour
    {
        [SerializeField] private Renderer globeRenderer;
        [SerializeField] private GlobeRegion startRegion = GlobeRegion.None;
        [SerializeField] private Color highlightColor = new Color(0.1f, 0.9f, 1f, 1f);
        [SerializeField, Range(0f, 1f)] private float highlightStrength = 0.8f;
        [SerializeField] private UnityEvent onRegionChanged;

        private Material runtimeMaterial;
        public GlobeRegion CurrentRegion { get; private set; }

        public void Configure(Renderer targetRenderer)
        {
            globeRenderer = targetRenderer;
        }

        private void Awake()
        {
            if (globeRenderer == null)
                globeRenderer = GetComponent<Renderer>();

            if (globeRenderer != null)
                runtimeMaterial = globeRenderer.material;

            Highlight(startRegion);
        }

        public void Highlight(GlobeRegion region)
        {
            CurrentRegion = region;
            if (runtimeMaterial == null && globeRenderer != null)
                runtimeMaterial = globeRenderer.material;
            if (runtimeMaterial == null) return;

            runtimeMaterial.SetFloat("_SelectedRegion", (int)region);
            runtimeMaterial.SetColor("_HighlightColor", highlightColor);
            runtimeMaterial.SetFloat("_HighlightStrength", highlightStrength);
            onRegionChanged?.Invoke();
        }

        public void HighlightById(int regionId) =>
            Highlight((GlobeRegion)Mathf.Clamp(regionId, 0, 12));

        public void ClearHighlight() => Highlight(GlobeRegion.None);
        public void HighlightAsia() => Highlight(GlobeRegion.Asia);
        public void HighlightAfrica() => Highlight(GlobeRegion.Africa);
        public void HighlightNorthAmerica() => Highlight(GlobeRegion.NorthAmerica);
        public void HighlightSouthAmerica() => Highlight(GlobeRegion.SouthAmerica);
        public void HighlightEurope() => Highlight(GlobeRegion.Europe);
        public void HighlightAustraliaOceania() => Highlight(GlobeRegion.AustraliaOceania);
        public void HighlightAntarctica() => Highlight(GlobeRegion.Antarctica);
        public void HighlightPacificOcean() => Highlight(GlobeRegion.PacificOcean);
        public void HighlightAtlanticOcean() => Highlight(GlobeRegion.AtlanticOcean);
        public void HighlightIndianOcean() => Highlight(GlobeRegion.IndianOcean);
        public void HighlightArcticOcean() => Highlight(GlobeRegion.ArcticOcean);
        public void HighlightSouthernOcean() => Highlight(GlobeRegion.SouthernOcean);

        public void SetHighlightColor(Color color)
        {
            highlightColor = color;
            if (runtimeMaterial != null)
                runtimeMaterial.SetColor("_HighlightColor", color);
        }

        public void SetHighlightStrength(float value)
        {
            highlightStrength = Mathf.Clamp01(value);
            if (runtimeMaterial != null)
                runtimeMaterial.SetFloat("_HighlightStrength", highlightStrength);
        }
    }
}
