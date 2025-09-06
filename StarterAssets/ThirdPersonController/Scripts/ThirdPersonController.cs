using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System;

public class ThirdPersonController : MonoBehaviour {

   [System.Serializable] public struct MovementSettings { public float moveSpeed; public float sprintSpeed; public float rotationSmoothTime; public float speedChangeRate; [HideInInspector] public float speed; [HideInInspector] public float targetRotation; [HideInInspector] public float rotationVelocity; [HideInInspector] public float targetSpeed; }
   [System.Serializable] public struct JumpSettings { public float jumpHeight; public float gravity; public float jumpTimeout; public float fallTimeout; [HideInInspector] public float verticalVelocity; [HideInInspector] public float terminalVelocity; [HideInInspector] public float jumpTimeoutDelta; [HideInInspector] public float fallTimeoutDelta; }
   [System.Serializable] public struct GroundSettings { public bool grounded; public float groundedOffset; public float groundedRadius; public LayerMask groundLayers; }
   [System.Serializable] public struct CameraSettings { public GameObject cinemachineCameraTarget; public float topClamp; public float bottomClamp; public float cameraAngleOverride; public bool lockCameraPosition; [HideInInspector] public float cinemachineTargetYaw; [HideInInspector] public float cinemachineTargetPitch; }
   [System.Serializable] public struct AudioSettings { public AudioClip landingAudioClip; public AudioClip[] footstepAudioClips; public float footstepAudioVolume; }
   [System.Serializable] public struct AnimatorSettings { public float blendedValue; public int speedParam; public int isGroundedBool; public int jumpBool; public int freeFallBool; public int motionSpeedParam; }

   public MovementSettings movements;
   public JumpSettings jumps;
   public GroundSettings grounds;
   public CameraSettings cameras;
   public AudioSettings audios;
   public AnimatorSettings animators;

   #if ENABLE_INPUT_SYSTEM 
   private PlayerInput _playerInput;
   #endif
   private Animator _animator;
   private CharacterController _controller;
   private StarterAssetsInputs _input;
   private GameObject _mainCamera;

   private const float _threshold = 0.01f;
   private bool _hasAnimator;

   private bool IsCurrentDeviceMouse {
      get {
         #if ENABLE_INPUT_SYSTEM
         return _playerInput.currentControlScheme == "KeyboardMouse";
         #else
         return false;
         #endif
      }
   }

   private void Awake() {
      if (_mainCamera == null) { _mainCamera = GameObject.FindGameObjectWithTag("MainCamera"); }
      //!!! Bind grounds.groundLayers, cameras.cinemachineCameraTarget, audios.landingAudioClip, audios.footstepAudioClips in Inspector
      //!!! groundLayers to "Default" and cct to "CameraRoot" plssssssss!!!
      var gl = grounds.groundLayers; var cct = cameras.cinemachineCameraTarget;
      var ld = audios.landingAudioClip; var fs = audios.footstepAudioClips;
      movements = new MovementSettings { moveSpeed = 2.0f, sprintSpeed = 5.335f, rotationSmoothTime = 0.12f, speedChangeRate = 10.0f };
      jumps = new JumpSettings { jumpHeight = 1.2f, gravity = -15.0f, jumpTimeout = 0.50f, fallTimeout = 0.15f, terminalVelocity = 53.0f };
      grounds = new GroundSettings { grounded = true, groundedOffset = -0.14f, groundedRadius = 0.28f, groundLayers = gl };
      cameras = new CameraSettings { topClamp = 70.0f, bottomClamp = -30.0f, cameraAngleOverride = 0.0f, lockCameraPosition = false, cinemachineCameraTarget = cct };
      audios = new AudioSettings { footstepAudioVolume = 0.5f, landingAudioClip = ld, footstepAudioClips = fs };
   }

   private void Start() {
      cameras.cinemachineTargetYaw = cameras.cinemachineCameraTarget.transform.rotation.eulerAngles.y;

      _hasAnimator = TryGetComponent(out _animator);
      _controller = GetComponent<CharacterController>();
      _input = GetComponent<StarterAssetsInputs>();
      #if ENABLE_INPUT_SYSTEM 
      _playerInput = GetComponent<PlayerInput>();
      #else
      Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
      #endif

      AssignAnimationIDs();

      jumps.jumpTimeoutDelta = jumps.jumpTimeout;
      jumps.fallTimeoutDelta = jumps.fallTimeout;
   }

   private void Update() {
      _hasAnimator = TryGetComponent(out _animator);

      JumpAndGravity();
      GroundedCheck_SetAnimator_IsGroundedBool();
      Move();
   }

   private void LateUpdate() {
      CameraRotation();
   }

   private void AssignAnimationIDs() {
      animators.speedParam = Animator.StringToHash("Speed");
      animators.isGroundedBool = Animator.StringToHash("Grounded");
      animators.jumpBool = Animator.StringToHash("Jump");
      animators.freeFallBool = Animator.StringToHash("FreeFall");
      animators.motionSpeedParam = Animator.StringToHash("MotionSpeed");
   }

   private void GroundedCheck_SetAnimator_IsGroundedBool() {
      Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - grounds.groundedOffset,
         transform.position.z);
      grounds.grounded = Physics.CheckSphere(spherePosition, grounds.groundedRadius, grounds.groundLayers,
         QueryTriggerInteraction.Ignore);

