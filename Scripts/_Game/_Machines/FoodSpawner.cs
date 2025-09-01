using UnityEngine;
using Mirror;

public class FoodSpawner : Machine{

   public static FoodSpawner CreateFoodSpawner(FoodSpawner prefab, Transform parentTransform, Vector3 position, Quaternion rotation){
      var fs = Instantiate(prefab, position, rotation, parentTransform);
      fs.gameObject.name = prefab.gameObject.name;
      NetworkServer.Spawn(fs.gameObject);
      return fs;
   }

   //* The responsibility has sent to be done at "ConveyorBeltSegment".
}