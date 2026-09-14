using UnityEngine;
using UnityEngine.Serialization;

public class Switch3DButton : MonoBehaviour
{
    [Header("3D Camera")]
    [SerializeField] private Camera threeDCamera;

    [Header("Switch Manager")]
    [SerializeField] private ElectricalSwitchManager electricalSwitchManager;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [FormerlySerializedAs("light")]
    [SerializeField] private GameObject switchLight;
    private void Update()
    {
        if (Input.touchCount <= 0)
            return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase != TouchPhase.Began)
            return;

        if (threeDCamera == null)
        {
            return;
        }

        Ray ray = threeDCamera.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            if (hit.collider.gameObject == gameObject)
            {
                if (audioSource != null && audioClip != null)
                    audioSource.PlayOneShot(audioClip);

                if (switchLight != null)
                    switchLight.SetActive(true);

                if (electricalSwitchManager != null)
                    electricalSwitchManager.ClickSwitchBtn();
            }
        }
    }
}
