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
      rpcm.ps = this.ps;
   }

   void Update(){

   }

   void FixedUpdate(){
         if (isLocalPlayer){




         // if (Input.GetKeyDown(KeyCode.Space)) { Debug.Log("Space"); PickUpFoodLogics(); }
         if (Input.GetKey(KeyCode.Space)) { character.Jump(); }
         if (Input.anyKey == false ) { character.Idle(); }
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

      //! Has to set syncDirection before passing authority to client.
      // foodTray.syncDirection = SyncDirection.ClientToServer;

      // Pass authority to client.
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
         foodTray.transform.parent = pc.character.transform;
         foodTray.bInTrack = false; //! HOOK
         Debug.Log($"Rpc_SyncPickup: Food synced to Player {player.name}'s Food Slot!");
      }
   }
}

