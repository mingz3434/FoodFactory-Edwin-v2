using UnityEngine;
public class Hook : Actor_Game{
   public static Hook CreateHook(Hook prefab, Transform parentTransform){
      return Instantiate(prefab, parentTransform);
   }
}