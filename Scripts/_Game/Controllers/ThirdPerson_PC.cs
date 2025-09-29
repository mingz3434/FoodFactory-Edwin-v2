using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Mirror;
using _ = GameInstance;

//!!! Bind grounds.groundLayers, cameras.cinemachineCameraTarget, audios.landingAudioClip, audios.footstepAudioClips in Inspector
//!!! groundLayers to "Default" and cct to "CameraRoot" plssssssss!!!

public class ThirdPerson_PC : NetworkBehaviour {

   [SyncVar] public int playerId = -1;
   public Sprite[] playerIcons_Sprite; public SpriteRenderer headFlag_SR;
   public Material[] playerCharacters_Material; public SkinnedMeshRenderer smr_Body; public MeshRenderer mr_LeftWeapon, mr_RightWeapon;
   [Serializable] public class MovementSettings { public float moveSpeed = 4.0f, sprintSpeed = 5.355f, rotationSmoothTime = .12f; [ReadOnly] public float rotationVelocity; }
   [Serializable] public class CC_Cust_JumpSettings { public float JUMP_HEIGHT = 1.2f, GRAVITY = -15f, JUMP_TIMEOUT = .5f, FALL_TIMEOUT = .15f, TERMINAL_VELOCITY = -50f; public Transform characterFootTransform; public LayerMask GROUND_LAYER; [ReadOnly] public float verticalVelocity; [ReadOnly] public bool bIsGrounded, bIsJumping; }




   [Serializable] public class CameraSettings { public GameObject cinemachineCameraTarget; public float topClamp = 70f, bottomClamp = -30f, cameraAngleOverride = 0f; public bool lockCameraPosition = false; [ReadOnly] public float cinemachineTargetYaw, cinemachineTargetPitch; }
   [Serializable] public class AudioSettings { public AudioClip landingAudioClip; public AudioClip[] footstepAudioClips; public float footstepAudioVolume = .5f; }
   [Serializable] public class AnimSettings { [ReadOnly] public float blendedValue; [HideInInspector] public int speed_Id, loco_Id, jumping_Id, punching_Id; public bool bLoco, bJumping; }
   [Serializable] public class InputValues { public bool bLocal; public Vector2 move, look; public bool bSprint; }

   public enum AnimationState { Locomotion, Jumping, Punching }
   public AnimationState animationState = AnimationState.Locomotion;

   public MovementSettings movements;
   public CC_Cust_JumpSettings _j;
   public CameraSettings cameras;
   public AudioSettings audios;
   public AnimSettings _a;

   PlayerInput playerInput;
   public InputValues iv = new InputValues();

   public Animator animator;
   public CharacterController characterController;
   public GameObject mainCamera;
   public GameObject followCamScript;
   public GameObject character;
   public GameObject characterMesh_GO;


   private const float THRESHOLD = 0.01f;

   public System.Random random = new System.Random();

   #region //! PART 2
   [Serializable] public class UserInterface { public HUD_Game hud_Inst; public Transform canvasTransform; }
   [Serializable] public class Extras { public Hook hook; public TrajectoryLine trajectoryLine; public Transform hookContainerTransform; }
   [Serializable] public struct TrajectorySettings{ public float maxDragDistance, launchPower, upwardAngle, maxAngle; }
   [Serializable] public struct Status{ public bool bIsDragging; public bool bProjectileRecastLocked; public Vector3 dragStartPosition; }
   public UserInterface ui;
   public Extras extras;
   public TrajectorySettings trajs;
   public Status status;
   #endregion


   #region Input region
   void Awake(){ }

   void LateEnable() { OnLateEnable(); }
   void OnLateEnable() {
      if(playerInput){
         playerInput.actions["Jump"].started += OnJump_KeyDown;
         playerInput.actions["Sprint"].started += OnSprint_KeyDown;
         playerInput.actions["Sprint"].canceled += OnSprint_KeyUp;
      }
   }

   void OnDisable() {
      if(playerInput){         
         playerInput.actions["Jump"].started -= OnJump_KeyDown;
         playerInput.actions["Sprint"].started -= OnSprint_KeyDown;
         playerInput.actions["Sprint"].canceled -= OnSprint_KeyUp;
      }
   }

