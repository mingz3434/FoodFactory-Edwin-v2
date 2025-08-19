using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;
using Unity.Mathematics;
using _ = GameInstance;

[AddComponentMenu("Controllers/Player Controller Game")]
public class PlayerController_Game : PlayerController{
   public float speed = 10f; public float mass = 1f;
   public SplineContainer splineContainer; public SplineContainer GetSplineContainer() { return splineContainer; }
   Rigidbody rigidbody; public Rigidbody GetRigidbody() { return rigidbody; }
   private float splinePortionValue = 0f; // 當前在 Spline 上的位置 (0-1)
   public Transform slotTransform; // 用於存放食物的容器
   private Vector3 splineCenter; // 閉環的中心點

   // Hook 和 Slingshot 相關
   public Transform hookContainerTransform; // HookContainer 物件
   public Transform hookTransform; // Hook 物件（在 HookContainer 下）
   public float maxPullDistance = 2f; // 最大拉動距離
   public float launchPower = 10f; // 彈出力度倍數
   public float upwardAngle = 45f; // 拋物線向上角度 (度)
   public float maxAngle = 45f; // 限制彈出方向為玩家朝向的 ±45 度
   private bool isPulling = false;
   private Vector3 pullStartPos;
   private LineRenderer trajectoryLine; // 軌跡線
   private int trajectoryPoints = 50; // 軌跡點數
   private float trajectoryTimeStep = 0.05f; // 每點時間間隔
   private bool isHookLaunched = false; // 是否彈出 Hook

   void Start(){
      Action generalStart = () => {
         rigidbody = GetComponent<Rigidbody>();
         rigidbody.mass = mass;
         rigidbody.useGravity = false;
         rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
         transform.position = new Vector3(transform.position.x, 3f, transform.position.z);
      };

      Action hookInit = () => {
         if (!hookContainerTransform) { hookContainerTransform = transform.Find("HookContainer"); }
         if (hookContainerTransform&& !hookTransform) { hookTransform = hookContainerTransform.Find("Hook"); }
         if (hookTransform){
            Rigidbody hookRb = hookTransform.GetComponent<Rigidbody>();
            hookRb.useGravity = false;
            hookRb.isKinematic = true; // 初始靜止
         }
      };

      Action trajectoryInit = () => {
         trajectoryLine = gameObject.AddComponent<LineRenderer>();
         trajectoryLine.positionCount = trajectoryPoints;
         trajectoryLine.enabled = false;
         trajectoryLine.startWidth = 0.1f;
         trajectoryLine.endWidth = 0.1f;
         trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
         trajectoryLine.startColor = Color.yellow;
         trajectoryLine.endColor = Color.red;
      };


      Action playerPosInit_OnSpline = () => {
         float3 playerPos = new float3(transform.position.x, transform.position.y, transform.position.z);
         SplineUtility.GetNearestPoint(splineContainer.Spline, playerPos, out float3 nearestPoint, out float initialT);
         splinePortionValue = initialT;
      };


      Action calculateSplineCenter = () => {
         if (!splineContainer) return;

         int samples = 100;
         Vector3 sum = Vector3.zero;
         for (int i = 0; i < samples; i++){
            float t = i / (float)samples;
            sum += (Vector3)splineContainer.EvaluatePosition(t);
         }
         splineCenter = sum / samples;
         splineCenter.y = 3f;
      };


      generalStart();
      hookInit();
      trajectoryInit();
      playerPosInit_OnSpline();
      calculateSplineCenter();

   }


   void FixedUpdate(){
      //! All fixed update for move only.

      float input = Input.GetAxis("Horizontal"); //P: horizontal input
      splinePortionValue += input * speed * Time.fixedDeltaTime / splineContainer.Spline.GetLength();
      splinePortionValue = Mathf.Repeat(splinePortionValue, 1f);

      Vector3 targetPosition = splineContainer.EvaluatePosition(splinePortionValue);
      targetPosition.y = 3f;
      Vector3 tangent = math.normalize(splineContainer.EvaluateTangent(splinePortionValue));

      // 應用移動
      Vector3 move = tangent * input * speed;
      rigidbody.AddForce(move - rigidbody.linearVelocity, ForceMode.VelocityChange);
      rigidbody.MovePosition(targetPosition);

      // 面向閉環中心點
      Vector3 directionToCenter = splineCenter - transform.position;
      directionToCenter.y = 0;
      if (directionToCenter != Vector3.zero){
         transform.rotation = Quaternion.LookRotation(directionToCenter, Vector3.up);
      }
   }

   void Update(){
      if (Input.GetKeyDown(KeyCode.Space)) { PickUpFoodLogics(); }

      if (Input.GetKeyDown(KeyCode.E)) { MachineInteractionLogics(); }

      if (Input.GetMouseButtonDown(0)) { DrawNewTrajectoryLogics(); }

      if (isPulling) { OpearateTrajectoryLogics(); } //! including release logics at the end.
   }


   void PickUpFoodLogics(){

      // Confirm empty slot.
      if (slotTransform.childCount > 0) { Debug.Log("ObjectContainer is full!"); return; }

      // Get food below player.
      Ray ray = new Ray(transform.position, Vector3.down);
      RaycastHit hit;
      Physics.Raycast(ray, out hit, 4f);

      // Return when null.
      if(hit.collider == null) {Debug.Log("No hit"); return; }

      // If it's food, destroy it's conveyor mover comp, then set its rb to desired state.
      if (hit.collider.CompareTag("Food")){
         GameObject food = hit.collider.gameObject;
         Destroy(food.GetComponent<ConveyorMover>());
         Rigidbody foodRb = food.GetComponent<Rigidbody>();
         if (foodRb){
            foodRb.isKinematic = true;
            foodRb.useGravity = false;
            foodRb.linearVelocity = Vector3.zero;
            foodRb.angularVelocity = Vector3.zero;
         }
         food.transform.SetParent(slotTransform);
         food.transform.localPosition = Vector3.zero;
         Debug.Log("Food placed in ObjectContainer!");
      }
      
   }

