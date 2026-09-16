//using UnityEngine;

//// Optional desktop/manual test component.
//// For VR/XR, your normal grab/interaction component can move this same 3D object.
//// Thermometer3DController will read its position automatically.
//public class Thermometer3DMouseDrag : MonoBehaviour
//{
//    public Thermometer3DController controller;
//    public Camera targetCamera;
//    bool dragging;
//    Plane dragPlane;

//    void OnMouseDown()
//    {
//        if (!controller) return;
//        if (!targetCamera) targetCamera = Camera.main;
//        if (!targetCamera) return;
//        dragPlane = new Plane(-targetCamera.transform.forward, transform.position);
//        dragging = true;
//    }

//    void OnMouseDrag()
//    {
//        if (!dragging || !targetCamera || !controller) return;
//        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
//        if (dragPlane.Raycast(ray, out float enter))
//        {
//            transform.position = ray.GetPoint(enter);
//            controller.ReadTemperatureFrom3DSlider();
//        }
//    }

//    void OnMouseUp() { dragging = false; }
//}
using UnityEngine;

public class Thermometer3DMouseDrag : MonoBehaviour
{
    [Header("REFERENCE")]
    public Thermometer3DController controller;

    [Header("CAMERA")]
    public Camera targetCamera;

    private bool dragging = false;
    private Plane dragPlane;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (controller == null)
            return;

        // FIRST CHECKBOX
        // OFF = Signal control ONLY
        // ON  = Mouse control + Signal control
        if (!controller.allowManualSliderMovement)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        dragPlane = new Plane(
            -targetCamera.transform.forward,
            transform.position
        );

        dragging = true;
    }

    private void OnMouseDrag()
    {
        if (!dragging)
            return;

        if (controller == null)
            return;

        // If Allow Manual Slider Movement becomes OFF,
        // immediately stop mouse dragging.
        if (!controller.allowManualSliderMovement)
        {
            dragging = false;
            return;
        }

        if (targetCamera == null)
            return;

        Ray ray = targetCamera.ScreenPointToRay(
            Input.mousePosition
        );

        if (dragPlane.Raycast(ray, out float enter))
        {
            // Mouse moves physical 3D slider
            transform.position = ray.GetPoint(enter);

            // Main controller calculates temperature.
            //
            // Clamp Slider To Track = ON
            // -> slider is locked onto thermometer track.
            //
            // Clamp Slider To Track = OFF
            // -> slider is not forced onto track.
            controller.ReadTemperatureFrom3DSlider();
        }
    }

    private void OnMouseUp()
    {
        dragging = false;
    }

    private void OnDisable()
    {
        dragging = false;
    }
}