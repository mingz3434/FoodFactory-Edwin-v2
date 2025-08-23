using UnityEngine;

public class Fryer : Machine{

   public static Fryer CreateFryer(Fryer prefab, Transform parentTransform, Vector3 position){
      var fryer = Instantiate(prefab, parentTransform);
      fryer.transform.position = position;
      return fryer;
   }

}