   public void OnMove(InputAction.CallbackContext context){ iv.move = context.ReadValue<Vector2>(); }
   public void OnLook(InputAction.CallbackContext context){ iv.look = context.ReadValue<Vector2>(); }
   void OnJump_KeyDown(InputAction.CallbackContext context){ Jump(); } //! Go Jump, no bool is need
   void OnSprint_KeyDown(InputAction.CallbackContext context){ iv.bSprint = true; }
   void OnSprint_KeyUp(InputAction.CallbackContext context){ iv.bSprint = false; }


   #endregion

   void Start() {
      playerInput = GetComponent<PlayerInput>();

      // Local player check.
      if (isLocalPlayer && !playerInput) { Debug.LogError("PlayerInput component not found on local player! Following scripts not going to run..."); return; }

      // Non-local player, destroy the PlayerInput component and disable the main camera
      if (!isLocalPlayer){
         if (playerInput) { Destroy(playerInput); }
         if (mainCamera) { Destroy(mainCamera); }
         if (followCamScript) { Destroy(followCamScript); }
      }

      void SyncIdInit(){
         headFlag_SR.sprite = playerIcons_Sprite[playerId];
         this.gameObject.name = $"P{playerId+1}_Controller";
         smr_Body.material = playerCharacters_Material[playerId];
         mr_LeftWeapon.material = playerCharacters_Material[playerId];
         mr_RightWeapon.material = playerCharacters_Material[playerId];
      }

      Timer.CreateTimer_NoPhysics(this.gameObject, .1f, () => {
         SyncIdInit();
         playerInput.enabled = true;
      });



      // iv local for inspector to view.
      iv.bLocal = true;
      
      cameras.cinemachineTargetYaw = cameras.cinemachineCameraTarget.transform.rotation.eulerAngles.y;

      AssignAnimatorHashIds();

      LateEnable();

   }



   private void AssignAnimatorHashIds() {
      _a.speed_Id = Animator.StringToHash("Speed");
      _a.loco_Id = Animator.StringToHash("Loco");
      _a.jumping_Id = Animator.StringToHash("Jumping");
   }

   void Update(){
      if(!isLocalPlayer) return;



      AnimatorParamToLocal();

      // FallByGravity(); 
      Move(iv.move);


      LocalParamToAnimator();

      
      if (Input.GetMouseButtonDown(0)) { EnableTrajectory_StartDragging(); }

      if (this.status.bIsDragging) { TrajectoryLogics(); } //! including release logics at the end.


   }

   void AnimatorParamToLocal() { // No need translate speed to local
      _a.bLoco = animator.GetBool(_a.loco_Id);
      _a.bJumping = animator.GetBool(_a.jumping_Id);
   }

   void LocalParamToAnimator() {
      var bLoco = animationState == AnimationState.Locomotion;
      var bJumping = animationState == AnimationState.Jumping;
      animator.SetBool(_a.loco_Id, bLoco);
      animator.SetBool(_a.jumping_Id, bJumping);

   }

   public void Jump() {
      var bLoco = animationState == AnimationState.Locomotion;
      if(bLoco) { //playing loco and is grounded
         animationState = AnimationState.Jumping;

         if(!B_AlreadyPlayingDesiredAnimClip(animator.GetCurrentAnimatorStateInfo(0), "Jump_1")) animator.CrossFade("Jump_1", .25f, 0, 0f);

         

         Timer.CreateTimer_NoPhysics(this.gameObject, .9f, () => { animationState = AnimationState.Locomotion; });
      }
   }

