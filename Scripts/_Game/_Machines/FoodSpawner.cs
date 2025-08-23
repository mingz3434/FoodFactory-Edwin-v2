using UnityEngine;

public class FoodSpawner : Machine{

   public static FoodSpawner CreateFoodSpawner(FoodSpawner prefab, Transform parentTransform, Vector3 position, Quaternion rotation){
      var fs = Instantiate(prefab, position, rotation, parentTransform);
      fs.gameObject.name = prefab.gameObject.name;
      return fs;
   }

}