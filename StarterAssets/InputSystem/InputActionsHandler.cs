using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using _ = GameInstance;



public class InputActionsHandler : NetworkBehaviour{
	public PlayerInput playerInput;
   public ThirdPerson_PC pc;
   public InputAction jumpAction;
   [Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public bool jump;
	public bool sprint;

	[Header("Movement Settings")]
	public bool analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;

	void Awake(){
		playerInput = this.GetComponent<PlayerInput>();
		jumpAction = playerInput.actions["Jump"];
	}
	void OnEnable(){
	   jumpAction.performed += OnJump_KeyDown;
	}

	void OnDisable(){
	   jumpAction.performed -= OnJump_KeyDown;
	}

	public void OnMove(InputValue value) { move = value.Get<Vector2>(); }


	// public void OnLook(InputAction.CallbackContext context) { if(cursorInputForLook) { look = context.ReadValue<Vector2>(); } }
	public void OnLook(InputValue value) { if(cursorInputForLook) { look = value.Get<Vector2>(); } }
   public void OnJump_KeyDown(InputAction.CallbackContext context) { pc.Jump(); }

   public void OnSprint(InputValue value) { sprint = value.isPressed; }
	private void OnApplicationFocus(bool hasFocus) { Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None; }


}