   void MachineInteractionLogics(){
      Machine[] allMachines = FindObjectsOfType<Machine>();
      foreach (Machine machine in allMachines){

         //! Case: Mixer
         if (machine.type == Machine.MachineType.Mixer){
            if (machine.needsInput) { machine.StopMixer(); }
            else{ // 運作期間按 E，銷毀食物
               for (int i = machine.slotPositions.Length - 1; i >= 0; i--){
                  Food food = machine.GetFoodAtSlot(i);
                  if (food != null){
                     Destroy(food.gameObject);
                     machine.RemoveFood(i);
                     Debug.Log("Mixer: Food destroyed during processing.");
                  }
               }
            }
         }

      }
   }


   void DrawNewTrajectoryLogics(){
      if (isHookLaunched) return; // Hook 未返回時禁止拉動
      isPulling = true;
      pullStartPos = Input.mousePosition;
      trajectoryLine.enabled = true;
   }

   void OpearateTrajectoryLogics(){ //P: including release mouse button logics
      Vector3 pullDelta = Input.mousePosition - pullStartPos;
      float pullDistance = Mathf.Clamp(pullDelta.magnitude / Screen.height, 0f, maxPullDistance);
      Vector3 pullDirection = -pullDelta.normalized;

      // 將螢幕空間方向映射到玩家所在平面（y=0）
      Vector3 playerForward = transform.forward;
      playerForward.y = 0;
      playerForward.Normalize();
      Vector3 playerRight = transform.right;
      playerRight.y = 0;
      playerRight.Normalize();

      // 映射：y+（向上拖拽）對應 playerForward，x 對應 playerRight
      Vector3 worldPullDir = (pullDirection.x * playerRight + pullDirection.y * playerForward).normalized;

      // 限制在玩家朝向的 ±45 度範圍
      float angle = Vector3.SignedAngle(worldPullDir, playerForward, Vector3.up);
      if (Mathf.Abs(angle) > maxAngle){
         float clampedAngle = Mathf.Sign(angle) * maxAngle;
         worldPullDir = Quaternion.Euler(0, clampedAngle, 0) * playerForward;
      }

      // 計算初速度（與拉動方向一致）
      Vector3 velocity = worldPullDir * pullDistance * launchPower;

      // 添加向上分量
      float rad = upwardAngle * Mathf.Deg2Rad;
      velocity = new Vector3(velocity.x, velocity.magnitude * Mathf.Sin(rad), velocity.z * Mathf.Cos(rad));

      // 確定軌跡起始點
      Vector3 startPos = slotTransform.childCount > 0 ? slotTransform.position : hookContainerTransform.position;

      // 更新軌跡線
      UpdateDrawTrajectoryLine(startPos, velocity);

      if (Input.GetMouseButtonUp(0)){
         isPulling = false;
         trajectoryLine.enabled = false;
         FireProjectile(velocity);
      }
   }

   void UpdateDrawTrajectoryLine(Vector3 startPos, Vector3 velocity){
      Vector3[] points = new Vector3[trajectoryPoints];
      for (int i = 0; i < trajectoryPoints; i++){
         float time = i * trajectoryTimeStep;
         points[i] = startPos + velocity * time + 0.5f * Physics.gravity * time * time;
      }
      trajectoryLine.SetPositions(points);
   }

   void FireProjectile(Vector3 velocity){
      GameObject projectile = null;
      Rigidbody projRb = null;

      if (slotTransform.childCount > 0){
         // 彈出 ObjectContainer 中的第一個物件
         projectile = slotTransform.GetChild(0).gameObject;
         projectile.transform.SetParent(null);
         projRb = projectile.GetComponent<Rigidbody>();
         if (projRb == null) { projRb = projectile.AddComponent<Rigidbody>(); }
         projRb.isKinematic = false;
         projRb.useGravity = true;
         projRb.mass = 1f;
         projRb.linearVelocity = Vector3.zero;
         projRb.angularVelocity = Vector3.zero;
         projRb.AddForce(velocity, ForceMode.VelocityChange);
      }
      else if (hookTransform != null && !isHookLaunched){
         // 彈出 Hook
         projectile = hookTransform.gameObject;
         projectile.transform.SetParent(null);
         isHookLaunched = true;
         projRb = projectile.GetComponent<Rigidbody>();
         projRb.isKinematic = false;
         projRb.useGravity = true;
         projRb.AddForce(velocity, ForceMode.VelocityChange);
         StartCoroutine(AutoResetHook(3f));
      }
   }

   public void ResetHook(){
      if (hookTransform == null || hookContainerTransform == null) return;

      isHookLaunched = false;
      hookTransform.transform.SetParent(hookContainerTransform);
      hookTransform.transform.localPosition = Vector3.zero;
      hookTransform.transform.localRotation = Quaternion.identity;
      Rigidbody hookRb = hookTransform.GetComponent<Rigidbody>();
      if (hookRb != null){
         hookRb.useGravity = false;
         hookRb.isKinematic = true;
         hookRb.linearVelocity = Vector3.zero;
         hookRb.angularVelocity = Vector3.zero;
      }
   }

   IEnumerator AutoResetHook(float delay){
      yield return new WaitForSeconds(delay);
      ResetHook();
   }


}