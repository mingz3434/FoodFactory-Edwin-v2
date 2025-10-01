using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Mirror;
using _ = GameInstance;
using System.Linq;

//!!! Bind grounds.groundLayers, cameras.cinemachineCameraTarget, audios.landingAudioClip, audios.footstepAudioClips in Inspector
//!!! groundLayers to "Default" and cct to "CameraRoot" plssssssss!!!

public partial class ThirdPerson_PC : NetworkBehaviour {

   [SyncVar] public int playerId = -1; //! Hook for connected players

   public bool bAssetsReady, bInited;
   public Sprite[] playerIcons_Sprite; public SpriteRenderer headFlag_SR;
   public Material[] playerCharacters_Material; public SkinnedMeshRenderer smr_Body; public MeshRenderer mr_LeftWeapon, mr_RightWeapon;
   [Serializable] public class MovementSettings { public float moveSpeed = 4.0f, sprintSpeed = 5.355f, rotationSmoothTime = .12f; [ReadOnly] public float rotationVelocity; }
   [Serializable] public class CC_Cust_JumpSettings { public float JUMP_HEIGHT = 1.2f, GRAVITY = -15f, JUMP_TIMEOUT = .5f, FALL_TIMEOUT = .15f, TERMINAL_VELOCITY = -50f; public Transform characterFootTransform; public LayerMask GROUND_LAYER; [ReadOnly] public float verticalVelocity; [ReadOnly] public bool bIsGrounded, bIsJumping; }




   [Serializable] public class CameraSettings { public GameObject cinemachineCameraTarget; public float topClamp = 70f, bottomClamp = -30f, cameraAngleOverride = 0f; public bool lockCameraPosition = false; [ReadOnly] public float cinemachineTargetYaw, cinemachineTargetPitch; }
   [Serializable] public class AudioSettings { public AudioClip landingAudioClip; public AudioClip[] footstepAudioClips; public float footstepAudioVolume = .5f; }
   [Serializable] public class AnimSettings { [ReadOnly] public float blendedValue; [HideInInspector] public int speed_Id, loco_Id, jumping_Id, punching_Id; public bool bLoco, bJumping; }
   [Serializable] public class InputValues { public bool bLocal; public Vector2 move, look, dragXY; public bool bSprint; public bool bHookDragging, bFoodTrayDragging; }

   public enum AnimationState { Locomotion, Jumping, Punching }
   public AnimationState animationState = AnimationState.Locomotion;

   public MovementSettings movements;
   public CC_Cust_JumpSettings _j;
   public CameraSettings cameras;
   public AudioSettings audios;
   public AnimSettings _a;

   PlayerInput playerInput;
   public InputValues iv;
   public Animator animator;
   public CharacterController characterController;
   public GameObject mainCamera;
   public GameObject followCamScript;
   public GameObject character;


   private const float THRESHOLD = 0.01f;

   public System.Random random = new System.Random();

   void LateEnable() { OnLateEnable(); }
   void OnLateEnable() {
      if(playerInput){
         playerInput.actions["Jump"].started += OnJump_KeyDown;
         playerInput.actions["Sprint"].started += OnSprint_KeyDown;
         playerInput.actions["Sprint"].canceled += OnSprint_KeyUp;
         playerInput.actions["HookDraggingToggle"].started += OnHookDraggingToggle_KeyDown_G;
         playerInput.actions["HookDraggingToggle"].canceled += OnHookDraggingToggle_KeyUp_G;
         playerInput.actions["FoodTrayDraggingToggle"].started += OnPickFood_or_OnFoodTrayDraggingToggle_KeyDown_F;
         playerInput.actions["FoodTrayDraggingToggle"].canceled += OnFoodTrayDraggingToggle_KeyUp_F;
      }
   }

   void OnDisable() {
      if(playerInput){         
         playerInput.actions["Jump"].started -= OnJump_KeyDown;
         playerInput.actions["Sprint"].started -= OnSprint_KeyDown;
         playerInput.actions["Sprint"].canceled -= OnSprint_KeyUp;
         playerInput.actions["HookDraggingToggle"].started -= OnHookDraggingToggle_KeyDown_G;
         playerInput.actions["HookDraggingToggle"].canceled -= OnHookDraggingToggle_KeyUp_G;
         playerInput.actions["FoodTrayDraggingToggle"].started -= OnPickFood_or_OnFoodTrayDraggingToggle_KeyDown_F;
         playerInput.actions["FoodTrayDraggingToggle"].canceled -= OnFoodTrayDraggingToggle_KeyUp_F;
      }
   }

