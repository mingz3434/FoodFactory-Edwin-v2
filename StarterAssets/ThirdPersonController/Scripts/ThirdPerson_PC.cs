using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Mirror;
using UnityEditor.Animations;

public class ThirdPerson_PC : NetworkBehaviour {

   [Serializable] public class MovementSettings { public float moveSpeed = 4.0f, sprintSpeed = 5.355f, rotationSmoothTime = .12f, speedChangeRate = 10f; [ReadOnly] public float speed, targetRotation, rotationVelocity, targetSpeed; }
   [Serializable] public class JumpSettings { public float jumpHeight = 1.2f, gravity = -15f, jumpTimeout = .5f, fallTimeout = .15f; [ReadOnly] public float verticalVelocity, terminalVelocity, jumpTimeoutDelta, fallTimeoutDelta; }
   [Serializable] public class GroundSettings { public bool bIsGrounded = true; public float groundedOffset = -.14f, groundedRadius = .28f; public LayerMask groundLayers; }
   [Serializable] public class CameraSettings { public GameObject cinemachineCameraTarget; public float topClamp = 70f, bottomClamp = -30f, cameraAngleOverride = 0f; public bool lockCameraPosition = false; [ReadOnly] public float cinemachineTargetYaw, cinemachineTargetPitch; }
   [Serializable] public class AudioSettings { public AudioClip landingAudioClip; public AudioClip[] footstepAudioClips; public float footstepAudioVolume = .5f; }
   [Serializable] public class AnimSettings { [ReadOnly] public float blendedValue; [HideInInspector] public int playingNonLocomotionAnim_Id, isMoving_Id, isJumping_Id; }

   public enum AnimationState { Locomotion, Jumping, Punching }
   public AnimationState animationState = AnimationState.Locomotion;

   public MovementSettings movements;
   public JumpSettings jumps;
   public GroundSettings grounds;
   public CameraSettings cameras;
   public AudioSettings audios;
   public AnimSettings animators;

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
      animators.playingNonLocomotionAnim_Id = Animator.StringToHash("playingNonLocomotionAnim");
      animators.isMoving_Id = Animator.StringToHash("isMoving");
      animators.isJumping_Id = Animator.StringToHash("isJumping");
   }

   void Update(){
      _OnMove(input.move);
      if (bSprintPressing) { _OnSprintPressing(); } else { _OnSprintReleasing(); }
   }
   bool bSprintPressing = false;
   public void _OnSprintPressing(){ bSprintPressing = true; }
   public void _OnSprintReleasing(){ bSprintPressing = false; }
   public void _OnJump() {

      var bPlayingNonLocomotionAnim = animator.GetBool(animators.playingNonLocomotionAnim_Id);
      if(bPlayingNonLocomotionAnim) return;

      var bJumping = animator.GetBool(animators.isJumping_Id);
      if(!bJumping){
         animator.CrossFade("Jump_1",.25f, 0, 0f);
         animator.SetBool(animators.isJumping_Id, true);
         Timer.CreateTimer_NoPhysics(this.gameObject, .7f, () => { animator.SetBool(animators.isJumping_Id, false); animator.CrossFade("Idle_1",.25f, 0, 0f); } );
      }
   }

   public void _OnMove(Vector2 inputVector) {
      if(animator.GetBool(animators.playingNonLocomotionAnim_Id)) return;
      if(animator.GetBool(animators.isJumping_Id)) return;

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

         animators.blendedValue = Mathf.Lerp(animators.blendedValue, movements.targetSpeed, Time.deltaTime * movements.speedChangeRate);
         if (animators.blendedValue < 0.01f) animators.blendedValue = 0f;
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

      void playAnimation() {
         var bJumping = animator.GetBool(animators.isJumping_Id);
         var bPlayingNonLocomotionAnim = animator.GetBool(animators.playingNonLocomotionAnim_Id);
         if(bPlayingNonLocomotionAnim) return;
         if(bJumping) return;
         
         var bMoving = animator.GetBool(animators.isMoving_Id);
         if(!bMoving) {
            animator.SetBool(animators.isMoving_Id, true);
            animator.CrossFade("Move_1",.5f, 0, 0f);
         }

         if(movements.speed <= 0.1f) {
            animator.Play("Idle_1");
         }
      }

      calculateTargetSpeed();
      smoothSpeedChange();
      calculateTargetRotation();
      applyMovementAndRotation();
      playAnimation();
   }

   void ResetToIdle(){
      if(animator.GetBool(animators.playingNonLocomotionAnim_Id)) return;
      if(animator.GetBool(animators.isJumping_Id)) return;
      if(animator.GetBool(animators.isMoving_Id)){
         animator.CrossFade("Idle_1",.5f, 0, 0f);
         animator.SetBool(animators.isMoving_Id, false);
      }
   }



   void _OnLook(Vector2 inputVector) {
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
      _OnLook(input.look);
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
}