      if (_hasAnimator) {
         _animator.SetBool(animators.isGroundedBool, grounds.grounded);
      }
   }

   private void CameraRotation() {
      if (_input.look.sqrMagnitude >= _threshold && !cameras.lockCameraPosition) {
         float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

         cameras.cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
         cameras.cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
      }

      cameras.cinemachineTargetYaw = ClampAngle(cameras.cinemachineTargetYaw, float.MinValue, float.MaxValue);
      cameras.cinemachineTargetPitch = ClampAngle(cameras.cinemachineTargetPitch, cameras.bottomClamp, cameras.topClamp);

      cameras.cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cameras.cinemachineTargetPitch + cameras.cameraAngleOverride,
         cameras.cinemachineTargetYaw, 0.0f);
   }

   private void Move() {
      Action calculateTargetSpeed = () => {
         float targetSpeed = _input.sprint ? movements.sprintSpeed : movements.moveSpeed;
         if (_input.move == Vector2.zero) targetSpeed = 0.0f;
         movements.targetSpeed = targetSpeed;
      };

      Action smoothSpeedChange = () => {
         float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

         float speedOffset = 0.1f;
         float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

         if (currentHorizontalSpeed < movements.targetSpeed - speedOffset ||
            currentHorizontalSpeed > movements.targetSpeed + speedOffset) {
            movements.speed = Mathf.Lerp(currentHorizontalSpeed, movements.targetSpeed * inputMagnitude,
               Time.deltaTime * movements.speedChangeRate);

            movements.speed = Mathf.Round(movements.speed * 1000f) / 1000f;
         } else {
            movements.speed = movements.targetSpeed;
         }

         animators.blendedValue = Mathf.Lerp(animators.blendedValue, movements.targetSpeed, Time.deltaTime * movements.speedChangeRate);
         if (animators.blendedValue < 0.01f) animators.blendedValue = 0f;
      };

      Action calculateTargetRotation = () => {
         Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

         if (_input.move != Vector2.zero) {
            movements.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
               _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, movements.targetRotation, ref movements.rotationVelocity,
               movements.rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
         }
      };

      Action applyMovementAndRotation = () => {
         Vector3 targetDirection = Quaternion.Euler(0.0f, movements.targetRotation, 0.0f) * Vector3.forward;

         _controller.Move(targetDirection.normalized * (movements.speed * Time.deltaTime) +
            new Vector3(0.0f, jumps.verticalVelocity, 0.0f) * Time.deltaTime);
      };

      Action paramSyncToAnimator = () => {
         if (_hasAnimator) {
            _animator.SetFloat(animators.speedParam, animators.blendedValue);
            _animator.SetFloat(animators.motionSpeedParam, _input.move.magnitude);
         }
      };

      calculateTargetSpeed();
      smoothSpeedChange();
      calculateTargetRotation();
      applyMovementAndRotation();
      paramSyncToAnimator();
   }

   private void JumpAndGravity() {
      if (grounds.grounded) {
         jumps.fallTimeoutDelta = jumps.fallTimeout;

         if (_hasAnimator) {
            _animator.SetBool(animators.jumpBool, false);
            _animator.SetBool(animators.freeFallBool, false);
         }

         if (jumps.verticalVelocity < 0.0f) {
            jumps.verticalVelocity = -2f;
         }

         if (_input.jump && jumps.jumpTimeoutDelta <= 0.0f) {
            jumps.verticalVelocity = Mathf.Sqrt(jumps.jumpHeight * -2f * jumps.gravity);

            if (_hasAnimator) {
               _animator.SetBool(animators.jumpBool, true);
            }
         }

         if (jumps.jumpTimeoutDelta >= 0.0f) {
            jumps.jumpTimeoutDelta -= Time.deltaTime;
         }
      } else {
         jumps.jumpTimeoutDelta = jumps.jumpTimeout;

         if (jumps.fallTimeoutDelta >= 0.0f) {
            jumps.fallTimeoutDelta -= Time.deltaTime;
         } else {
            if (_hasAnimator) {
               _animator.SetBool(animators.freeFallBool, true);
            }
         }

         _input.jump = false;
      }

      if (jumps.verticalVelocity < jumps.terminalVelocity) {
         jumps.verticalVelocity += jumps.gravity * Time.deltaTime;
      }
   }

   private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
      if (lfAngle < -360f) lfAngle += 360f;
      if (lfAngle > 360f) lfAngle -= 360f;
      return Mathf.Clamp(lfAngle, lfMin, lfMax);
   }

   private void OnDrawGizmosSelected() {
      Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
      Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
      Gizmos.color = grounds.grounded ? transparentGreen : transparentRed;
      Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - grounds.groundedOffset, transform.position.z), grounds.groundedRadius);
   }

   private void OnFootstep(AnimationEvent animationEvent) {
      if (animationEvent.animatorClipInfo.weight > 0.5f) {
         if (audios.footstepAudioClips.Length > 0) {
            var index = UnityEngine.Random.Range(0, audios.footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(audios.footstepAudioClips[index], transform.TransformPoint(_controller.center), audios.footstepAudioVolume);
         }
      }
   }

   private void OnLand(AnimationEvent animationEvent) {
      if (animationEvent.animatorClipInfo.weight > 0.5f) {
         AudioSource.PlayClipAtPoint(audios.landingAudioClip, transform.TransformPoint(_controller.center), audios.footstepAudioVolume);
      }
   }
}