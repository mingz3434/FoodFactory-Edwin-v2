using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_Game : UserWidget{

   public static HUD_Game CreateHUD(HUD_Game prefab, Transform parentTransform){
      return Instantiate(prefab, parentTransform);
   }

   public GameObject timer;
   public GameObject score; //Maybe use money afterwards
   public GameObject orders;
   public GameObject orderTray;
   



}