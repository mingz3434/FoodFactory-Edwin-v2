using UnityEngine;
public class Hook : Actor_Game{
   public Hook CreateHook(Hook prefab, Transform parentTransform){
      return Instantiate(prefab, parentTransform);
   }
}