using UnityEngine;

public class VOController : MonoBehaviour
{
    public static VOController Instance;

    [Header("Audio")]
    [SerializeField] private AudioSource voiceSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayVO(AudioClip clip)
    {
        if (clip == null || voiceSource == null)
            return;

        voiceSource.Stop();

        voiceSource.clip = clip;
        voiceSource.Play();
    }

    public void StopVO()
    {
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
    }

    public bool IsPlaying()
    {
        return voiceSource != null && voiceSource.isPlaying;
    }

    public void PauseVO()
    {
        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Pause();
    }
    public void ResumeVO()
    {
        if (voiceSource != null)
            voiceSource.UnPause();
    }
}
