using System;
using System.Collections;
using UnityEngine;
using Mirror;
using _ = GameInstance;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager {

   public int nextPlayerId = 0; // 用於分配 playerId
   public Dictionary<NetworkConnection, int> playerIds = new Dictionary<NetworkConnection, int>();

   public List<int> playerIdList;
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

   public override void OnServerAddPlayer(NetworkConnectionToClient newConnection){
      base.OnServerAddPlayer(newConnection);

      if (newConnection.identity != null){

         var assignedPlayerId = nextPlayerId++;

         playerIds.Add(newConnection, assignedPlayerId);
         Debug.Log(newConnection.connectionId);
         playerIdList.Add(assignedPlayerId);

         newConnection.identity.GetComponent<ThirdPerson_PC>().playerId = assignedPlayerId;

      }
   }

   public int GetSelfPlayerId(NetworkConnectionToClient conn){
      Debug.Log("Est conn:"+playerIds.Count);
      return playerIds.ContainsKey(conn) ? playerIds[conn] : -1;
   }

}