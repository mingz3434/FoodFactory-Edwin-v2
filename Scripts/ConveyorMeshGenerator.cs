using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class ConveyorMeshGenerator : MonoBehaviour
{
    public SplineContainer spline;
    public GameObject segmentPrefab; // 簡單平面預製體（e.g., Quad）
    public GameObject startSegPrefab;
    public float segmentLength = 1f; // 每個段的長度
    public float width = 1f;        // 輸送帶寬度

    void Start()
    {
        if (spline == null || segmentPrefab == null) return;

        float splineLength = spline.Spline.GetLength();
        int segmentCount = Mathf.CeilToInt(splineLength / segmentLength);



        for (int i = 0; i < segmentCount; i++)
        {
            float t = i * segmentLength / splineLength;
            Vector3 position = spline.EvaluatePosition(t);
            Vector3 tangent = spline.EvaluateTangent(t);
            Vector3 up = spline.EvaluateUpVector(t); // 使用 Spline 的上向量

            GameObject segment = null;
            if (i == 0)
            {
                
                segment = Instantiate(startSegPrefab, position, Quaternion.identity, transform);
                segment.transform.rotation = Quaternion.LookRotation(tangent, up);               
                segment.transform.localScale = new Vector3(width, 2f, segmentLength);
                Vector3 offset = new Vector3(0f, segment.transform.localScale.y / 2, 0f);
                segment.transform.position = position + offset; 

            }
            else
            {
                segment = Instantiate(segmentPrefab, position, Quaternion.identity, transform);
                segment.transform.rotation = Quaternion.LookRotation(tangent, up);
                segment.transform.Rotate(90f, 0f, 0f, Space.Self);
                segment.transform.localScale = new Vector3(width, 1f, segmentLength);
            }
        }
    }
}