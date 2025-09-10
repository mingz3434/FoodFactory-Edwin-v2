using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Mirror;
using _ = GameInstance;

//!!! Bind grounds.groundLayers, cameras.cinemachineCameraTarget, audios.landingAudioClip, audios.footstepAudioClips in Inspector
//!!! groundLayers to "Default" and cct to "CameraRoot" plssssssss!!!

[RequireComponent(typeof(PlayerInput))]
public class ThirdPerson_PC : NetworkBehaviour {

   [Serializable] public class MovementSettings { public float moveSpeed = 4.0f, sprintSpeed = 5.355f, rotationSmoothTime = .12f, speedChangeRate = 10f; [ReadOnly] public float speed, targetRotation, rotationVelocity, targetSpeed; }
   [Serializable] public class JumpSettings { public float jumpHeight = 1.2f, gravity = -15f, jumpTimeout = .5f, fallTimeout = .15f; [ReadOnly] public float verticalVelocity, terminalVelocity, jumpTimeoutDelta, fallTimeoutDelta; }
   [Serializable] public class GroundSettings { public bool bIsGrounded = true; public float groundedOffset = -.14f, groundedRadius = .28f; public LayerMask groundLayers; }
   [Serializable] public class CameraSettings { public GameObject cinemachineCameraTarget; public float topClamp = 70f, bottomClamp = -30f, cameraAngleOverride = 0f; public bool lockCameraPosition = false; [ReadOnly] public float cinemachineTargetYaw, cinemachineTargetPitch; }
   [Serializable] public class AudioSettings { public AudioClip landingAudioClip; public AudioClip[] footstepAudioClips; public float footstepAudioVolume = .5f; }
   [Serializable] public class AnimSettings { [ReadOnly] public float blendedValue; [HideInInspector] public int speed_Id, loco_Id, jumping_Id, punching_Id; public bool bLoco, bJumping; }
   [Serializable] public class InputValues { public bool bLocal; public Vector2 move, look; public bool bSprint; }

   public enum AnimationState { Locomotion, Jumping, Punching }
   public AnimationState animationState = AnimationState.Locomotion;

   public MovementSettings movements;
   public JumpSettings jumps;
   public GroundSettings grounds;
   public CameraSettings cameras;
   public AudioSettings audios;
   public AnimSettings _a;

   PlayerInput playerInput;
   public InputValues iv = new InputValues();
   public Material blackMat;

   public Animator animator;
   public CharacterController characterController;
   public GameObject mainCamera;
   public GameObject followCamScript;
   public GameObject character;
   public GameObject characterMesh_GO;


   private const float THRESHOLD = 0.01f;

   public System.Random random = new System.Random();



   #region Input region
   void Awake(){ }


   void OnEnable() {
      playerInput.actions["Jump"].started += OnJump_KeyDown;
      playerInput.actions["Sprint"].started += OnSprint_KeyDown;
      playerInput.actions["Sprint"].canceled += OnSprint_KeyUp;
   }

   void OnDisable() {
      playerInput.actions["Jump"].started -= OnJump_KeyDown;
      playerInput.actions["Sprint"].started -= OnSprint_KeyDown;
      playerInput.actions["Sprint"].canceled -= OnSprint_KeyUp;
   }

   public void OnMove(InputAction.CallbackContext context){ iv.move = context.ReadValue<Vector2>(); }
   public void OnLook(InputAction.CallbackContext context){ iv.look = context.ReadValue<Vector2>(); }
   void OnJump_KeyDown(InputAction.CallbackContext context){ Jump(); } //! Go Jump, no bool is need
   void OnSprint_KeyDown(InputAction.CallbackContext context){ iv.bSprint = true; }
   void OnSprint_KeyUp(InputAction.CallbackContext context){ iv.bSprint = false; }


   #endregion

