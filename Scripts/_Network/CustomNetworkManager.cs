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
}