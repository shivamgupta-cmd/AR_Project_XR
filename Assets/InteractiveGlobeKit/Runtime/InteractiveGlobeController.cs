using System.Collections.Generic;
using UnityEngine;

namespace InteractiveGlobeKit
{
    [ExecuteAlways]
    public sealed class InteractiveGlobeController : MonoBehaviour
    {
        [System.Serializable]
        public sealed class GlobeLocation
        {
            public string name = "New Delhi";
            [Range(-90, 90)] public float latitude = 28.6139f;
            [Range(-180, 180)] public float longitude = 77.209f;
            [TextArea] public string description = "Capital of India";
            public Color color = new Color(.15f, .9f, 1f, 1f);
        }

        [Header("Required")]
        [SerializeField] private Transform globeSurface;
        [SerializeField] private Transform globeRotationTarget;
        [SerializeField] private float radius = 1f;
        [SerializeField] private float longitudeOffset;
        [SerializeField, HideInInspector] private Vector3 surfaceCenter;

        [Header("Materials")]
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Material equatorMaterial;
        [SerializeField] private Material primeMeridianMaterial;
        [SerializeField] private Material axisMaterial;
        [SerializeField] private Material specialCircleMaterial;
        [SerializeField] private Material pinMaterial;
        [SerializeField] private Material hemisphereMaterial;
        [SerializeField] private Material dayNightMaterial;
        [SerializeField] private Material particleMaterial;

        [Header("Grid")]
        [SerializeField, Range(10, 45)] private int latitudeInterval = 15;
        [SerializeField, Range(10, 45)] private int longitudeInterval = 15;
        [SerializeField, Range(24, 256)] private int curveResolution = 96;
        [SerializeField] private float surfaceOffset = .012f;
        [SerializeField] private float gridLineWidth = .0035f;
        [SerializeField] private float mainLineWidth = .009f;

        [Header("Locations")]
        [SerializeField] private List<GlobeLocation> locations = new List<GlobeLocation>
        {
            new GlobeLocation(),
            new GlobeLocation { name = "Greenwich", latitude = 51.4826f, longitude = 0f, description = "The Prime Meridian passes through Greenwich.", color = new Color(.3f, 1f, .45f) },
            new GlobeLocation { name = "Equator", latitude = 0f, longitude = 25f, description = "A point on 0 degrees latitude.", color = new Color(1f, .35f, .2f) }
        };

        private const string GeneratedName = "Generated_Educational_Overlays";
        [SerializeField, HideInInspector] private Transform generatedRoot;
        [SerializeField, HideInInspector] private Transform gridGroup;
        [SerializeField, HideInInspector] private Transform equatorGroup;
        [SerializeField, HideInInspector] private Transform primeGroup;
        [SerializeField, HideInInspector] private Transform specialGroup;
        [SerializeField, HideInInspector] private Transform axisGroup;
        [SerializeField, HideInInspector] private Transform hotspotGroup;
        [SerializeField, HideInInspector] private Transform hemisphereGroup;
        [SerializeField, HideInInspector] private Transform dayNightGroup;
        [SerializeField, HideInInspector] private Transform particleGroup;
        public Transform GlobeRotationTarget => globeRotationTarget != null ? globeRotationTarget : globeSurface;

        public void Configure(Transform surface, Transform rotationTarget)
        {
            globeSurface = surface;
            globeRotationTarget = rotationTarget;
            DetectRadius();
        }

        public void ConfigureMaterials(Material grid, Material equator, Material prime, Material axis, Material special,
            Material pin, Material hemisphere, Material dayNight, Material particle)
        {
            gridMaterial = grid; equatorMaterial = equator; primeMeridianMaterial = prime; axisMaterial = axis;
            specialCircleMaterial = special; pinMaterial = pin; hemisphereMaterial = hemisphere;
            dayNightMaterial = dayNight; particleMaterial = particle;
        }