   private void Start() {
      playerInput = GetComponent<PlayerInput>();

      // Non-local player, destroy the PlayerInput component and disable the main camera
      if (!isLocalPlayer){
        if (playerInput) { Timer.CreateTimer_NoPhysics(this.gameObject, .1f , ()=>Destroy(playerInput) ); }
        mainCamera.SetActive(false);
        return;
      }

      // Local player, check the existence of the PlayerInput then enable it.
      if (!playerInput) { Debug.LogError("PlayerInput component not found on local player!"); return; }
      playerInput.enabled = true;

      // iv local for inspector to view, set black for visually showing the local player.
      iv.bLocal = true;
      characterMesh_GO.GetComponent<SkinnedMeshRenderer>().material = blackMat;
      
      grounds.groundLayers = LayerMask.GetMask("Default");
      cameras.cinemachineTargetYaw = cameras.cinemachineCameraTarget.transform.rotation.eulerAngles.y;

      AssignAnimatorHashIds();

      jumps.jumpTimeoutDelta = jumps.jumpTimeout;
      jumps.fallTimeoutDelta = jumps.fallTimeout;
   }



   private void AssignAnimatorHashIds() {
      _a.speed_Id = Animator.StringToHash("Speed");
      _a.loco_Id = Animator.StringToHash("Loco");
      _a.jumping_Id = Animator.StringToHash("Jumping");
   }

   void Update(){


      if(!isLocalPlayer) return;
      AnimatorParamToLocal();

      Move(iv.move);

      LocalParamToAnimator();
   }

   void AnimatorParamToLocal() { // No need translate speed to local
      _a.bLoco = animator.GetBool(_a.loco_Id);
      _a.bJumping = animator.GetBool(_a.jumping_Id);
   }

   void LocalParamToAnimator() {
      var speed = movements.speed;
      var bLoco = animationState == AnimationState.Locomotion;
      var bJumping = animationState == AnimationState.Jumping;
      animator.SetFloat(_a.speed_Id, speed);
      animator.SetBool(_a.loco_Id, bLoco);
      animator.SetBool(_a.jumping_Id, bJumping);

   }

   public void Jump() {
      var bLoco = animationState == AnimationState.Locomotion;
      if(bLoco) {
         animationState = AnimationState.Jumping;
         if(!B_AnimClipFinished(animator.GetCurrentAnimatorStateInfo(0), "Jump_1")) animator.CrossFade("Jump_1", .25f, 0, 0f);
         Timer.CreateTimer_NoPhysics(this.gameObject, .9f, () => { animationState = AnimationState.Locomotion; });
      }
   }

   public void Move(Vector2 v) { //! Complex

      Debug.Log("Move");
      transform.Translate(v.x*.05f, 0, v.y*.05f);
      return;
      void calculateTargetSpeed() {
         float targetSpeed = iv.bSprint ? movements.sprintSpeed : movements.moveSpeed;
         if (v == Vector2.zero) targetSpeed = 0.0f;
         movements.targetSpeed = targetSpeed;
      }

      void smoothSpeedChange() {
         float currentHorizontalSpeed = new Vector3(characterController.velocity.x, 0.0f, characterController.velocity.z).magnitude;

         float speedOffset = 0.1f;

         if (currentHorizontalSpeed < movements.targetSpeed - speedOffset ||
            currentHorizontalSpeed > movements.targetSpeed + speedOffset) {
            movements.speed = Mathf.Lerp(currentHorizontalSpeed, movements.targetSpeed,
               Time.deltaTime * movements.speedChangeRate);

            movements.speed = Mathf.Round(movements.speed * 1000f) / 1000f;
         } else {
            movements.speed = movements.targetSpeed;
         }

         _a.blendedValue = Mathf.Lerp(_a.blendedValue, movements.targetSpeed, Time.deltaTime * movements.speedChangeRate);
         if (_a.blendedValue < 0.01f) _a.blendedValue = 0f;
      }

      void calculateTargetRotation() {
         Vector3 inputDirection = new Vector3(v.x, 0.0f, v.y).normalized;

         if (v != Vector2.zero) {
            movements.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
               mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(character.transform.eulerAngles.y, movements.targetRotation, ref movements.rotationVelocity,
               movements.rotationSmoothTime);

            character.transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
         }
      }

      void applyMovementAndRotation() {
         Vector3 targetDirection = Quaternion.Euler(0.0f, movements.targetRotation, 0.0f) * Vector3.forward;

         characterController.Move(targetDirection.normalized * (movements.speed * Time.deltaTime) +
            new Vector3(0.0f, jumps.verticalVelocity, 0.0f) * Time.deltaTime);
      }



      calculateTargetSpeed();
      smoothSpeedChange();
      calculateTargetRotation();
      applyMovementAndRotation();


      // Anim
      var bJumping = animationState == AnimationState.Jumping;
      if(bJumping) return;

      animationState = AnimationState.Locomotion;

      //!!!!!!!!!!!!!!!!!!!!!!!
      var animatorInfo = animator.GetCurrentAnimatorStateInfo(0);

      //varies anim by speed sth like that.
      if(movements.speed > 0.1f) {
         if (!B_AnimClipFinished(animatorInfo, "Move_1")) animator.Play("Move_1");
      }
      else{
         if (!B_AnimClipFinished(animatorInfo, "Idle_1")) animator.Play("Idle_1");
      }

   } 





