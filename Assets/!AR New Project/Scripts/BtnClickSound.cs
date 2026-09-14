using UnityEngine;
using UnityEngine.UI;

public class BtnClickSound : MonoBehaviour
{
    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clickSound;


    private void Start()
    {
    }


    public void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
