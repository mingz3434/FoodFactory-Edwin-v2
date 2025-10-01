// using UnityEngine;
// using UnityEngine.InputSystem;
// using System;
// using Mirror;
// using _ = GameInstance;

// public partial class ThirdPerson_PC : NetworkBehaviour{

//    [Serializable] public class UserInterface { public HUD_Game hud_Inst; public Transform canvasTransform; }
//    [Serializable] public class Extras { public Hook hook; public TrajectoryLine trajectoryLine; public Transform hookContainerTransform, foodTraySlotTransform; }
//    [Serializable] public class TrajectoryParams{ public float MAX_DRAG_DISTANCE = 2f, LAUNCH_POWER = 10f, UPWARD_ANGLE= 45f, MAX_ANGLE = 45f; [ReadOnly] public Vector3 dragStartPosition, dragging_VelocityCombined; }
//    [Serializable] public class Status{ public bool bIsHookDragging, bProjectileRecastable = true; }
//    public UserInterface ui;
//    public Extras extras;
//    public TrajectoryParams trajs;
//    public Status status;

//    void Update_P2(){
//       if (iv.bHookDragging) OnHookDragging();
//       if (iv.bFoodTrayDragging) OnFoodTrayDragging();
//    }

//    void OnHookDraggingToggle_KeyDown_G(InputAction.CallbackContext context){
//       if(!status.bProjectileRecastable) return;
//       iv.bHookDragging = true;
//       this.playerInput.actions["Look"].Disable();
//       this.playerInput.actions["DragXY"].Enable();
//       var mousePosition = Input.mousePosition;
//       this.trajs.dragStartPosition = mousePosition; Debug.Log(mousePosition);
//       this.extras.trajectoryLine.lineRenderer.enabled = true;
//    }

//    void OnHookDraggingToggle_KeyUp_G(InputAction.CallbackContext context){
//       if (!iv.bHookDragging) return;
//       FireHook(this.trajs.dragging_VelocityCombined);
//    }

//    void OnPickFood_or_OnFoodTrayDraggingToggle_KeyDown_F(InputAction.CallbackContext context){
//       if(!status.bProjectileRecastable) return;

//       // Pick Food
//       if (!(this.extras.foodTraySlotTransform.childCount > 0)) {
//          Debug.Log("No food on hand! Now try pick food");
//          Physics.Raycast(character.transform.position, character.transform.forward + Vector3.down*.5f, out RaycastHit hit, 4f);
//          if (!hit.collider) return;
//          var tray = hit.collider.GetComponent<FoodTray>();
//          if (!tray) { Debug.Log("No foodTray in front of you!"); return; }
//          tray.SnapTo(extras.foodTraySlotTransform);
//          tray.Set_NoMoreInTrack(); // !!!!!!
//          Debug.Log("Food placed in Player's Food Slot!");
//          return;
//       }

//       // Drag Food Tray to throw
//       iv.bFoodTrayDragging = true;
//       this.playerInput.actions["Look"].Disable();
//       this.playerInput.actions["DragXY"].Enable();
//       var mousePosition = Input.mousePosition;
//       this.trajs.dragStartPosition = mousePosition; Debug.Log(mousePosition);
//       this.extras.trajectoryLine.lineRenderer.enabled = true;
//    }

//    void OnFoodTrayDraggingToggle_KeyUp_F(InputAction.CallbackContext context){
//       if (!iv.bFoodTrayDragging) return;
//       FireFoodPlate(this.trajs.dragging_VelocityCombined);
//    }

//    void OnHookDragging(){
//       trajs.dragging_VelocityCombined = GetVelocityCombined_By_Calculating_DragDistance();
//       UpdateDrawTrajectoryLine_LineRenderer(this.extras.hookContainerTransform.position, trajs.dragging_VelocityCombined);
//    }

//    void OnFoodTrayDragging(){
//       trajs.dragging_VelocityCombined = GetVelocityCombined_By_Calculating_DragDistance();
//       UpdateDrawTrajectoryLine_LineRenderer(this.extras.foodTraySlotTransform.position, trajs.dragging_VelocityCombined);
//    }

