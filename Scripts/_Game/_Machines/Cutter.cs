using UnityEngine;

public class Cutter : Machine{

   public GameObject slot;

   public static Cutter CreateCutter(Cutter prefab, Transform parentTransform, Vector3 position){
      var cutter = Instantiate(prefab, parentTransform);
      cutter.transform.position = position;
      return cutter;
   }

}