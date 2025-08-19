using System;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;

using Unity.Mathematics;

[AddComponentMenu("Characters/Player Character Game")]
public class PlayerCharacter_Game : Character_Game{

   public GameState_Game gs;
   public float speed = 10f; public float mass = 1f;
   public SplineContainer splineContainer; public SplineContainer GetSplineContainer() { var gs = _.gs as GameState_Game; return gs.splineContainer; }
   Rigidbody rigidbody; public Rigidbody GetRigidbody() { return rigidbody; }
   public float splinePortionValue = 0f; // 當前在 Spline 上的位置 (0-1)
   public Transform slotTransform; // 用於存放食物的容器
   public Vector3 splineCenter; // 閉環的中心點

   // Hook 和 Slingshot 相關
   public Transform hookContainerTransform; // HookContainer 物件
   public Transform hookTransform; // Hook 物件（在 HookContainer 下）
   public float maxPullDistance = 2f; // 最大拉動距離
   public float launchPower = 10f; // 彈出力度倍數
   public float upwardAngle = 45f; // 拋物線向上角度 (度)
   public float maxAngle = 45f; // 限制彈出方向為玩家朝向的 ±45 度
   public bool isPulling = false;
   public Vector3 pullStartPos;
   public LineRenderer trajectoryLine; // 軌跡線
   public int trajectoryPoints = 50; // 軌跡點數
   public float trajectoryTimeStep = 0.05f; // 每點時間間隔
   public bool isHookLaunched = false; // 是否彈出 Hook

   void Awake() { _.pChar_Game = this; }

   void Start(){
      gs = _.gs as GameState_Game;

      Action generalStart = () => { //*
         this.rigidbody = GetComponent<Rigidbody>();
         this.rigidbody.mass = mass;
         this.rigidbody.useGravity = false;
         this.rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
         this.transform.position = new Vector3(transform.position.x, 3f, transform.position.z);
      };

      Action hookInit = () => {
         if (!this.hookContainerTransform) { this.hookContainerTransform = transform.Find("HookContainer"); }
         if (this.hookContainerTransform && !this.hookTransform) { this.hookTransform = hookContainerTransform.Find("Hook"); }
         if (this.hookTransform){
            Rigidbody hookRb = this.hookTransform.GetComponent<Rigidbody>();
            hookRb.useGravity = false;
            hookRb.isKinematic = true; // 初始靜止
         }
      };




      Action setPlayerStartingPos = () => {
         var spline = gs.splineContainer.Spline;
         this.transform.position = spline.EvaluatePosition(0);
      };


      Action calculateMapCenter = () => {
         if (!gs.splineContainer) return;

         int samples = 100;
         Vector3 sum = Vector3.zero;
         for (int i = 0; i < samples; i++){
            float t = i / (float)samples;
            sum += (Vector3)gs.splineContainer.EvaluatePosition(t);
         }
         gs.mapCenter = sum / samples;
         gs.mapCenter.y = 3f;
      };


      generalStart();
      hookInit();
      trajectoryInit();
      setPlayerStartingPos();
      calculateMapCenter();

   }
}


public class TrajectoryLine : Actor_Game {

   const int TRAJECTORY_POINTS = 50;
   const float TRAJECTORY_TIMESTEP = .05f;
   Vector3 pullStartPos;

   public static TrajectoryLine CreateTrajectoryLine(TrajectoryLine prefab, Transform parentTransform){
      var trajectory = Instantiate(prefab, parentTransform);
   }
}
public class Hook : Actor_Game{


}