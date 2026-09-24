using UnityEngine;
using UnityEngine.EventSystems;

namespace InteractiveGlobeKit
{
    public sealed class GlobeTouchRotateZoom : MonoBehaviour
    {
        [SerializeField] private Transform rotationTarget;
        [SerializeField] private Transform scaleTarget;
        [SerializeField] private Collider globeCollider;
        [SerializeField] private float rotationSpeed = .22f;
        [SerializeField] private float zoomSpeed = .006f;
        [SerializeField] private float mouseWheelSpeed = .12f;
        [SerializeField] private float minScale = .65f;
        [SerializeField] private float maxScale = 1.7f;
        private Vector2 lastPointer;
        private bool dragging;

        public void Configure(Transform rotate, Transform scale, Collider hitCollider)
        {
            rotationTarget = rotate;
            scaleTarget = scale;
            globeCollider = hitCollider;
        }

        private void Update()
        {
            if (rotationTarget == null) rotationTarget = transform;
            if (scaleTarget == null) scaleTarget = transform;
            HandleTouch();
            HandleMouse();
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    dragging = !IsOverUI(touch.fingerId) && HitGlobe(touch.position);
                    lastPointer = touch.position;
                }
                else if (dragging && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
                {
                    Rotate(touch.position - lastPointer);
                    lastPointer = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) dragging = false;
            }
            else if (Input.touchCount == 2)
            {
                dragging = false;
                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);
                Vector2 oldA = a.position - a.deltaPosition;
                Vector2 oldB = b.position - b.deltaPosition;
                float delta = Vector2.Distance(a.position, b.position) - Vector2.Distance(oldA, oldB);
                Zoom(delta * zoomSpeed);
            }
        }

        private void HandleMouse()
        {
            if (Input.touchCount > 0) return;
            Vector2 mouse = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))
            {
                dragging = !IsOverUI(-1) && HitGlobe(mouse);
                lastPointer = mouse;
            }
            if (dragging && Input.GetMouseButton(0))
            {
                Rotate(mouse - lastPointer);
                lastPointer = mouse;
            }
            if (Input.GetMouseButtonUp(0)) dragging = false;
            float wheel = Input.mouseScrollDelta.y;
            if (Mathf.Abs(wheel) > .001f) Zoom(wheel * mouseWheelSpeed);
        }

        private void Rotate(Vector2 delta)
        {
            rotationTarget.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            Camera cam = Camera.main;
            Vector3 axis = cam == null ? Vector3.right : cam.transform.right;
            rotationTarget.Rotate(axis, delta.y * rotationSpeed, Space.World);
        }

        private void Zoom(float delta)
        {
            float current = scaleTarget.localScale.x;
            float next = Mathf.Clamp(current + delta, minScale, maxScale);
            scaleTarget.localScale = Vector3.one * next;
        }

        private bool HitGlobe(Vector2 point)
        {
            if (globeCollider == null || Camera.main == null) return true;
            Ray ray = Camera.main.ScreenPointToRay(point);
            return globeCollider.Raycast(ray, out _, 1000f);
        }

        private static bool IsOverUI(int fingerId)
        {
            if (EventSystem.current == null) return false;
            return fingerId >= 0 ? EventSystem.current.IsPointerOverGameObject(fingerId) : EventSystem.current.IsPointerOverGameObject();
        }
    }
}
