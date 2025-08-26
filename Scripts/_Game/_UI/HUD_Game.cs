using UnityEngine;
using UnityEngine.UI;
using TMPro;
using _ = GameInstance;
using System;

public class HUD_Game : UserWidget{

   public static HUD_Game CreateHUD(HUD_Game prefab, Transform parentTransform){
      return Instantiate(prefab, parentTransform);
   }

   public GameObject timer;
   public GameObject score; //Maybe use money afterwards
   public GameObject orders;
   public GameObject orderTray;
   
   void Start(){
      
   }

   void Update(){
      Action updateTimer = () => {
         var remainingTime = (_.gs as GameState_Game).remainingTime;
         var mm = remainingTime / 60;
         var ss = remainingTime % 60;
         timer.GetComponent<TMP_Text>().text = mm.ToString("00") + ":" + ss.ToString("00");
      };

      updateTimer();
      
      Action updateOrders = () => {
         var orders = (_.gs as GameState_Game).orders;
         var orderFirst = orders[0];
         var orderSecond = orders[1];
         var orderThird = orders[2];
      };
      
      updateOrders();
      

      
   }

}