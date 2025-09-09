using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Mirror;
using UnityEditor.Animations;
using UnityEngine.SocialPlatforms;

public class ThirdPerson_PC : NetworkBehaviour {

   [Serializable] public class MovementSettings { public float moveSpeed = 4.0f, sprintSpeed = 5.355f, rotationSmoothTime = .12f, speedChangeRate = 10f; [ReadOnly] public float speed, targetRotation, rotationVelocity, targetSpeed; }
   [Serializable] public class JumpSettings { public float jumpHeight = 1.2f, gravity = -15f, jumpTimeout = .5f, fallTimeout = .15f; [ReadOnly] public float verticalVelocity, terminalVelocity, jumpTimeoutDelta, fallTimeoutDelta; }
   [Serializable] public class GroundSettings { public bool bIsGrounded = true; public float groundedOffset = -.14f, groundedRadius = .28f; public LayerMask groundLayers; }
   [Serializable] public class CameraSettings { public GameObject cinemachineCameraTarget; public float topClamp = 70f, bottomClamp = -30f, cameraAngleOverride = 0f; public bool lockCameraPosition = false; [ReadOnly] public float cinemachineTargetYaw, cinemachineTargetPitch; }
   [Serializable] public class AudioSettings { public AudioClip landingAudioClip; public AudioClip[] footstepAudioClips; public float footstepAudioVolume = .5f; }
   [Serializable] public class AnimSettings { [ReadOnly] public float blendedValue; [HideInInspector] public int speed_Id, loco_Id, jumping_Id, punching_Id; }

   public enum AnimationState { Locomotion, Jumping, Punching }
   public AnimationState animationState = AnimationState.Locomotion;

   public MovementSettings movements;
   public JumpSettings jumps;
   public GroundSettings grounds;
   public CameraSettings cameras;
   public AudioSettings audios;
   public AnimSettings _a;

   public PlayerInput playerInput;
   public Animator animator;
   public CharacterController characterController;
   public InputActionsHandler input;
   public GameObject mainCamera;

   private const float THRESHOLD = 0.01f;

   //!!! Bind grounds.groundLayers, cameras.cinemachineCameraTarget, audios.landingAudioClip, audios.footstepAudioClips in Inspector
   //!!! groundLayers to "Default" and cct to "CameraRoot" plssssssss!!!


   private void Start() {
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
      // _a.punching_Id = Animator.StringToHash("Punching");
   }

   void Update(){
      AnimatorParamToLocal();

      OnMove_(input.move);
      if (bSprintPressing) { OnSprintPressing_(); } else { OnSprintReleasing_(); }

      LocalParamToAnimator();
   }

   void AnimatorParamToLocal() {
      // var speed = Mathf.Abs(_a.speed_Id); (no need)
      var bLoco = animator.GetBool(_a.loco_Id);
      var bJumping = animator.GetBool(_a.jumping_Id);

   }

   void LocalParamToAnimator() {
      var speed = movements.speed;
      var bLoco = animationState == AnimationState.Locomotion;
      var bJumping = animationState == AnimationState.Jumping;
      // var bPunching = animationState == AnimationState.Punching;
      animator.SetFloat(_a.speed_Id, speed);
      animator.SetBool(_a.loco_Id, bLoco);
      animator.SetBool(_a.jumping_Id, bJumping);
      // animator.SetBool(_a.punching_Id, bPunching);

   }

   bool bSprintPressing = false;
   public void OnSprintPressing_(){ bSprintPressing = true; }
   public void OnSprintReleasing_(){ bSprintPressing = false; }
   public void OnJump_() {
      var bLoco = animationState == AnimationState.Locomotion;
      if(bLoco) {
         animationState = AnimationState.Jumping;
         if(!B_AnimClipFinished(animator.GetCurrentAnimatorStateInfo(0), "Jump_1")) animator.CrossFade("Jump_1", .25f, 0, 0f);
         Timer.CreateTimer_NoPhysics(this.gameObject, .9f, () => { animationState = AnimationState.Locomotion; });
      }

   }

   public void OnMove_(Vector2 inputVector) {

      void calculateTargetSpeed() {
         float targetSpeed = bSprintPressing ? movements.sprintSpeed : movements.moveSpeed;
         if (inputVector == Vector2.zero) targetSpeed = 0.0f;
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
         Vector3 inputDirection = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

         if (inputVector != Vector2.zero) {
            movements.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
               mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, movements.targetRotation, ref movements.rotationVelocity,
               movements.rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
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

      //varis anim by speed sth like that.
      if(movements.speed > 0.1f) {
         if (!B_AnimClipFinished(animatorInfo, "Move_1")) animator.Play("Move_1");
      }
      else{
         if (!B_AnimClipFinished(animatorInfo, "Idle_1")) animator.Play("Idle_1");
      }

   }





   void OnLook_(Vector2 inputVector) {
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
      OnLook_(input.look);
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
      Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - grounds.groundedOffset, transform.position.z), grounds.groundedRadius);
   }

   private void OnFootstep(AnimationEvent animationEvent) {
      if (animationEvent.animatorClipInfo.weight > 0.5f) {
         if (audios.footstepAudioClips.Length > 0) {
            var index = UnityEngine.Random.Range(0, audios.footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(audios.footstepAudioClips[index], transform.TransformPoint(characterController.center), audios.footstepAudioVolume);
         }
      }
   }

   private void OnLand(AnimationEvent animationEvent) {
      if (animationEvent.animatorClipInfo.weight > 0.5f) {
         AudioSource.PlayClipAtPoint(audios.landingAudioClip, transform.TransformPoint(characterController.center), audios.footstepAudioVolume);
      }
   }

   public bool B_AnimClipFinished(AnimatorStateInfo animatorStateInfo, string stateName){
      return animatorStateInfo.IsName(stateName) && animatorStateInfo.normalizedTime >= 1.0f;
   }
}