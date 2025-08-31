using UnityEngine;
using Mirror;
using BroadcastToClients = Mirror.ClientRpcAttribute;
using CallServer = Mirror.CommandAttribute;
using ServerActive = Mirror.ServerAttribute;
using UnityEngine.Splines;

public class GameState_Game_RPCM : NetworkBehaviour {

   public GameState_Game gs;

   [ServerActive]
   public void Server_StartTimer() { //Broadcast timer time by SyncVar
      Timer.CreateLoopingTimer_NoPhysics(gs.gameObject, 1f, () => gs.inGameInfo.remainingTime -= 1, gs.inGameInfo.remainingTime < 0);
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
         var foodTray = FoodTray.CreateFoodTray(gs.prefabs.foodTray_Prefab, gs.transforms.foodTrayOnBeltContainerTransform);
         Server_RegularSpawnFood();
      });
   }
}