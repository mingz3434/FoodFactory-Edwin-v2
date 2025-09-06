using UnityEngine;
using Mirror;
using System;

public class TestCharacter : NetworkBehaviour{
   float pitch = 0f, yaw = 0f;
   public Rigidbody rb;
   public Animator animator;
   public Camera _camera;
   public int bPlayingNonLocomotionAnim_Id = Animator.StringToHash("playingNonLocomotionAnim");
   public int bIsJumping_Id = Animator.StringToHash("isJumping");
   public int bIsMoving_Id = Animator.StringToHash("isMoving");
   public float rotationVelocity = .12f;
   public float rotationSmoothTime = .5f;
   public float targetRotation;
   void Start(){
      rb = GetComponent<Rigidbody>();
   }


   void Update(){
      var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

      Action calculateTargetRotation = () => {
         Vector3 inputDirection = new Vector3(input.x, 0.0f, input.y).normalized;

         if (input != Vector2.zero) {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
               _camera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity,
               rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
         }
      };
      Action applyMovement = () => {
         if(input == Vector2.zero) return;
         if (input.y > 0) rb.MovePosition(rb.position + transform.forward * 4f * Time.deltaTime);
         if (input.y < 0) rb.MovePosition(rb.position - transform.forward * 4f * Time.deltaTime);
      };

      calculateTargetRotation();
      applyMovement();

      var bJumping = animator.GetBool("isJumping");
      if(bJumping) return;

      var bMoving = animator.GetBool(bIsMoving_Id);
      if(!bMoving){
         animator.CrossFade("Move_1",.5f, 0, 0f);
         // animator.Play("Move_1", 0);
         animator.SetBool(bIsMoving_Id, true);
      }
   }
   public void Move(Vector3 movement){
      var bJumping = animator.GetBool("isJumping");
      var bPlayingNonLocomotionAnim = animator.GetBool(bPlayingNonLocomotionAnim_Id);

      if(bPlayingNonLocomotionAnim) return;

      var inputDirection = movement.normalized;
      var targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _camera.transform.eulerAngles.y;
      var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
      transform.rotation = Quaternion.Euler(0f, rotation, 0f);
      
      // var targetDirection = Quaternion.Euler(0, targetRotation, 0) * Vector3.forward;
      rb.MovePosition(transform.position + transform.forward * movement.magnitude * 80 * Time.deltaTime);

      if(bJumping) return;

      var bMoving = animator.GetBool(bIsMoving_Id);
      if(!bMoving){
         animator.CrossFade("Move_1",.5f, 0, 0f);
         // animator.Play("Move_1", 0);
         animator.SetBool(bIsMoving_Id, true);
      }
      
   }


   public void Idle(){
      var bPlayingNonLocomotionAnim = animator.GetBool(bPlayingNonLocomotionAnim_Id);
      if(bPlayingNonLocomotionAnim) return;

      var bMoving = animator.GetBool(bIsMoving_Id);
      if(bMoving){
         animator.CrossFade("Idle_1",.5f, 0, 0f);
         // animator.Play("Idle_1", 0);
         animator.SetBool(bIsMoving_Id, false);
      }
   }

   public void Jump(){
      var bPlayingNonLocomotionAnim = animator.GetBool(bPlayingNonLocomotionAnim_Id);
      if(bPlayingNonLocomotionAnim) return;

      var bJumping = animator.GetBool(bIsJumping_Id);
      if(!bJumping){
         animator.CrossFade("Jump_1",.25f, 0, 0f);
         animator.SetBool(bIsJumping_Id, true);
         Timer.CreateTimer_NoPhysics(this.gameObject, .7f, () => { animator.SetBool(bIsJumping_Id, false); animator.CrossFade("Idle_1",.25f, 0, 0f); } );
      }
   }
}



   // private void Move() {




   //    Action calculateTargetRotation = () => {
   //       Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

   //       if (_input.move != Vector2.zero) {
   //          movements.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
   //             _mainCamera.transform.eulerAngles.y;
   //          float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, movements.targetRotation, ref movements.rotationVelocity,
   //             movements.rotationSmoothTime);

   //          transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
   //       }
   //    };

   //    Action applyMovementAndRotation = () => {
   //       Vector3 targetDirection = Quaternion.Euler(0.0f, movements.targetRotation, 0.0f) * Vector3.forward;

   //       _controller.Move(targetDirection.normalized * (movements.speed * Time.deltaTime) +
   //          new Vector3(0.0f, jumps.verticalVelocity, 0.0f) * Time.deltaTime);
   //    };

   //    Action paramSyncToAnimator = () => {
   //       if (_hasAnimator) {
   //          _animator.SetFloat(animators.speedParam, animators.blendedValue);
   //          _animator.SetFloat(animators.motionSpeedParam, _input.move.magnitude);
   //       }
   //    };

   //    calculateTargetSpeed();
   //    smoothSpeedChange();
   //    calculateTargetRotation();
   //    applyMovementAndRotation();
   //    paramSyncToAnimator();
   // }