//    void UpdateDrawTrajectoryLine_LineRenderer(Vector3 startPos, Vector3 velocityCombined){
//       var points = new Vector3[50];
//       for (int i = 0; i < 50; i++){ //P: hardcode 50 temporary
//          float time = i * .05f; //P: Est. each .05s timeframe as segment of trajectory
//          points[i] = startPos + velocityCombined * time + 0.5f * Physics.gravity * time * time;
//       }
//       this.extras.trajectoryLine.lineRenderer.SetPositions(points);
//    }


//    void FireHook(Vector3 velocity){
//       var hook = this.extras.hook;
//       hook.transform.SetParent(null);
//       this.status.bProjectileRecastable = false;

//       hook.RB_Activate();

//       hook.rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!

//       // Reset PC+PI
//       iv.bHookDragging = false;
//       this.extras.trajectoryLine.lineRenderer.enabled = false;
//       this.playerInput.actions["Look"].Enable();
//       this.playerInput.actions["DragXY"].Disable();

//       // Reset Hook + set now recastable
//       Timer.CreateTimer_Physics(this.gameObject, 3f, () => {
//          this.SetStatus_Recastable();
//          this.extras.hook.ReattachHookContainer_ResetTransform(extras.hookContainerTransform);
//          this.extras.hook.RB_ResetStatic();
//       });
//    }


//    void FireFoodPlate(Vector3 velocity){
//       var firstFood_FoodTray = this.extras.foodTraySlotTransform.GetChild(0).gameObject.GetComponent<FoodTray>();
//       firstFood_FoodTray.transform.SetParent(null);
//       this.status.bProjectileRecastable = false;

//       firstFood_FoodTray.RB_Activate();

//       firstFood_FoodTray.rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!

//       // Reset PC+PI
//       iv.bFoodTrayDragging = false;
//       this.extras.trajectoryLine.lineRenderer.enabled = false;
//       this.playerInput.actions["Look"].Enable();
//       this.playerInput.actions["DragXY"].Disable();


//       Timer.CreateTimer_Physics(this.gameObject, 3f, () => {
//          this.SetStatus_Recastable();
//       });
     

//    }



//    // ! Complex math getters
//    Vector3 GetVelocityCombined_By_Calculating_DragDistance(){
//       //* Remarks: Here only calculating input delta, no lineRenderer involved.
//       //P: Get world fly direction first.
//       var delta = Input.mousePosition - this.trajs.dragStartPosition;
//       var dragDistance = Math.Clamp(delta.magnitude / Screen.height, 0f, 2);
//       var desiredDirection = -delta.normalized;

//       var playerForward = character.transform.forward; //!!!
//       var playerRight = character.transform.right; //!!!
//       var inputForward = new Vector3(playerForward.x, 0, playerForward.z).normalized;
//       var inputRight   = new Vector3(playerRight.x, 0, playerRight.z).normalized;

//       // direction + input magnitude, not the real physics term of force.
//       var rawFlyingForce_xComp = (desiredDirection.y * inputForward + desiredDirection.x * inputRight).normalized;

//       //P: Calculate initial xComp velocity
//       var velocity_xComponent = rawFlyingForce_xComp * dragDistance * this.trajs.LAUNCH_POWER;

//       //P: Add yComp(height) and become vector combined
//       var radian = this.trajs.UPWARD_ANGLE * Mathf.Deg2Rad;
//       var velocity_Combined = new Vector3(velocity_xComponent.x, velocity_xComponent.magnitude * Mathf.Sin(radian), velocity_xComponent.z * Mathf.Cos(radian));
//       return velocity_Combined;
//    }

//    public void SetStatus_Recastable(){
//       this.status.bProjectileRecastable = true;
//    }

//    public void SetStatus_NotRecastable(){
//       this.status.bProjectileRecastable = false;
//    }
// }