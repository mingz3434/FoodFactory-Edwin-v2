using System;
using System.Collections.Generic;
using UnityEngine;
using _ = GameInstance;

public class UserWidget : MonoBehaviour{

   public static T CreateWidget<T>(T prefab) where T : UserWidget{
      if (prefab == null){
         Debug.LogError("UW: Prefab not found!");
         return null;
      }
      T instance = Instantiate(prefab);
      instance.gameObject.name = prefab.gameObject.name;
      return instance;
   }

   public static T CreateWidget<T>(T prefab, Transform parentTransform, Vector3 position) where T : UserWidget{
      if (prefab == null){
         Debug.LogError("UW: Prefab not found!");
         return null;
      }
      T instance = Instantiate(prefab, parentTransform);
      instance.gameObject.name = prefab.gameObject.name;
      instance.GetComponent<RectTransform>().position = position;
      return instance;
   }

   public virtual void RemoveFromParent(){
      if(gameObject.activeInHierarchy){
         Destroy(gameObject);
      }
   }

}

