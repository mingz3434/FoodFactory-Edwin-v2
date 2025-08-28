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
   public GameObject ordersContainer;
   public GameObject deliveryPlate_Prompt;
   
   public Order_UI order_UI_Prefab;
   Order_UI order_UI_Inst;

   void Start(){
      var prefab = (_.gs as GameState_Game).prefabs.order_Prefab;
      var order = Order.CreateEmptyOrder(prefab, this.transform, 1);
      order.AddSuborder("H",2);
      order.AddSuborder("zg",2);
      HUD_AddOrderUI_ByOrder(order);
      
   }

   void HUD_AddOrderUI_ByOrder(Order order){
      var orderUI_Inst = Order_UI.CreateEmptyOrderUI(this.order_UI_Prefab, this.ordersContainer.transform);
      orderUI_Inst.UI_Set_UI_ByData(order);

   }

   void Update(){
      Action updateTimer = () => {
         var remainingTime = (_.gs as GameState_Game).inGameInfo.remainingTime;
         var mm = remainingTime / 60;
         var ss = remainingTime % 60;
         timer.GetComponent<TMP_Text>().text = mm.ToString("00") + ":" + ss.ToString("00");
      };

      updateTimer();

      
   }


}