   public void Move(Vector2 input) { //! Complex
      Physics.Raycast(_j.characterFootTransform.position, Vector3.down, out RaycastHit hit, .01f, _j.GROUND_LAYER, QueryTriggerInteraction.Ignore);

      float speedHorizontal = input == Vector2.zero ? 0f : iv.bSprint ? input.magnitude * movements.sprintSpeed : input.magnitude * movements.moveSpeed;
      float speedVertical = hit.collider ? 0f : 5f;
      Vector3 velocityCombined;
      Vector3 inputDirection = new Vector3(input.x, 0.0f, input.y).normalized;

      float targetRotation = 0f;
      float smoothedRotation;
      Vector3 targetDirection;

      void calculateTargetRotation() {
         if (speedHorizontal != 0f) {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            smoothedRotation = Mathf.SmoothDampAngle(character.transform.eulerAngles.y, targetRotation, ref movements.rotationVelocity, movements.rotationSmoothTime);
            character.transform.rotation = Quaternion.Euler(0.0f, smoothedRotation, 0.0f);
         }
      }

      void applyMovementAndRotation() {
         targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
         velocityCombined = targetDirection.normalized * speedHorizontal + Vector3.down * speedVertical;
         characterController.Move(velocityCombined * Time.deltaTime);
      }

      calculateTargetRotation();
      applyMovementAndRotation();

      // Anim
      var bJumping = animationState == AnimationState.Jumping;
      if(bJumping) return;

      animationState = AnimationState.Locomotion;

      //!!!!!!!!!!!!!!!!!!!!!!!
      var animatorInfo = animator.GetCurrentAnimatorStateInfo(0);

      //varies anim by speed sth like that.
      if(speedHorizontal > 0.1f && !B_AlreadyPlayingDesiredAnimClip(animatorInfo, "Move_1")) {
         animator.Play("Move_1");
      }
      else if(!B_AlreadyPlayingDesiredAnimClip(animatorInfo, "Idle_1")){
         animator.Play("Idle_1");
      }

   } 





