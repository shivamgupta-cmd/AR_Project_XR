using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;

using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class ARTouchManipulator : MonoBehaviour
{
    public enum InteractionMode
    {
        None,
        Move,
        Rotate,
        Zoom
    }

    [SerializeField] private Transform target;
    [SerializeField] private Camera arCamera;

    [SerializeField] private Button resetButton;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button rotateButton;
    [SerializeField] private Button zoomButton;

    [SerializeField] private InteractionMode currentMode = InteractionMode.None;

    [SerializeField] private float rotationSpeed = 0.2f;
    [SerializeField] private float moveSpeed = 0.0015f;

    [SerializeField] private float minScaleMultiplier = 0.3f;
    [SerializeField] private float maxScaleMultiplier = 3f;

    [SerializeField] private bool requireTouchOnObject = true;
    [SerializeField] private LayerMask touchableLayers = ~0;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalScale;

    private bool gestureActive = false;

    private void Awake()
    {
        if (target == null)
            target = transform;

        if (arCamera == null)
            arCamera = Camera.main;

        initialLocalPosition = target.localPosition;
        initialLocalRotation = target.localRotation;
        initialLocalScale = target.localScale;

        EnhancedTouchSupport.Enable();
    }

    private void Start()
    {
        if (moveButton != null)
            moveButton.onClick.AddListener(SelectMove);

        if (rotateButton != null)
            rotateButton.onClick.AddListener(SelectRotate);

        if (zoomButton != null)
            zoomButton.onClick.AddListener(SelectZoom);

        if (resetButton != null)
            resetButton.onClick.AddListener(ResetObject);
    }

    private void Update()
    {
        var touches = Touch.activeTouches;

        if (touches.Count == 0)
        {
            gestureActive = false;
            return;
        }

        if (currentMode == InteractionMode.Move)
        {
            HandleMove(touches);
            return;
        }

        if (currentMode == InteractionMode.Rotate)
        {
            HandleRotate(touches);
            return;
        }

        if (currentMode == InteractionMode.Zoom)
        {
            HandleZoom(touches);
        }
    }

    private void HandleMove(
        UnityEngine.InputSystem.Utilities.ReadOnlyArray<Touch> touches)
    {
        if (touches.Count != 1)
        {
            gestureActive = false;
            return;
        }

        Touch touch = touches[0];

        if (touch.phase == TouchPhase.Began)
        {
            gestureActive =
                !requireTouchOnObject ||
                IsTouchOnTarget(touch.screenPosition);
        }

        if (!gestureActive)
            return;

        if (touch.phase == TouchPhase.Moved)
            MoveTarget(touch.delta);

        if (touch.phase == TouchPhase.Ended ||
            touch.phase == TouchPhase.Canceled)
        {
            gestureActive = false;
        }
    }

    private void HandleRotate(
        UnityEngine.InputSystem.Utilities.ReadOnlyArray<Touch> touches)
    {
        if (touches.Count != 1)
        {
            gestureActive = false;
            return;
        }

        Touch touch = touches[0];

        if (touch.phase == TouchPhase.Began)
        {
            gestureActive =
                !requireTouchOnObject ||
                IsTouchOnTarget(touch.screenPosition);
        }

        if (!gestureActive)
            return;

        if (touch.phase == TouchPhase.Moved)
            RotateTarget(touch.delta);

        if (touch.phase == TouchPhase.Ended ||
            touch.phase == TouchPhase.Canceled)
        {
            gestureActive = false;
        }
    }

    private void HandleZoom(
        UnityEngine.InputSystem.Utilities.ReadOnlyArray<Touch> touches)
    {
        if (touches.Count < 2)
        {
            gestureActive = false;
            return;
        }

        Touch touch0 = touches[0];
        Touch touch1 = touches[1];

        if (!gestureActive)
        {
            if (!requireTouchOnObject)
            {
                gestureActive = true;
            }
            else
            {
                bool first =
                    IsTouchOnTarget(touch0.screenPosition);

                bool second =
                    IsTouchOnTarget(touch1.screenPosition);

                gestureActive = first || second;
            }
        }

        if (!gestureActive)
            return;

        Vector2 current0 = touch0.screenPosition;
        Vector2 current1 = touch1.screenPosition;

        Vector2 previous0 =
            current0 - touch0.delta;

        Vector2 previous1 =
            current1 - touch1.delta;

        float currentDistance =
            Vector2.Distance(current0, current1);

        float previousDistance =
            Vector2.Distance(previous0, previous1);

        if (previousDistance <= 0.01f)
            return;

        float scaleRatio =
            currentDistance / previousDistance;

        scaleRatio =
            Mathf.Clamp(scaleRatio, 0.8f, 1.2f);

        ScaleTarget(scaleRatio);
    }

    //private void RotateTarget(Vector2 delta)
    //{
    //    float rotationAmount =
    //        -delta.x * rotationSpeed;

    //    target.Rotate(
    //        0f,
    //        rotationAmount,
    //        0f,
    //        Space.Self
    //    );
    //}

    private void RotateTarget(Vector2 delta)
    {
        float rotationAmount =
            -delta.x * rotationSpeed;

        Vector3 currentEuler =
            target.eulerAngles;

        float newY =
            currentEuler.y + rotationAmount;

        target.rotation =
            Quaternion.Euler(
                0f,
                newY,
                0f
            );
    }

    private void MoveTarget(Vector2 delta)
    {
        if (arCamera == null)
            return;

        float distance =
            Vector3.Distance(
                arCamera.transform.position,
                target.position
            );

        distance = Mathf.Max(distance, 0.25f);

        Vector3 horizontal =
            arCamera.transform.right * delta.x;

        Vector3 vertical =
            arCamera.transform.up * delta.y;

        target.position +=
            (horizontal + vertical) *
            moveSpeed *
            distance;
    }

    private void ScaleTarget(float scaleRatio)
    {
        if (Mathf.Abs(initialLocalScale.x) < 0.0001f)
            return;

        float currentMultiplier =
            target.localScale.x /
            initialLocalScale.x;

        float newMultiplier =
            currentMultiplier *
            scaleRatio;

        newMultiplier =
            Mathf.Clamp(
                newMultiplier,
                minScaleMultiplier,
                maxScaleMultiplier
            );

        target.localScale =
            initialLocalScale *
            newMultiplier;
    }

    private bool IsTouchOnTarget(Vector2 screenPosition)
    {
        if (arCamera == null || target == null)
            return false;

        Ray ray =
            arCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            100f,
            touchableLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == target)
                return true;

            if (hit.transform.IsChildOf(target))
                return true;
        }

        return false;
    }

    public void SelectMove()
    {
        currentMode = InteractionMode.Move;
        gestureActive = false;
    }

    public void SelectRotate()
    {
        currentMode = InteractionMode.Rotate;
        gestureActive = false;
    }

    public void SelectZoom()
    {
        currentMode = InteractionMode.Zoom;
        gestureActive = false;
    }

    public void ResetObject()
    {
        if (target == null)
            return;

        target.localPosition = initialLocalPosition;
        target.localRotation = initialLocalRotation;
        target.localScale = initialLocalScale;

        gestureActive = false;
    }

    private void OnDestroy()
    {
        if (moveButton != null)
            moveButton.onClick.RemoveListener(SelectMove);

        if (rotateButton != null)
            rotateButton.onClick.RemoveListener(SelectRotate);

        if (zoomButton != null)
            zoomButton.onClick.RemoveListener(SelectZoom);

        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetObject);
    }
}