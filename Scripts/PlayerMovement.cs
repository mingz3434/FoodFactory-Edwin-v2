using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 10f;
    public float mass = 1f;
    private Rigidbody rb;
    private float splineT = 0f;
    public Transform objectContainer;
    private Vector3 splineCenter;

    public Transform hookContainer;
    public Transform hook;
    public float maxPullDistance = 2f;
    public float launchPower = 10f;
    public float upwardAngle = 45f;
    public float maxAngle = 45f;
    private bool isPulling = false;
    private Vector3 pullStartPos;
    private LineRenderer trajectoryLine;
    private int trajectoryPoints = 50;
    private float trajectoryTimeStep = 0.05f;
    private bool isHookLaunched = false;

    public GameObject deliveryPlatePrefab; // 出餐口 Prefab
    private OrderManager orderManager;

    void Start()
    {
        orderManager = FindFirstObjectByType<OrderManager>();
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        transform.position = new Vector3(transform.position.x, 3f, transform.position.z);

        if (objectContainer == null)
        {
            GameObject container = new GameObject("ObjectContainer");
            container.transform.SetParent(transform);
            container.transform.localPosition = new Vector3(0f, 1f, 0f);
            objectContainer = container.transform;
        }

        if (hookContainer == null)
        {
            hookContainer = transform.Find("HookContainer");
        }
        if (hookContainer != null && hook == null)
        {
            hook = hookContainer.Find("Hook");
        }
        if (hook != null)
        {
            Rigidbody hookRb = hook.GetComponent<Rigidbody>();
            hookRb.useGravity = false;
            hookRb.isKinematic = true;
        }

        trajectoryLine = gameObject.AddComponent<LineRenderer>();
        trajectoryLine.positionCount = trajectoryPoints;
        trajectoryLine.enabled = false;
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.1f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = Color.yellow;
        trajectoryLine.endColor = Color.red;

        float3 playerPos = new float3(transform.position.x, transform.position.y, transform.position.z);
        SplineUtility.GetNearestPoint(spline.Spline, playerPos, out float3 nearestPoint, out float initialT);
        splineT = initialT;

        CalculateSplineCenter();
    }

    void CalculateSplineCenter()
    {
        if (spline == null) return;

        int samples = 100;
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;
            sum += (Vector3)spline.EvaluatePosition(t);
        }
        splineCenter = sum / samples;
        splineCenter.y = 3f;
    }

    void FixedUpdate()
    {
        float input = Input.GetAxis("Horizontal");
        splineT += input * speed * Time.fixedDeltaTime / spline.Spline.GetLength();
        splineT = Mathf.Repeat(splineT, 1f);

        Vector3 targetPosition = spline.EvaluatePosition(splineT);
        targetPosition.y = 3f;
        Vector3 tangent = math.normalize(spline.EvaluateTangent(splineT));

        Vector3 move = tangent * input * speed;
        rb.AddForce(move - rb.linearVelocity, ForceMode.VelocityChange);
        rb.MovePosition(targetPosition);

        Vector3 directionToCenter = splineCenter - transform.position;
        directionToCenter.y = 0;
        if (directionToCenter != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PickUpFood();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            GenerateDeliveryPlate();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Machine[] allMachines = FindObjectsByType<Machine>(FindObjectsSortMode.None);
            foreach (Machine machine in allMachines)
            {
                if (machine.type == MachineType.Mixer)
                {
                    if (machine.needsInput)
                    {
                        machine.StopMixer();
                    }
                    else
                    {
                        for (int i = machine.slotPositions.Length - 1; i >= 0; i--)
                        {
                            Food food = machine.GetFoodAtSlot(i);
                            if (food != null)
                            {
                                Destroy(food.gameObject);
                                machine.RemoveFood(i);
                                Debug.Log("Mixer: Food destroyed during processing.");
                            }
                        }
                    }
                }
            }
        }

        HandleSlingshotInput();
    }

    void GenerateDeliveryPlate()
    {
        if (objectContainer.childCount > 0)
        {
            Debug.Log("ObjectContainer is not empty!");
            return;
        }

        GameObject plate = orderManager.GenerateDeliveryPlate();
        if (plate == null)
        {
            Debug.Log("Failed to generate plate: max plates reached or no orders.");
        }
    }

    void HandleSlingshotInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PickUpFood();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (isHookLaunched) return;
            isPulling = true;
            pullStartPos = Input.mousePosition;
            trajectoryLine.enabled = true;
        }

        if (isPulling)
        {
            Vector3 pullDelta = Input.mousePosition - pullStartPos;
            float pullDistance = Mathf.Clamp(pullDelta.magnitude / Screen.height, 0f, maxPullDistance);
            Vector3 pullDirection = -pullDelta.normalized;

            Vector3 playerForward = transform.forward;
            playerForward.y = 0;
            playerForward.Normalize();
            Vector3 playerRight = transform.right;
            playerRight.y = 0;
            playerRight.Normalize();

            Vector3 worldPullDir = (pullDirection.x * playerRight + pullDirection.y * playerForward).normalized;

            Vector3 velocity = worldPullDir * pullDistance * launchPower;

            float rad = upwardAngle * Mathf.Deg2Rad;
            velocity = new Vector3(velocity.x, velocity.magnitude * Mathf.Sin(rad), velocity.z * Mathf.Cos(rad));

            Vector3 startPos = objectContainer.childCount > 0 ? objectContainer.position : hookContainer.position;

            UpdateTrajectoryLine(startPos, velocity);

            if (Input.GetMouseButtonUp(0))
            {
                isPulling = false;
                trajectoryLine.enabled = false;
                LaunchProjectile(velocity);
            }
        }
    }

    void UpdateTrajectoryLine(Vector3 startPos, Vector3 velocity)
    {
        Vector3[] points = new Vector3[trajectoryPoints];
        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * trajectoryTimeStep;
            points[i] = startPos + velocity * time + 0.5f * Physics.gravity * time * time;
        }
        trajectoryLine.SetPositions(points);
    }

    void LaunchProjectile(Vector3 velocity)
    {
        GameObject projectile = null;
        Rigidbody projRb = null;

        if (objectContainer.childCount > 0)
        {
            projectile = objectContainer.GetChild(0).gameObject;
            projectile.transform.SetParent(null);
            projRb = projectile.GetComponent<Rigidbody>();
            if (projRb == null)
            {
                projRb = projectile.AddComponent<Rigidbody>();
            }
            projRb.isKinematic = false;
            projRb.useGravity = true;
            projRb.mass = 1f;
            projRb.linearVelocity = Vector3.zero;
            projRb.angularVelocity = Vector3.zero;
            projRb.AddForce(velocity, ForceMode.VelocityChange);
        }
        else if (hook != null && !isHookLaunched)
        {
            projectile = hook.gameObject;
            projectile.transform.SetParent(null);
            isHookLaunched = true;
            projRb = projectile.GetComponent<Rigidbody>();
            projRb.isKinematic = false;
            projRb.useGravity = true;
            projRb.AddForce(velocity, ForceMode.VelocityChange);
            StartCoroutine(AutoResetHook(3f));
        }
    }

    public void ResetHook()
    {
        if (hook == null || hookContainer == null) return;

        isHookLaunched = false;
        hook.transform.SetParent(hookContainer);
        hook.transform.localPosition = Vector3.zero;
        hook.transform.localRotation = Quaternion.identity;
        Rigidbody hookRb = hook.GetComponent<Rigidbody>();
        if (hookRb != null)
        {
            hookRb.useGravity = false;
            hookRb.isKinematic = true;
            //hookRb.linearVelocity = Vector3.zero;
            //hookRb.angularVelocity = Vector3.zero;
        }
    }

    IEnumerator AutoResetHook(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Debug.Log("AutoResetHook");
        ResetHook();
    }

    void PickUpFood()
    {
        if (objectContainer.childCount > 0)
        {
            Debug.Log("ObjectContainer is full!");
            return;
        }

        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 4f))
        {
            if (hit.collider.CompareTag("Food"))
            {
                GameObject food = hit.collider.gameObject;
                Destroy(food.GetComponent<ConveyorMover>());
                Rigidbody foodRb = food.GetComponent<Rigidbody>();
                if (foodRb != null)
                {
                    foodRb.isKinematic = true;
                    foodRb.useGravity = false;
                    //foodRb.linearVelocity = Vector3.zero;
                    //foodRb.angularVelocity = Vector3.zero;
                }
                food.transform.SetParent(objectContainer);
                food.transform.localPosition = Vector3.zero;
                //Debug.Log("Food placed in ObjectContainer!");
            }
        }
    }
}