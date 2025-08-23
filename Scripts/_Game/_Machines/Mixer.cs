using UnityEngine;

public class Mixer : Machine{

   public GameObject slot;

   public static Mixer CreateMixer(Mixer prefab, Transform parentTransform, Vector3 position){
      var mixer = Instantiate(prefab, parentTransform);
      mixer.transform.position = position;
      return mixer;
   }

}