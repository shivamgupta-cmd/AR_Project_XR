using UnityEngine;

namespace EducationalSun.VFX
{
    /// <summary>
    /// Playback controller for the generated Sun prefab. The prefab is created
    /// once in the Unity Editor; this component never rebuilds it at runtime.
    /// </summary>
    public sealed class SunVFXController : MonoBehaviour
    {
        [Header("Sun Rotation")]
        [SerializeField] private Transform rotatingSurface;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField, Min(0f)] private float rotationSpeed = 2.5f;

        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem[] continuousEffects;
        [SerializeField] private ParticleSystem solarFlare;
        [SerializeField] private ParticleSystem coronalMassEjection;

        [Header("Automatic Solar Activity")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool automaticFlares = true;
        [SerializeField] private Vector2 flareInterval = new Vector2(7f, 13f);
        [SerializeField, Min(1)] private int flareParticles = 18;
        [SerializeField, Min(1)] private int cmeParticles = 10;

        private float nextFlareTime;

        private void OnEnable()
        {
            ScheduleNextFlare();

            if (playOnEnable)
                PlayAll();
        }

        private void Update()
        {
            if (rotatingSurface != null && rotationSpeed > 0f)
            {
                rotatingSurface.Rotate(
                    rotationAxis.normalized,
                    rotationSpeed * Time.deltaTime,
                    Space.Self);
            }

            if (!automaticFlares || Time.time < nextFlareTime)
                return;

            TriggerSolarFlare();
            ScheduleNextFlare();
        }

        public void Configure(
            Transform surface,
            ParticleSystem[] continuous,
            ParticleSystem flare,
            ParticleSystem cme)
        {
            rotatingSurface = surface;
            continuousEffects = continuous;
            solarFlare = flare;
            coronalMassEjection = cme;
        }

        public void PlayAll()
        {
            if (continuousEffects != null)
            {
                foreach (ParticleSystem effect in continuousEffects)
                {
                    if (effect != null && !effect.isPlaying)
                        effect.Play(true);
                }
            }
        }

        public void StopAll()
        {
            if (continuousEffects != null)
            {
                foreach (ParticleSystem effect in continuousEffects)
                {
                    if (effect != null)
                        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            if (solarFlare != null)
                solarFlare.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (coronalMassEjection != null)
                coronalMassEjection.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void TriggerSolarFlare()
        {
            if (solarFlare != null)
            {
                if (!solarFlare.isPlaying)
                    solarFlare.Play(true);

                solarFlare.Emit(flareParticles);
            }

            if (coronalMassEjection != null)
            {
                if (!coronalMassEjection.isPlaying)
                    coronalMassEjection.Play(true);

                coronalMassEjection.Emit(cmeParticles);
            }
        }

        public void SetAutomaticFlares(bool enabled)
        {
            automaticFlares = enabled;
            ScheduleNextFlare();
        }

        public void SetRotationSpeed(float degreesPerSecond)
        {
            rotationSpeed = Mathf.Max(0f, degreesPerSecond);
        }

        private void ScheduleNextFlare()
        {
            float minimum = Mathf.Max(0.25f, flareInterval.x);
            float maximum = Mathf.Max(minimum, flareInterval.y);
            nextFlareTime = Time.time + Random.Range(minimum, maximum);
        }
    }
}