        [ContextMenu("Build / Rebuild Educational Globe")]
        public void Rebuild()
        {
            if (globeSurface == null) return;
            ClearGenerated();
            DetectRadius();
            generatedRoot = NewGroup(GeneratedName, globeSurface);
            gridGroup = NewGroup("01_Latitude_Longitude_Grid", generatedRoot);
            equatorGroup = NewGroup("02_Equator", generatedRoot);
            primeGroup = NewGroup("03_Prime_Meridian", generatedRoot);
            specialGroup = NewGroup("04_Tropics_And_Polar_Circles", generatedRoot);
            axisGroup = NewGroup("05_Axis_And_Poles", generatedRoot);
            hotspotGroup = NewGroup("06_Location_Hotspots", generatedRoot);
            hemisphereGroup = NewGroup("07_Hemisphere_Highlights", generatedRoot);
            dayNightGroup = NewGroup("08_Day_Night", generatedRoot);
            particleGroup = NewGroup("09_Equator_Flow_VFX", generatedRoot);

            BuildGrid();
            BuildAxisAndPoles();
            BuildHotspots();
            BuildHemispheres();
            BuildDayNight();
            BuildEquatorParticles();
        }

        [ContextMenu("Clear Generated Overlays")]
        public void ClearGenerated()
        {
            Transform old = transform.Find(GeneratedName);
            if (old == null && globeSurface != null) old = globeSurface.Find(GeneratedName);
            if (old == null) return;
            if (Application.isPlaying) Destroy(old.gameObject); else DestroyImmediate(old.gameObject);
        }

        private void DetectRadius()
        {
            if (globeSurface == null) return;
            MeshFilter filter = globeSurface.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null)
            {
                surfaceCenter = filter.sharedMesh.bounds.center;
                radius = Mathf.Max(filter.sharedMesh.bounds.extents.x, filter.sharedMesh.bounds.extents.y, filter.sharedMesh.bounds.extents.z);
            }
            radius = Mathf.Max(.01f, radius);
        }

        private void BuildGrid()
        {
            float r = radius * (1f + surfaceOffset);
            for (int lat = -75; lat <= 75; lat += latitudeInterval)
            {
                if (lat == 0) continue;
                CreateLatitude($"Latitude_{lat}", lat, r, gridLineWidth, gridMaterial, gridGroup);
            }
            for (int lon = -180; lon < 180; lon += longitudeInterval)
            {
                if (lon == 0) continue;
                CreateLongitude($"Longitude_{lon}", lon, r, gridLineWidth, gridMaterial, gridGroup);
            }

            CreateLatitude("Equator_0deg", 0, r * 1.002f, mainLineWidth, equatorMaterial, equatorGroup);
            CreateLongitude("Prime_Meridian_0deg", 0, r * 1.003f, mainLineWidth, primeMeridianMaterial, primeGroup);
            CreateLatitude("Tropic_of_Cancer_23.5N", 23.5f, r * 1.001f, mainLineWidth * .65f, specialCircleMaterial, specialGroup);
            CreateLatitude("Tropic_of_Capricorn_23.5S", -23.5f, r * 1.001f, mainLineWidth * .65f, specialCircleMaterial, specialGroup);
            CreateLatitude("Arctic_Circle_66.5N", 66.5f, r * 1.001f, mainLineWidth * .65f, specialCircleMaterial, specialGroup);
            CreateLatitude("Antarctic_Circle_66.5S", -66.5f, r * 1.001f, mainLineWidth * .65f, specialCircleMaterial, specialGroup);
            CreateLabel("EQUATOR  0°", Point(0, -22, r * 1.035f), equatorMaterial, equatorGroup);
            CreateLabel("PRIME MERIDIAN  0°", Point(35, 0, r * 1.035f), primeMeridianMaterial, primeGroup);
        }

        private void BuildAxisAndPoles()
        {
            Vector3[] points = { surfaceCenter + Vector3.down * radius * 1.35f, surfaceCenter + Vector3.up * radius * 1.35f };
            CreateLine("Earth_Axis_23.5deg", points, mainLineWidth * radius, axisMaterial, axisGroup, false);
            CreatePole("NORTH POLE  90°N", surfaceCenter + Vector3.up * radius * 1.025f);
            CreatePole("SOUTH POLE  90°S", surfaceCenter + Vector3.down * radius * 1.025f);
            CreateLabel("AXIS TILT  23.5°", surfaceCenter + new Vector3(radius * .18f, radius * 1.25f, 0), axisMaterial, axisGroup);
        }

