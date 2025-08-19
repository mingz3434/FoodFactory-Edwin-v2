using UnityEngine;
using UnityEngine.Splines;

public class ConveyorMover : MonoBehaviour
{
    public SplineContainer spline;
    public float speed = 5f;
    public bool preserveMomentum = true;
    public float yOffset = 0.5f; // 食物在輸送帶上方的偏移
    public float distance = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (spline == null) return;

        distance += speed * Time.fixedDeltaTime / spline.Spline.GetLength();
        distance %= 1f;

        Vector3 basePosition = spline.EvaluatePosition(distance);
        Vector3 up = spline.EvaluateUpVector(distance);
        Vector3 position = basePosition + up * yOffset; // 應用偏移
        Vector3 tangent = spline.EvaluateTangent(distance);

        if (rb != null)
        {
            rb.MovePosition(position);
            if (preserveMomentum)
            {
                rb.linearVelocity = tangent.normalized * speed;
            }
        }
        else
        {
            transform.position = position;
        }

        transform.rotation = Quaternion.LookRotation(tangent);
    }
}

public class DestroyWhenEnd : MonoBehaviour
{
    public ConveyorMover mover;

    void Update()
    {
        if (mover.distance >= 0.99f)
        {
            Destroy(gameObject);
        }
    }
}