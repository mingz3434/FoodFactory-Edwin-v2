using System;
using UnityEngine;
using Mirror;
public class TestPC : NetworkBehaviour{

   public TestCharacter character;
   public TestPS ps;
   public TestPC_RPCM rpcm;

   void Start(){
      if(!isLocalPlayer){ var cam = this.transform.GetChild(0).GetChild(0).gameObject; cam.SetActive(false); }
      character.transform.SetParent(this.transform.parent);
      this.transform.SetParent(null);
      rpcm.character = this.character;
      rpcm.ps = this.ps;
   }

   void Update(){
      if (isLocalPlayer){
         float horizontalInput = Input.GetAxis("Horizontal");
         float verticalInput = Input.GetAxis("Vertical");
         
         // 計算移動向量
         Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized * Time.deltaTime * 2;
         
         // 更新角色位置
         character.Move(movement);
         
         // 將新位置發送到伺服器
         if (movement != Vector3.zero)
         {
               rpcm.UpdatePosition_ServerRPC(character.transform.position);
         }

         if (Input.GetKeyDown(KeyCode.Space)) { Debug.Log("Space"); PickUpFoodLogics(); }
      }
   }

   void PickUpFoodLogics(){

      //P: Get food below player.
      Ray ray = new Ray(character.transform.position, Vector3.down);
      RaycastHit hit;
      Physics.Raycast(ray, out hit, 4f);

      //P: Return when null.
      if(hit.collider == null) {Debug.Log("PC: PickUpFood: No hit collider."); return; }
      Debug.Log("PC: PickUpFood: Hit collider: " + hit.collider.name);
      //P: If it's food, no more move.
      if (hit.collider.CompareTag("Food")){
         var go = hit.collider.gameObject;
         Cmd_PickUpFood(go);
      }
      
   }

   [Command (requiresAuthority = false)]
   void Cmd_PickUpFood(GameObject go, NetworkConnectionToClient sender = null){
      if(!go || !go.GetComponent<NetworkIdentity>()){ Debug.LogWarning("Cmd_PickUpFood: Invalid food object or missing NetworkIdentity."); return;}
      if (sender == null || !sender.identity) { Debug.LogWarning("Cmd_PickUpFood: Invalid sender."); return; }
      var foodTray = go.GetComponent<FoodTray>(); if(!foodTray){ Debug.LogWarning("Cmd_PickUpFood: Invalid food tray."); return; }
      var pc = sender.identity.GetComponent<TestPC>(); if(!pc){ Debug.LogWarning("Cmd_PickUpFood: Invalid player controller."); return; }

      var identity = go.GetComponent<NetworkIdentity>();
      identity.AssignClientAuthority(sender);

      Rpc_SyncPickup(go, sender.identity.gameObject);
   }

   [ClientRpc]
   void Rpc_SyncPickup(GameObject foodObject, GameObject player){
      // 在客戶端同步拾取狀態
      if (foodObject == null || player == null) return;

      TestPC pc = player.GetComponent<TestPC>();
      if (!pc) return;

      FoodTray foodTray = foodObject.GetComponent<FoodTray>();
      if (foodTray != null){
         foodTray.transform.parent = pc.character.transform + Vector3.up * 1.5f;
         foodTray.bInTrack = false;
         Debug.Log($"Rpc_SyncPickup: Food synced to Player {player.name}'s Food Slot!");
      }
   }
}

