using UnityEngine;

public class ThomsonPositiveSphereController : MonoBehaviour
{
    [Header("References")]
    public GameObject solidSphere;
    public GameObject cutawaySphere;
    public GameObject positiveCharges;

    [Header("Rotation")]
    public float rotationSpeed = 12f;
    public bool rotate = true;

    void Update()
    {
        if (rotate) transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    public void ShowNormalSphere()
    {
        if (solidSphere) solidSphere.SetActive(true);
        if (cutawaySphere) cutawaySphere.SetActive(false);
        if (positiveCharges) positiveCharges.SetActive(false);
    }

    public void ShowPositiveCharges()
    {
        if (solidSphere) solidSphere.SetActive(true);
        if (cutawaySphere) cutawaySphere.SetActive(false);
        if (positiveCharges) positiveCharges.SetActive(true);
    }

    public void ShowNoNucleusCutaway()
    {
        if (solidSphere) solidSphere.SetActive(false);
        if (cutawaySphere) cutawaySphere.SetActive(true);
        if (positiveCharges) positiveCharges.SetActive(true);
    }

    public void HideAll()
    {
        if (solidSphere) solidSphere.SetActive(false);
        if (cutawaySphere) cutawaySphere.SetActive(false);
        if (positiveCharges) positiveCharges.SetActive(false);
    }
}
