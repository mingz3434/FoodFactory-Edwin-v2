using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;
using _ = GameInstance;
using Unity.Mathematics;

[AddComponentMenu("Controllers/Player Controller Game")]
public class PlayerController_Game : PlayerController{

   GameState_Game gs;
   PlayerCharacter_Game pChar;
   public Camera camera;

   [Serializable] public struct TrajectorySettings{ public float maxDragDistance, launchPower, upwardAngle, maxAngle; }

   [Serializable] public struct Status{ public bool bIsDragging; public bool bHookHasLaunched; public Vector3 dragStartPosition; }

   TrajectorySettings trajs = new TrajectorySettings(){ maxDragDistance = 2f, launchPower = 10f, upwardAngle = 45f, maxAngle = 45f };
   Status status;

   void Awake() { _.pc = this; }
   void Start() { pChar = _.pChar_Game; gs = _.gs as GameState_Game; }
   void FixedUpdate(){
      //! Fixed update for move only.
      if(!pChar) return;

      var sc = gs.splineContainer;

      float input = Input.GetAxis("Horizontal"); //P: horizontal input
      pChar.portionValue += input * pChar.speed * Time.fixedDeltaTime / sc.Spline.GetLength();
      pChar.portionValue = Mathf.Repeat(pChar.portionValue, 1f); //?

      Vector3 targetPosition = sc.EvaluatePosition(pChar.portionValue);
      targetPosition.y = 3f;
      Vector3 tangent = math.normalize(sc.EvaluateTangent(pChar.portionValue));

      // Move
      Vector3 move = tangent * input * pChar.speed;
      pChar.GetComponent<Rigidbody>().AddForce(move - pChar.GetComponent<Rigidbody>().linearVelocity, ForceMode.VelocityChange);
      pChar.GetComponent<Rigidbody>().MovePosition(targetPosition);

      // Always face center
      Vector3 directionToCenter = gs.splineCenter - pChar.transform.position;
      directionToCenter.y = 0;
      if (directionToCenter != Vector3.zero){
         var temp = Quaternion.LookRotation(directionToCenter, Vector3.up);
         pChar.transform.rotation = Quaternion.Slerp(pChar.transform.rotation, temp, 0.1f);
      }
   }

   void Update(){
      if (Input.GetKeyDown(KeyCode.Space)) { PickUpFoodLogics(); }

      if (Input.GetKeyDown(KeyCode.E)) { MachineInteractionLogics(); }

      if (Input.GetMouseButtonDown(0)) { EnableTrajectory_StartDragging(); }

      if (this.status.bIsDragging) { TrajectoryLogics(); } //! including release logics at the end.
   }


