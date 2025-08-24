using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player; // ���a Transform
    public SplineContainer spline; // ��e�a Spline
    public float followDistance = 5f; // ��v���P���a�������Z��
    public float followHeight = 3f; // ��v���۹缾�a���������� (y+3)
    public float smoothTime = 0.3f; // ���Ƹ��H�ɶ�
    public float lookDownAngle = 45f; // �V�U�ɨ��]�ס^

    private Vector3 velocity; // �Ω� SmoothDamp
    private Vector3 centerPoint; // Spline �����I

    void Start()
    {
        if (spline == null || player == null)
        {
            Debug.LogError("Spline or Player not assigned!");
            return;
        }

        // �p�� Spline �����I
        centerPoint = CalculateSplineCenter();
    }

    void LateUpdate()
    {
        if (spline == null || player == null) return;

        // �p����v���ؼЦ�m�]���a��W��^
        Vector3 playerForward = player.forward; // ���a�¦V
        Vector3 targetPosition = player.position + new Vector3(0f, followHeight, 0f) - playerForward * followDistance;

        // ���Ʋ�����v��
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // �p��¦V�����I����V
        Vector3 toCenter = centerPoint - transform.position;
        // ����45�פU�ɨ�
        Quaternion targetRotation = Quaternion.LookRotation(toCenter);
        targetRotation = Quaternion.Euler(lookDownAngle, targetRotation.eulerAngles.y, 0f);

        // ���Ʊ�����v��
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTime);
    }

    Vector3 CalculateSplineCenter()
    {
        if (spline.Spline == null || spline.Spline.Count == 0)
            return Vector3.zero;

        float3 center = float3.zero;
        int knotCount = spline.Spline.Count;

        // �M���Ҧ��`�I�A�p�⥭����m
        for (int i = 0; i < knotCount; i++)
        {
            center += spline.Spline[i].Position;
        }
        center /= knotCount;

        return new Vector3(center.x, 0f, center.z); // �����I�O�� y=0
    }
}