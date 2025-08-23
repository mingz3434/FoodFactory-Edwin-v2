using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;
using _ = GameInstance;
using Unity.Mathematics;

[AddComponentMenu("Controllers/Player Controller Game")]
public class PlayerController_Game : PlayerController{

   [ReadOnly] public GameState_Game gs; public GameState_Game GetGameState(){ return gs; }
   [ReadOnly] public PlayerCharacter_Game pChar;
   [ReadOnly] public TrajectoryLine trajectoryLine;
   [ReadOnly] public Hook hook;

   [Serializable] public struct TrajectorySettings{ public float maxDragDistance, launchPower, upwardAngle, maxAngle; }

   [Serializable] public struct Status{ public bool bIsDragging; public bool bProjectileRecastLocked; public Vector3 dragStartPosition; }

   TrajectorySettings trajs = new TrajectorySettings(){ maxDragDistance = 2f, launchPower = 10f, upwardAngle = 45f, maxAngle = 45f };
   public Status status;

   void Awake() { _.pc = this; }
   void Start() {

      pChar = _.pChar_Game;
      gs = _.gs as GameState_Game;

      trajectoryLine = TrajectoryLine.CreateTrajectoryLine(gs.prefabs.trajectoryLine_Prefab, pChar.transform);
      hook = Hook.CreateHook(gs.prefabs.hook_Prefab, pChar.hookContainerTransform);
   
      Action setPlayerStartingPos = () => {
         var spline = gs.splineContainer.Spline;
         pChar.transform.position = spline.EvaluatePosition(0);
         pChar.transform.Translate(new Vector3(0, 3, 0));
      };

      Action calculateMapCenter = () => {
         if (!gs.splineContainer) return;

         int samples = 100;
         Vector3 sum = Vector3.zero;
         for (int i = 0; i < samples; i++){
            float t = i / (float)samples;
            sum += (Vector3)gs.splineContainer.EvaluatePosition(t);
         }
         gs.splineCenter = sum / samples;
         gs.splineCenter.y = 3f;
      };

      setPlayerStartingPos();
      calculateMapCenter();

   }
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
      pChar.GetRigidbody().AddForce(move - pChar.GetRigidbody().linearVelocity, ForceMode.VelocityChange);
      pChar.GetRigidbody().MovePosition(targetPosition);

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

      //P: If it's food, no more move.
      if (hit.collider.CompareTag("Food")){
         var foodTray = hit.collider.gameObject.GetComponent<FoodTray>();
         foodTray.SnapTo(pChar.slotTransform, false);
         foodTray.bInTrack = false; // !!!!!!
         Debug.Log("Food placed in Player's Food Slot!");
      }
      
   }

   void MachineInteractionLogics(){

   }


   void EnableTrajectory_StartDragging(){
      if (this.status.bProjectileRecastLocked) return; // !!!!!!!!!!!!!!
      this.status.bIsDragging = true;
      this.status.dragStartPosition = Input.mousePosition; Debug.Log(Input.mousePosition);
      this.trajectoryLine.lineRenderer.enabled = true;
   }



   void TrajectoryLogics(){ //P: Including release mouse button logics

      //// prefer become pointing back to real method cuz here is inside the update
      //// but now will not do it cuz for better managing.

      Action<Vector3, Vector3> updateDrawTrajectoryLine_LineRenderer = (startPos, velocityCombined) => {
         var points = new Vector3[50];
         for (int i = 0; i < 50; i++){ //P: hardcode 50 temporary
            float time = i * .05f; //P: Est. each .05s timeframe as segment of trajectory
            points[i] = startPos + velocityCombined * time + 0.5f * Physics.gravity * time * time;
         }
         this.trajectoryLine.lineRenderer.SetPositions(points);
      };

      var velocityCombined = GetVelocityCombined_By_Calculating_DragDistance();

      updateDrawTrajectoryLine_LineRenderer( GetFiringStartPosition(), velocityCombined );

      //P: If release mouse left btn, fire.
      if (Input.GetMouseButtonUp(0)){
         this.status.bIsDragging = false;
         this.trajectoryLine.lineRenderer.enabled = false; // hide trajectory line
         FireProjectile(velocityCombined);
      }
   }


   void FireProjectile(Vector3 velocity){

      // Fire first food when having food on hand.
      if (pChar.slotTransform.childCount > 0 ){
         this.status.bProjectileRecastLocked = true;
         var firstFood = pChar.slotTransform.GetChild(0).gameObject; firstFood.transform.SetParent(null);
         var rb = firstFood.GetComponent<Rigidbody>(); rb.isKinematic = false; rb.useGravity = true; rb.mass = 1f; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
         rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!
         Timer.CreateTimer(this.gameObject, 3f, () => this.status.bProjectileRecastLocked = false );
      }

      // Fire a hook for getting something back.
      else{
         var hook = this.hook.gameObject; hook.transform.SetParent(null); this.status.bProjectileRecastLocked = true;
         var rb = hook.GetComponent<Rigidbody>(); rb.isKinematic = false; rb.useGravity = true;
         rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!
         Timer.CreateTimer(this.gameObject, 3f, () => {
            this.status.bProjectileRecastLocked = false;
            this.hook.ReattachHookContainer_ResetTransform(pChar.hookContainerTransform);
            this.hook.ResetRigidbody();
         });
      }
   }




   // ! Complex math getters

   Vector3 GetFiringStartPosition(){
      return pChar.slotTransform.childCount > 0 ? pChar.slotTransform.position : pChar.hookContainerTransform.position;
   }

   Vector3 GetVelocityCombined_By_Calculating_DragDistance(){
      //* Remarks: Here only calculating input delta, no lineRenderer involved.
      //P: Get world fly direction first.
      var delta = Input.mousePosition - this.status.dragStartPosition;
      var dragDistance = Math.Clamp(delta.magnitude / Screen.height, 0f, 2);
      var desiredDirection = -delta.normalized;

      var playerForward = pChar.transform.forward; //!!!
      var playerRight = pChar.transform.right; //!!!
      var inputForward = new Vector3(playerForward.x, 0, playerForward.z).normalized;
      var inputRight   = new Vector3(playerRight.x, 0, playerRight.z).normalized;

      // direction + input magnitude, not the real physics term of force.
      var rawFlyingForce_xComp = (desiredDirection.y * inputForward + desiredDirection.x * inputRight).normalized;

      //P: Limit fly direction within -45 to +45 degree.
      var angle = Vector3.SignedAngle(rawFlyingForce_xComp, inputForward, Vector3.up);

      if (Mathf.Abs(angle) > 45){
         var clampedAngle = Mathf.Sign(angle) * 45;
         rawFlyingForce_xComp = Quaternion.Euler(0, clampedAngle, 0) * inputForward;
      }

      //P: Calculate initial xComp velocity
      var velocity_xComponent = rawFlyingForce_xComp * dragDistance * this.trajs.launchPower;

      //P: Add yComp(height) and become vector combined
      var radian = this.trajs.upwardAngle * Mathf.Deg2Rad;
      var velocity_Combined = new Vector3(velocity_xComponent.x, velocity_xComponent.magnitude * Mathf.Sin(radian), velocity_xComponent.z * Mathf.Cos(radian));
      return velocity_Combined;
   }


}