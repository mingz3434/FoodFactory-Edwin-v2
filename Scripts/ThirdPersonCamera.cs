using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class ThirdPersonCamera : MonoBehaviour{
   public Transform player; // 玩家 Transform
   public SplineContainer spline; // 輸送帶 Spline
   public float followDistance = 5f; // 攝影機與玩家的水平距離
   public float followHeight = 3f; // 攝影機相對玩家的垂直偏移 (y+3)
   public float smoothTime = 0.3f; // 平滑跟隨時間
   public float lookDownAngle = 45f; // 向下傾角（度）

   private Vector3 velocity; // 用於 SmoothDamp
   private Vector3 centerPoint; // Spline 中心點

   void Start(){
      if (spline == null || player == null){
         Debug.LogError("Spline or Player not assigned!");
         return;
      }

      // 計算 Spline 中心點
      centerPoint = CalculateSplineCenter();
   }

   void LateUpdate(){
      if (spline == null || player == null) return;

      // 計算攝影機目標位置（玩家後上方）
      Vector3 playerForward = player.forward; // 玩家朝向
      Vector3 targetPosition = player.position + new Vector3(0f, followHeight, 0f) - playerForward * followDistance;

      // 平滑移動攝影機
      transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

      // 計算朝向中心點的方向
      Vector3 toCenter = centerPoint - transform.position;
      // 應用45度下傾角
      Quaternion targetRotation = Quaternion.LookRotation(toCenter);
      targetRotation = Quaternion.Euler(lookDownAngle, targetRotation.eulerAngles.y, 0f);

      // 平滑旋轉攝影機
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTime);
   }

   Vector3 CalculateSplineCenter(){
      if (spline.Spline == null || spline.Spline.Count == 0) return Vector3.zero;

      float3 center = float3.zero;
      int knotCount = spline.Spline.Count;

      // 遍歷所有節點，計算平均位置
      for (int i = 0; i < knotCount; i++) { center += spline.Spline[i].Position; }
      center /= knotCount;

      return new Vector3(center.x, 0f, center.z); // 中心點保持 y=0
   }
}