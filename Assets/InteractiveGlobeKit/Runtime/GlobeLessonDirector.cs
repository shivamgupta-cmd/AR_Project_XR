using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace InteractiveGlobeKit
{
    public sealed class GlobeLessonDirector : MonoBehaviour
    {
        [System.Serializable]
        public sealed class LessonStep
        {
            public string title;
            [TextArea(2, 6)] public string instruction;
            [TextArea(2, 8)] public string voiceOver;
            public AudioClip narration;
            public UnityEvent onEnter;
        }

        [SerializeField] private InteractiveGlobeController globe;
        [SerializeField] private Text titleText;
        [SerializeField] private Text instructionText;
        [SerializeField] private AudioSource narrationSource;
        [SerializeField] private LessonStep[] steps;
        [SerializeField] private int currentStep;
        public int CurrentStep => currentStep;

        private void Reset()
        {
            globe = GetComponentInChildren<InteractiveGlobeController>();
            steps = Defaults();
        }

        private void Start()
        {
            if (steps == null || steps.Length == 0) steps = Defaults();
            ShowStep(Mathf.Clamp(currentStep, 0, steps.Length - 1));
        }

        public void NextStep() => ShowStep(Mathf.Min(currentStep + 1, steps.Length - 1));
        public void PreviousStep() => ShowStep(Mathf.Max(currentStep - 1, 0));
        public void RestartLesson() => ShowStep(0);

        public void ShowStep(int index)
        {
            if (steps == null || steps.Length == 0) return;
            currentStep = Mathf.Clamp(index, 0, steps.Length - 1);
            ApplyVisuals(currentStep);
            LessonStep step = steps[currentStep];
            if (titleText != null) titleText.text = step.title;
            if (instructionText != null) instructionText.text = step.instruction;
            if (narrationSource != null)
            {
                narrationSource.Stop(); narrationSource.clip = step.narration;
                if (step.narration != null) narrationSource.Play();
            }
            step.onEnter?.Invoke();
        }

        private void ApplyVisuals(int step)
        {
            if (globe == null) return;
            globe.SetGridVisible(false); globe.SetEquatorVisible(false); globe.SetPrimeMeridianVisible(false);
            globe.SetSpecialCirclesVisible(false); globe.SetAxisVisible(false); globe.SetHotspotsVisible(false);
            globe.SetDayNightVisible(false); globe.SetParticlesVisible(false); globe.HideHemispheres(); globe.SetAutoRotation(false);
            switch (step)
            {
                case 0: globe.SetAutoRotation(true); break;
                case 2: globe.SetEquatorVisible(true); globe.SetAxisVisible(true); globe.SetParticlesVisible(true); break;
                case 3: globe.SetGridVisible(true); globe.SetEquatorVisible(true); globe.SetPrimeMeridianVisible(true); globe.SetSpecialCirclesVisible(true); break;
                case 4: globe.SetEquatorVisible(true); globe.SetPrimeMeridianVisible(true); globe.ShowBothHemispheres(); break;
                case 5: globe.SetGridVisible(true); globe.SetHotspotsVisible(true); break;
                case 6: globe.SetAxisVisible(true); globe.SetDayNightVisible(true); globe.SetAutoRotation(true); break;
                case 7: globe.SetGridVisible(true); globe.SetEquatorVisible(true); globe.SetPrimeMeridianVisible(true); globe.SetAxisVisible(true); globe.SetHotspotsVisible(true); break;
            }
        }

        private static LessonStep[] Defaults()
        {
            return new[]
            {
                Step("Our Earth", "Observe the globe and notice its spherical shape.", "A globe is a spherical model of Earth. It helps us understand places, directions and how our planet moves."),
                Step("Explore", "Drag to rotate. Pinch or use the mouse wheel to zoom.", "Rotate the globe to explore continents and oceans from every direction."),
                Step("Equator and Poles", "Study the equator, poles and tilted axis.", "The equator divides Earth into northern and southern hemispheres. Earth's axis is tilted by about twenty-three and a half degrees."),
                Step("Latitude and Longitude", "Turn on the coordinate grid and locate a point.", "Latitude measures north or south of the equator. Longitude measures east or west of the Prime Meridian."),
                Step("Hemispheres", "Compare Earth's northern, southern, eastern and western halves.", "The equator separates north and south. The Prime Meridian helps separate east and west."),
                Step("Real Places", "Tap glowing pins to identify locations and coordinates.", "Every location can be described using a latitude and longitude coordinate."),
                Step("Day and Night", "Observe the illuminated side and dark side as Earth rotates.", "Day and night occur because Earth rotates. The side facing the Sun has daylight while the opposite side experiences night."),
                Step("Review", "Review the overlays, then continue to the learning check.", "You explored the equator, poles, hemispheres, coordinates, axis tilt, locations, and day and night.")
            };
        }

        private static LessonStep Step(string title, string instruction, string voiceOver) =>
            new LessonStep { title = title, instruction = instruction, voiceOver = voiceOver, onEnter = new UnityEvent() };
    }
}
