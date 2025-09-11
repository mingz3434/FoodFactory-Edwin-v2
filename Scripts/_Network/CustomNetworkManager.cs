using System;
using System.Collections;
using UnityEngine;
using Mirror;
using _ = GameInstance;

public class CustomNetworkManager : NetworkManager {

   public override void OnStartServer() {
      Debug.Log("Mirror: Server started.");
   }
   public override void OnStartClient() {
      Debug.Log("Mirror: Client started.");
   }

   public override void OnServerSceneChanged(string sceneName) {
      base.OnServerSceneChanged(sceneName);
      Debug.Log("Mirror: Server scene changed.");  
   }

   private static int nextPlayerId = 0;

   public override void OnServerAddPlayer(NetworkConnectionToClient newConnection){
      base.OnServerAddPlayer(newConnection);

      if (newConnection.identity != null){
         ThirdPerson_PC playerScript = newConnection.identity.GetComponent<ThirdPerson_PC>();
         if (playerScript != null){
               playerScript.playerId = nextPlayerId;
               nextPlayerId++;
               Debug.Log($"已為連接 {newConnection.connectionId} 分配玩家 ID: {playerScript.playerId}");
         }
      }
   }
}