   void Look(Vector2 inputVector) {
      if (inputVector.sqrMagnitude >= THRESHOLD && !cameras.lockCameraPosition) {
         float deltaTimeMultiplier = 1.0f;

         cameras.cinemachineTargetYaw += inputVector.x * deltaTimeMultiplier;
         cameras.cinemachineTargetPitch += inputVector.y * deltaTimeMultiplier;
      }

      cameras.cinemachineTargetYaw = ClampAngle(cameras.cinemachineTargetYaw, float.MinValue, float.MaxValue);
      cameras.cinemachineTargetPitch = ClampAngle(cameras.cinemachineTargetPitch, cameras.bottomClamp, cameras.topClamp);

      cameras.cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cameras.cinemachineTargetPitch + cameras.cameraAngleOverride,
         cameras.cinemachineTargetYaw, 0.0f);
   }

   void LateUpdate(){
      if(!isLocalPlayer) return;
      Look(iv.look);
   }

   // ******* EXTRAS BELOW ********

   private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
      if (lfAngle < -360f) lfAngle += 360f;
      if (lfAngle > 360f) lfAngle -= 360f;
      return Mathf.Clamp(lfAngle, lfMin, lfMax);
   }

   private void OnDrawGizmosSelected() {
      Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
      Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
      Gizmos.color = grounds.bIsGrounded ? transparentGreen : transparentRed;
      Gizmos.DrawSphere(new Vector3(character.transform.position.x, character.transform.position.y - grounds.groundedOffset, character.transform.position.z), grounds.groundedRadius);
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

   public bool B_AnimClipFinished(AnimatorStateInfo animatorStateInfo, string stateName){
      return animatorStateInfo.IsName(stateName) && animatorStateInfo.normalizedTime >= 1.0f;
   }
}












   // void Preprocessing(){
   //    void disableNonLocalPlayerComponents(){
   //       if(!isLocalPlayer){
   //          this.gameObject.name = "RemotePlayer";
   //          character.name = "RemoteCharacter";
   //          Destroy(playerInput);
   //          Destroy(input);
   //          Destroy(mainCamera);
   //          Destroy(followCamScript);
   //       }
   //    }

   //    void unparentCharacter(){
   //       character.transform.SetParent(this.transform.parent);
   //    }
   //    void unparentSelf(){
   //       this.transform.SetParent(null);
   //    }

   //    void activateUnsurePlayerInput(){
   //       if(isLocalPlayer) playerInput.ActivateInput();
   //    }

   //    disableNonLocalPlayerComponents();
   //    unparentCharacter();
   //    unparentSelf();
   //    activateUnsurePlayerInput();
   // }