   public void OnMove(InputAction.CallbackContext context){ iv.move = context.ReadValue<Vector2>(); }
   public void OnLook(InputAction.CallbackContext context){ iv.look = context.ReadValue<Vector2>(); }
   public void OnDragXY(InputAction.CallbackContext context){ iv.dragXY = Input.mousePosition; }
   void OnJump_KeyDown(InputAction.CallbackContext context){ Jump(); } //! Go Jump, no bool is need
   void OnSprint_KeyDown(InputAction.CallbackContext context){ iv.bSprint = true; }
   void OnSprint_KeyUp(InputAction.CallbackContext context){ iv.bSprint = false; }
  
   void SyncIdInit(){
      headFlag_SR.sprite = playerIcons_Sprite[playerId];
      this.gameObject.name = $"P{playerId + 1}_Controller";
      smr_Body.material = playerCharacters_Material[playerId];
      mr_LeftWeapon.material = playerCharacters_Material[playerId];
      mr_RightWeapon.material = playerCharacters_Material[playerId];
      Log_1008_SyncIdInit();
   }

   [Command (requiresAuthority = false)]
   void Cmd_SyncIdInit() {
      Rpc_SyncIdInit((NetworkManager.singleton as CustomNetworkManager).GetSelfPlayerId(this.connectionToClient));
   }

   [ClientRpc]
   void Rpc_SyncIdInit(int playerId) {
      this.playerId = playerId;
      SyncIdInit();
   }

   void Start() {
      Log_1001_Start();

      playerInput = GetComponent<PlayerInput>();

      // Local player check.
      if (isLocalPlayer && !playerInput) { Debug.LogError("PlayerInput component not found on local player! Following scripts not going to run..."); return; }

      // Non-local player, destroy the PlayerInput component and disable the main camera
      if (!isLocalPlayer){
         if (playerInput) { Destroy(playerInput); }
         if (mainCamera) { Destroy(mainCamera); }
         if (followCamScript) { Destroy(followCamScript); }
         SyncIdInit();
      }

      if (isLocalPlayer) {
         _.localPlayer = this;
         _.localCharacter = this.character;
         Cmd_SyncIdInit();
      }

      // iv local for inspector to view.
      iv.bLocal = true;
      
      cameras.cinemachineTargetYaw = cameras.cinemachineCameraTarget.transform.rotation.eulerAngles.y;

      AssignAnimatorHashIds();

      LateEnable();

   }

   public override void OnStartLocalPlayer(){
      base.OnStartLocalPlayer();
      playerInput = GetComponent<PlayerInput>();
      if (playerInput) { playerInput.enabled = true; }
   }


   private void AssignAnimatorHashIds() {
      _a.speed_Id = Animator.StringToHash("Speed");
      _a.loco_Id = Animator.StringToHash("Loco");
      _a.jumping_Id = Animator.StringToHash("Jumping");
   }

   void Update(){

      


      AnimatorParamToLocal();

      // FallByGravity(); 
      Move(iv.move);


      LocalParamToAnimator();


      Update_P2();

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

         if(!B_AlreadyPlayingDesiredAnimClip_and_ClipFinished(animator.GetCurrentAnimatorStateInfo(0), "Jump_1")) animator.CrossFade("Jump_1", .25f, 0, 0f);

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
      if(speedHorizontal > 0.1f && !B_AlreadyPlayingDesiredAnimClip_and_ClipFinished(animatorInfo, "Move_1")) {
         animator.Play("Move_1");
      }
      else if(!B_AlreadyPlayingDesiredAnimClip_and_ClipFinished(animatorInfo, "Idle_1")){
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

   public bool B_AlreadyPlayingDesiredAnimClip_and_ClipFinished(AnimatorStateInfo animatorStateInfo, string stateName){
      return animatorStateInfo.IsName(stateName) && animatorStateInfo.normalizedTime >= 1.0f;
   }


}



public partial class ThirdPerson_PC : NetworkBehaviour {
   public void Log_1000_OnStartLocalPlayer(){ Debug.Log("1000: OnStartLocalPlayer");  }
   public void Log_1001_Start(){ Debug.Log("1001: Start");  }
   public void Log_1002_LateEnable(){ Debug.Log("1002: LateEnable");  }
   public void Log_1003_Disable(){ Debug.Log("1003: Disable");  }
   public void Log_1004U_Move(){ Debug.Log("1004U: Move");  }
   public void Log_1005U_Look(){ Debug.Log("1005U: Look");  }
   public void Log_1006U_DragXY(){ Debug.Log("1006U: DragXY"); }
   public void Log_1007U_Sprint(){ Debug.Log("1007U: Sprint"); }
   public void Log_1008_SyncIdInit(){ Debug.Log("1008: SyncIdInit"); }

}