   void PickUpFoodLogics(){

      //P: Confirm empty slot.
      if (pChar.slotTransform.childCount > 0) { Debug.Log("ObjectContainer is full!"); return; }

      //P: Get food below player.
      Ray ray = new Ray(pChar.transform.position, Vector3.down);
      RaycastHit hit;
      Physics.Raycast(ray, out hit, 4f);

      //P: Return when null.
      if(hit.collider == null) {Debug.Log("PC: PickUpFood: No hit collider."); return; }

      //P: If it's food, destroy it's conveyor mover comp, then set its rb to desired state.
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
         food.transform.SetParent(pChar.slotTransform);
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


   void EnableTrajectory_StartDragging(){
      if (this.status.bHookHasLaunched) return; // Hook 未返回時禁止拉動
      this.status.bIsDragging = true;
      this.status.dragStartPosition = Input.mousePosition;
      pChar.trajectoryLine.enabled = true;
   }



   void TrajectoryLogics(){ //P: Including release mouse button logics

      Action<Vector3, Vector3> updateDrawTrajectoryLine_LineRenderer = (startPos, velocityCombined) => {
         var points = new Vector3[50];
         for (int i = 0; i < 50; i++){ //P: hardcode 50 temporary
            float time = i * .05f; //P: Est. each .05s timeframe as segment of trajectory
            points[i] = startPos + velocityCombined * time + 0.5f * Physics.gravity * time * time;
         }
         pChar.trajectoryLine.lineRenderer.SetPositions(points);
      };

      var velocityCombined = GetVelocity_Combined_ByCalculating_DragDistance();

      updateDrawTrajectoryLine_LineRenderer( GetFiringStartPosition(), velocityCombined );

      //P: If release mouse left btn, fire.
      if (Input.GetMouseButtonUp(0)){
         this.status.bIsDragging = false;
         pChar.trajectoryLine.enabled = false;
         FireProjectile(velocityCombined);
      }
   }


   void FireProjectile(Vector3 velocity){
      if (pChar.slotTransform.childCount > 0){
         var firstFood = pChar.slotTransform.GetChild(0).gameObject; firstFood.transform.SetParent(null);
         var rb = firstFood.GetComponent<Rigidbody>(); rb.isKinematic = false; rb.useGravity = true; rb.mass = 1f; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
         rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!
      }
      else if (pChar.hook.transform != null && !this.status.bHookHasLaunched){
         var hook = pChar.hook.gameObject; hook.transform.SetParent(null); this.status.bHookHasLaunched = true;
         var rb = hook.GetComponent<Rigidbody>(); rb.isKinematic = false; rb.useGravity = true;
         rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!
         Timer.CreateTimer(hook.gameObject, 3f, () => ResetHook() );
      }
   }

   public void ResetHook(){
      if (pChar.hook.transform == null || pChar.hookContainerTransform == null) return;

      this.status.bHookHasLaunched = false;
      pChar.hook.transform.SetParent(pChar.hookContainerTransform);
      pChar.hook.transform.localPosition = Vector3.zero;
      pChar.hook.transform.localRotation = Quaternion.identity;
      Rigidbody hookRb = pChar.hook.GetComponent<Rigidbody>();
      if (hookRb != null){
         hookRb.useGravity = false;
         hookRb.isKinematic = true;
         hookRb.linearVelocity = Vector3.zero;
         hookRb.angularVelocity = Vector3.zero;
      }
   }









   Vector3 GetFiringStartPosition(){
      return pChar.slotTransform.childCount > 0 ? pChar.slotTransform.position : pChar.hookContainerTransform.position;
   }

   Vector3 GetVelocity_Combined_ByCalculating_DragDistance(){
      //* Remarks: Here only calculating input delta, no lineRenderer involved.
      //P: Get world fly direction first.
      var delta = Input.mousePosition - this.status.dragStartPosition;
      var dragDistance = Math.Clamp(delta.magnitude / Screen.height, 0f, 2);
      var flyDirection = -delta.normalized;

      var tForward = this.transform.forward;
      var tRight = this.transform.right;
      var flatForward = new Vector3(tForward.x, 0, tForward.z).normalized;
      var flatRight   = new Vector3(tRight.x, 0, tRight.z).normalized;

      var worldFlyDirection = (flyDirection.y * flatForward + flyDirection.x * flatRight).normalized;

      //P: Limit fly direction within -45 to +45 degree.
      var angle = Vector3.SignedAngle(worldFlyDirection, flatForward, Vector3.up);

      if (Mathf.Abs(angle) > 45){
         var clampedAngle = Mathf.Sign(angle) * 45;
         worldFlyDirection = Quaternion.Euler(0, clampedAngle, 0) * flatForward;
      }

      //P: Calculate initial xComp velocity
      var velocity_xComponent = worldFlyDirection * dragDistance * this.trajs.launchPower;

      //P: Add yComp(height) and become vector combined
      var radian = this.trajs.upwardAngle * Mathf.Deg2Rad;
      var velocity_Combined = new Vector3(velocity_xComponent.x, velocity_xComponent.magnitude * Mathf.Sin(radian), velocity_xComponent.z * Mathf.Cos(radian));
      return velocity_Combined;
   }
}