using UnityEngine;

public class IonMover : MonoBehaviour
{
    public enum MovementMode
    {
        MoveToTarget,
        MoveInDirection
    }

    [Header("Movement")]
    public MovementMode movementMode = MovementMode.MoveToTarget;

    [Tooltip("Target the ion will move toward.")]
    public Transform target;

    [Tooltip("Used only when Move In Direction is selected.")]
    public Vector3 moveDirection = Vector3.forward;

    [Min(0.01f)]
    public float moveSpeed = 0.25f;

    [Header("Play Settings")]
    [Tooltip("If enabled, the ion will start moving automatically when the object starts.")]
    public bool playOnStart = false;

    [Header("Movement Look")]
    public bool addFloatingMotion = true;

    public float floatAmount = 0.015f;
    public float floatSpeed = 3f;

    [Header("Target Settings")]
    [Tooltip("Ion disappears when it gets this close to target.")]
    public float reachDistance = 0.015f;

    [Header("Lifetime")]
    public float maxLifetime = 8f;

    [Header("Arrival Effect")]
    public GameObject arrivalEffect;

    [Header("Runtime")]
    [SerializeField]
    private bool isMoving = false;

    private float randomOffset;
    private float lifetime;

    private void Start()
    {
        randomOffset = Random.Range(0f, 100f);

        // Start automatically only if Play On Start is enabled
        if (playOnStart)
        {
            StartMoving();
        }
        else
        {
            isMoving = false;
        }
    }

    private void Update()
    {
        if (!isMoving)
            return;

        lifetime += Time.deltaTime;

        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        switch (movementMode)
        {
            case MovementMode.MoveToTarget:
                MoveTowardsTarget();
                break;

            case MovementMode.MoveInDirection:
                MoveDirection();
                break;
        }
    }

    // =========================================
    // START MOVEMENT
    // =========================================

    public void StartMoving()
    {
        lifetime = 0f;
        isMoving = true;
    }

    // =========================================
    // STOP MOVEMENT
    // =========================================

    public void StopMoving()
    {
        isMoving = false;
    }

    // =========================================
    // MOVEMENT
    // =========================================

    private void MoveTowardsTarget()
    {
        if (target == null)
            return;

        Vector3 direction =
            (target.position - transform.position).normalized;

        Vector3 movement =
            direction * moveSpeed * Time.deltaTime;

        if (addFloatingMotion)
        {
            movement += GetFloatingMotion();
        }

        transform.position += movement;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        if (distance <= reachDistance)
        {
            ReachedDestination();
        }
    }

    private void MoveDirection()
    {
        Vector3 direction = moveDirection.normalized;

        Vector3 movement =
            direction * moveSpeed * Time.deltaTime;

        if (addFloatingMotion)
        {
            movement += GetFloatingMotion();
        }

        transform.position += movement;
    }

    private Vector3 GetFloatingMotion()
    {
        float x =
            Mathf.Sin(
                (Time.time + randomOffset) * floatSpeed
            ) * floatAmount * Time.deltaTime;

        float y =
            Mathf.Cos(
                (Time.time + randomOffset * 0.7f) * floatSpeed
            ) * floatAmount * Time.deltaTime;

        return new Vector3(x, y, 0f);
    }

    // =========================================
    // TARGET REACHED
    // =========================================

    private void ReachedDestination()
    {
        if (arrivalEffect != null)
        {
            Instantiate(
                arrivalEffect,
                transform.position,
                Quaternion.identity
            );
        }

        Destroy(gameObject);
    }

    // =========================================
    // PUBLIC FUNCTIONS
    // =========================================

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        movementMode = MovementMode.MoveToTarget;
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction;
        movementMode = MovementMode.MoveInDirection;
    }

    public void MoveToTarget(Transform newTarget)
    {
        target = newTarget;
        movementMode = MovementMode.MoveToTarget;

        StartMoving();
    }
}