        private void CreatePole(string text, Vector3 position)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pole.name = text.Replace(" ", "_");
            pole.transform.SetParent(axisGroup, false);
            pole.transform.localPosition = position;
            pole.transform.localScale = Vector3.one * radius * .045f;
            DestroySafe(pole.GetComponent<Collider>());
            pole.GetComponent<Renderer>().sharedMaterial = axisMaterial;
            CreateLabel(text, surfaceCenter + (position - surfaceCenter) * 1.1f, axisMaterial, axisGroup);
        }

        private void BuildHotspots()
        {
            float r = radius * 1.035f;
            foreach (GlobeLocation data in locations)
            {
                Vector3 p = Point(data.latitude, data.longitude, r);
                GameObject pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                pin.name = "Hotspot_" + data.name;
                pin.transform.SetParent(hotspotGroup, false);
                pin.transform.localPosition = p;
                pin.transform.localScale = Vector3.one * radius * .055f;
                Renderer renderer = pin.GetComponent<Renderer>();
                renderer.sharedMaterial = pinMaterial;
                if (renderer.sharedMaterial == null) renderer.material.color = data.color;
                GlobeHotspot spot = pin.AddComponent<GlobeHotspot>();
                spot.locationName = data.name; spot.latitude = data.latitude; spot.longitude = data.longitude; spot.description = data.description;
                CreateLabel($"{data.name}\n{FormatCoordinate(data.latitude, true)}, {FormatCoordinate(data.longitude, false)}", surfaceCenter + (p - surfaceCenter) * 1.09f, pinMaterial, hotspotGroup);
            }
        }

        private void BuildHemispheres()
        {
            CreateHemisphere("Northern_Hemisphere", true);
            CreateHemisphere("Southern_Hemisphere", false);
            hemisphereGroup.gameObject.SetActive(false);
        }

        private void CreateHemisphere(string name, bool north)
        {
            int rings = 12, segments = 48;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int y = 0; y <= rings; y++)
            {
                float lat = Mathf.Lerp(0f, north ? 90f : -90f, y / (float)rings);
                for (int x = 0; x <= segments; x++)
                {
                    float lon = Mathf.Lerp(-180f, 180f, x / (float)segments);
                    vertices.Add(Point(lat, lon, radius * 1.018f));
                }
            }
            for (int y = 0; y < rings; y++)
            for (int x = 0; x < segments; x++)
            {
                int a = y * (segments + 1) + x;
                int b = a + segments + 1;
                triangles.Add(a); triangles.Add(b); triangles.Add(a + 1);
                triangles.Add(a + 1); triangles.Add(b); triangles.Add(b + 1);
            }
            Mesh mesh = new Mesh { name = name + "_Mesh" };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            GameObject go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(hemisphereGroup, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = hemisphereMaterial;
        }

        private void BuildDayNight()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Day_Night_Terminator_Overlay";
            sphere.transform.SetParent(dayNightGroup, false);
            sphere.transform.localPosition = surfaceCenter;
            sphere.transform.localScale = Vector3.one * radius * 2.026f;
            DestroySafe(sphere.GetComponent<Collider>());
            sphere.GetComponent<Renderer>().sharedMaterial = dayNightMaterial;
            dayNightGroup.gameObject.SetActive(false);
        }

        private void BuildEquatorParticles()
        {
            GameObject go = new GameObject("Equator_Moving_Direction_Particles", typeof(ParticleSystem), typeof(ParticleSystemRenderer));
            go.transform.SetParent(particleGroup, false);
            go.transform.localPosition = surfaceCenter;
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            var main = ps.main; main.loop = true; main.playOnAwake = true; main.startLifetime = 5f; main.startSpeed = 0f;
            main.startSize = radius * .025f; main.maxParticles = 120; main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission; emission.rateOverTime = 18f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = radius * 1.035f; shape.radiusThickness = 0f; shape.rotation = new Vector3(90f, 0f, 0f);
            var velocity = ps.velocityOverLifetime; velocity.enabled = true; velocity.orbitalY = 1.2f;
            var trails = ps.trails; trails.enabled = true; trails.lifetime = .16f; trails.dieWithParticles = true;
            ParticleSystemRenderer renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard; renderer.sharedMaterial = particleMaterial; renderer.trailMaterial = particleMaterial;
        }

        private void CreateLatitude(string name, float latitude, float r, float width, Material material, Transform parent)
        {
            Vector3[] points = new Vector3[curveResolution + 1];
            for (int i = 0; i <= curveResolution; i++)
                points[i] = Point(latitude, Mathf.Lerp(-180, 180, i / (float)curveResolution), r);
            CreateLine(name, points, width * radius, material, parent, true);
        }

        private void CreateLongitude(string name, float longitude, float r, float width, Material material, Transform parent)
        {
            Vector3[] points = new Vector3[curveResolution + 1];
            for (int i = 0; i <= curveResolution; i++)
                points[i] = Point(Mathf.Lerp(-90, 90, i / (float)curveResolution), longitude, r);
            CreateLine(name, points, width * radius, material, parent, false);
        }

        private static LineRenderer CreateLine(string name, Vector3[] points, float width, Material material, Transform parent, bool loop)
        {
            GameObject go = new GameObject(name, typeof(LineRenderer));
            go.transform.SetParent(parent, false);
            LineRenderer line = go.GetComponent<LineRenderer>();
            line.useWorldSpace = false; line.loop = loop; line.positionCount = points.Length; line.SetPositions(points);
            line.startWidth = width; line.endWidth = width; line.numCapVertices = 3; line.numCornerVertices = 3; line.sharedMaterial = material;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false;
            return line;
        }

        private void CreateLabel(string text, Vector3 localPosition, Material material, Transform parent)
        {
            GameObject go = new GameObject("Label_" + text.Replace("\n", "_"), typeof(TextMesh), typeof(GlobeBillboard));
            go.transform.SetParent(parent, false); go.transform.localPosition = localPosition;
            TextMesh label = go.GetComponent<TextMesh>(); label.text = text; label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.fontSize = 48; label.characterSize = radius * .014f; label.color = material != null && material.HasProperty("_Color") ? material.color : Color.white;
        }

        private static Transform NewGroup(string name, Transform parent)
        {
            Transform t = new GameObject(name).transform; t.SetParent(parent, false); return t;
        }

        private static string FormatCoordinate(float value, bool latitude)
        {
            string suffix = latitude ? (value >= 0 ? "N" : "S") : (value >= 0 ? "E" : "W");
            return $"{Mathf.Abs(value):0.##}°{suffix}";
        }

        private Vector3 Point(float latitude, float longitude, float pointRadius) =>
            surfaceCenter + GlobeCoordinateUtility.LatLonToLocal(latitude, longitude, pointRadius, longitudeOffset);

        private static void DestroySafe(Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }

        private static void SetVisible(Transform group, bool value) { if (group != null) group.gameObject.SetActive(value); }
        public void SetGridVisible(bool value) => SetVisible(gridGroup, value);
        public void SetEquatorVisible(bool value) => SetVisible(equatorGroup, value);
        public void SetPrimeMeridianVisible(bool value) => SetVisible(primeGroup, value);
        public void SetSpecialCirclesVisible(bool value) => SetVisible(specialGroup, value);
        public void SetAxisVisible(bool value) => SetVisible(axisGroup, value);
        public void SetHotspotsVisible(bool value) => SetVisible(hotspotGroup, value);
        public void SetDayNightVisible(bool value) => SetVisible(dayNightGroup, value);
        public void SetParticlesVisible(bool value) => SetVisible(particleGroup, value);
        public void ShowNorthernHemisphere() => SetHemisphere(true, false);
        public void ShowSouthernHemisphere() => SetHemisphere(false, true);
        public void ShowBothHemispheres() => SetHemisphere(true, true);
        public void HideHemispheres() => SetHemisphere(false, false);

        private void SetHemisphere(bool north, bool south)
        {
            if (hemisphereGroup == null) return;
            hemisphereGroup.gameObject.SetActive(north || south);
            Transform n = hemisphereGroup.Find("Northern_Hemisphere");
            Transform s = hemisphereGroup.Find("Southern_Hemisphere");
            if (n != null) n.gameObject.SetActive(north);
            if (s != null) s.gameObject.SetActive(south);
        }

        public void SetAutoRotation(bool value)
        {
            GlobeDayNightController dayNight = GetComponent<GlobeDayNightController>();
            if (dayNight != null) dayNight.SetAnimated(value);
        }

        public void ResetGlobeRotation()
        {
            if (GlobeRotationTarget != null) GlobeRotationTarget.localRotation = Quaternion.Euler(0, 0, -23.5f);
        }

        public float DistanceBetweenLocationsKm(int first, int second)
        {
            if (first < 0 || second < 0 || first >= locations.Count || second >= locations.Count) return -1f;
            GlobeLocation a = locations[first], b = locations[second];
            return GlobeCoordinateUtility.GreatCircleDistanceKm(a.latitude, a.longitude, b.latitude, b.longitude);
        }
    }
}