   void Look(Vector2 v) {
      if (v.sqrMagnitude >= THRESHOLD && !cameras.lockCameraPosition) {
         float deltaTimeMultiplier = 1.0f;

         cameras.cinemachineTargetYaw += v.x * deltaTimeMultiplier;
         cameras.cinemachineTargetPitch += v.y * deltaTimeMultiplier;
      }

      cameras.cinemachineTargetYaw = ClampAngle(cameras.cinemachineTargetYaw, float.MinValue, float.MaxValue);
      cameras.cinemachineTargetPitch = ClampAngle(cameras.cinemachineTargetPitch, cameras.bottomClamp, cameras.topClamp);

      cameras.cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cameras.cinemachineTargetPitch + cameras.cameraAngleOverride,
         cameras.cinemachineTargetYaw, 0.0f);
   }

   void LateUpdateHeadFlag_Local(){
      var directionToCamera = mainCamera.transform.position - headFlag_SR.transform.position;
      directionToCamera.y = 0;
      if(directionToCamera.sqrMagnitude>.01f){
         var targetRotation = Quaternion.LookRotation(directionToCamera);
         targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y + 180, 0);
         headFlag_SR.transform.rotation = targetRotation;
      }
   }

   void LateUpdateHeadFlag_NonLocal(){
      var directionToCamera = Camera.main.transform.position - headFlag_SR.transform.position;
      directionToCamera.y = 0;
      if(directionToCamera.sqrMagnitude>.01f){
         var targetRotation = Quaternion.LookRotation(directionToCamera);
         targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y + 180, 0);
         headFlag_SR.transform.rotation = targetRotation;
      }
   }

   void LateUpdate(){
      if (!isLocalPlayer) { LateUpdateHeadFlag_NonLocal(); return; }
      Look(iv.look);
      LateUpdateHeadFlag_Local();
   }

   // ******* EXTRAS BELOW ********

   private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
      if (lfAngle < -360f) lfAngle += 360f;
      if (lfAngle > 360f) lfAngle -= 360f;
      return Mathf.Clamp(lfAngle, lfMin, lfMax);
   }

   private void OnFootstep(AnimationEvent animationEvent) {
      if (animationEvent.animatorClipInfo.weight > 0.5f) {
         if (audios.footstepAudioClips.Length > 0) {
            var index = UnityEngine.Random.Range(0, audios.footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(audios.footstepAudioClips[index], character.transform.TransformPoint(characterController.center), audios.footstepAudioVolume);
         }
      }
   }

   private void OnLand(AnimationEvent animationEvent) {
      if (animationEvent.animatorClipInfo.weight > 0.5f) {
         AudioSource.PlayClipAtPoint(audios.landingAudioClip, character.transform.TransformPoint(characterController.center), audios.footstepAudioVolume);
      }
   }

   public bool B_AlreadyPlayingDesiredAnimClip(AnimatorStateInfo animatorStateInfo, string stateName){
      return animatorStateInfo.IsName(stateName) && animatorStateInfo.normalizedTime >= 1.0f;
   }





   #region //! PART 2
void EnableTrajectory_StartDragging(){
      if (this.status.bProjectileRecastLocked) return; // !!!!!!!!!!!!!!
      this.status.bIsDragging = true;
      this.status.dragStartPosition = Input.mousePosition; Debug.Log(Input.mousePosition);
      this.extras.trajectoryLine.lineRenderer.enabled = true;
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
         this.extras.trajectoryLine.lineRenderer.SetPositions(points);
      };

      var velocityCombined = GetVelocityCombined_By_Calculating_DragDistance();

      updateDrawTrajectoryLine_LineRenderer( GetFiringStartPosition(), velocityCombined );

      //P: If release mouse left btn, fire.
      if (Input.GetMouseButtonUp(0)){
         this.status.bIsDragging = false;
         this.extras.trajectoryLine.lineRenderer.enabled = false; // hide trajectory line
         FireProjectile(velocityCombined);
      }
   }

   void FireProjectile(Vector3 velocity){

      // Fire first food when having food on hand.
      // if (character.slotTransform.childCount > 0 ){
         // this.status.bProjectileRecastLocked = true;
         // var firstFood = pChar.slotTransform.GetChild(0).gameObject; firstFood.transform.SetParent(null);
         // var rb = firstFood.GetComponent<Rigidbody>(); rb.isKinematic = false; rb.useGravity = true; rb.mass = 1f; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
         // rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!
         // Timer.CreateTimer_Physics(this.gameObject, 3f, () => this.status.bProjectileRecastLocked = false );
      // }

      // Fire a hook for getting something back.
      // else{
         var hook = this.extras.hook.gameObject; hook.transform.SetParent(null); this.status.bProjectileRecastLocked = true;
         var rb = hook.GetComponent<Rigidbody>(); rb.isKinematic = false; rb.useGravity = true;
         rb.AddForce(velocity, ForceMode.VelocityChange); //!!!!! ADD FORCE !!!!!
         Timer.CreateTimer_Physics(this.gameObject, 3f, () => {
            this.status.bProjectileRecastLocked = false;
            this.extras.hook.ReattachHookContainer_ResetTransform(extras.hookContainerTransform);
            this.extras.hook.ResetRigidbody();
         });
      // }
   }




   // ! Complex math getters

   Vector3 GetFiringStartPosition(){
      return extras.hookContainerTransform.position;
   }

   Vector3 GetVelocityCombined_By_Calculating_DragDistance(){
      //* Remarks: Here only calculating input delta, no lineRenderer involved.
      //P: Get world fly direction first.
      var delta = Input.mousePosition - this.status.dragStartPosition;
      var dragDistance = Math.Clamp(delta.magnitude / Screen.height, 0f, 2);
      var desiredDirection = -delta.normalized;

      var playerForward = character.transform.forward; //!!!
      var playerRight = character.transform.right; //!!!
      var inputForward = new Vector3(playerForward.x, 0, playerForward.z).normalized;
      var inputRight   = new Vector3(playerRight.x, 0, playerRight.z).normalized;

      // direction + input magnitude, not the real physics term of force.
      var rawFlyingForce_xComp = (desiredDirection.y * inputForward + desiredDirection.x * inputRight).normalized;

      //P: Limit fly direction within -45 to +45 degree.
      var angle = Vector3.SignedAngle(rawFlyingForce_xComp, inputForward, Vector3.up);

      //P: Calculate initial xComp velocity
      var velocity_xComponent = rawFlyingForce_xComp * dragDistance * this.trajs.launchPower;

      //P: Add yComp(height) and become vector combined
      var radian = this.trajs.upwardAngle * Mathf.Deg2Rad;
      var velocity_Combined = new Vector3(velocity_xComponent.x, velocity_xComponent.magnitude * Mathf.Sin(radian), velocity_xComponent.z * Mathf.Cos(radian));
      return velocity_Combined;
   }
   #endregion
}
