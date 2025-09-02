using UnityEngine;
using Mirror;
using BroadcastToClients = Mirror.ClientRpcAttribute;
using CallServer = Mirror.CommandAttribute;
using ServerActive = Mirror.ServerAttribute;
using UnityEngine.Splines;
using _ = GameInstance;

public class GameState_Game_RPCM : NetworkBehaviour {

   public GameState_Game gs;

   [Server]
   public void Server_StartTimer() { //Broadcast timer time by SyncVar
      Timer.CreateTimer_NoPhysics(
         gs.gameObject,
         1f,
         () => {
            gs.inGameInfo_remainingTime -= 1;
            Debug.Log("Timer tick: " + gs.inGameInfo_remainingTime);
            var b = gs.inGameInfo_remainingTime <= 0;
            if(!b) {
               Server_StartTimer();
            }
         }
      );
   }

   [ServerActive]
   public void Server_GenerateConveyors() {
      var spline = gs.splineContainer.Spline;
      var segmentCount = Mathf.CeilToInt(spline.GetLength() / gs.conveyorSettings.segmentLength);
      for (int i = 0; i < segmentCount; i++) {
         var portionValue = i * gs.conveyorSettings.segmentLength / spline.GetLength();
         var position = spline.EvaluatePosition(portionValue); position.y += .5f; var tangent = spline.EvaluateTangent(portionValue); var up = spline.EvaluateUpVector(portionValue);
         var rotation = Quaternion.LookRotation(tangent, up);
         var trackRotation = rotation * Quaternion.Euler(90, 0, 0);

         if (i == 0) {
            var foodSpawner = FoodSpawner.CreateFoodSpawner(gs.prefabs.foodSpawner_Prefab, gs.transforms.conveyorBeltContainerTransform, position, rotation);
         }
         else {
            var conveyorBeltSegment = ConveyorBeltSegment.CreateConveyorBeltSegment(gs.prefabs.conveyorBeltSegment_Prefab, gs.transforms.conveyorBeltContainerTransform, position, trackRotation, i - 1);
         }

      }
   }

   [ServerActive]
   public void Server_RegularSpawnFood(){
      Timer.CreateTimer_Physics(gs.gameObject, 2f, () => {
         // Client_LogWarning(_.gameInstance ? "gi exists" : "gi does not exist");
         // Client_LogWarning(_.gs ? "gs exists" : "gs does not exist");
         var foodTray = FoodTray.CreateFoodTray(gs.prefabs.foodTray_Prefab, gs.transforms.foodTrayOnBeltContainerTransform);
         Server_RegularSpawnFood();
      });
   }

   [BroadcastToClients]
   public void Client_LogWarning(string message){
      Debug.LogWarning(message);
   }
}