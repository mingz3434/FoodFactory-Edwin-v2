using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

public class PI_PC : NetworkBehaviour{
   PlayerInput playerInput;
   public GameObject _camera;
   public Material black;
   public class V { public int vx, vy; }
   public V v = new V();
   void Awake() { playerInput = GetComponent<PlayerInput>(); }

   void Start() { if(!isLocalPlayer) _camera.SetActive(false); if(isLocalPlayer) this.GetComponent<MeshRenderer>().material = black; }
   public override void OnStartLocalPlayer(){
      base.OnStartLocalPlayer();
      playerInput.enabled = true;
   }

   public override void OnStopLocalPlayer(){
      base.OnStopLocalPlayer();
      playerInput.enabled = false;
   }

   // 處理移動輸入
   public void OnMove(InputAction.CallbackContext context){
      if (!isLocalPlayer) return;
      var value = context.ReadValue<Vector2>();
      v.vx = (int)value.x;
      v.vy = (int)value.y;
      // CmdMove(moveInput);
   }

   // [Command]
   // private void CmdMove(Vector2 input){
   //    Vector3 movement = new Vector3(input.x, 0, input.y) * Time.deltaTime * 5f;
   //    transform.Translate(movement);
   // }

   void Update(){
      if(v.vx == 1) transform.Translate(Vector3.right * Time.deltaTime * 5f);
      if(v.vy == 1) transform.Translate(Vector3.forward * Time.deltaTime * 5f);
      if(v.vx == -1) transform.Translate(Vector3.left * Time.deltaTime * 5f);
      if(v.vy == -1) transform.Translate(Vector3.back * Time.deltaTime * 5f);
   }
}