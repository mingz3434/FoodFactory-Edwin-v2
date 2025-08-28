using System;
using System.Collections.Generic;
using UnityEngine;
using _ = GameInstance;

public class Order : Actor_Game{

   [ReadOnly] public DeliveryPlate deliveryPlate;
   public Dictionary<Food, int> orderedFood_Quantity_Map = new Dictionary<Food, int>(); //!!! aka sub-orders
   public int orderId;
   public int waitedTime = 0;

   public static Order CreateEmptyOrder(Order prefab, Transform parentTransform, int orderId){
      Order order = Instantiate(prefab, parentTransform);
      order.orderId = orderId;
      return order;
   }

   public void AddSuborder(Food product, int quantity){
      orderedFood_Quantity_Map.Add(product, quantity);
   }

   public void PairingThePlate(DeliveryPlate deliveryPlate){
      this.deliveryPlate = deliveryPlate;
   }

   public void OnStackedNewFood_Order(){
      if(deliveryPlate.foods.Count == this.orderedFood_Quantity_Map.Count){
         // isCompleted = true;
      }
   }

   public void OnStackedFoodCount_Eq_OrderedFoodCount(){

      Action calculateScore = () => {
         int _score = 0;
         foreach(var orderedFoodName in orderedFood_Quantity_Map.Keys){
            foreach(Food food in deliveryPlate.foods){
               // if(food.rawFood == orderedFoodName.ToProduct().rawFood){
               //    _score += 5;
               // }
               // if(food.currentProduct == orderedFoodName.ToProduct().productName){
               //    _score += 15;
               // }
            }
         }
         (_.gs as GameState_Game).inGameInfo.score = _score;
      };


      Destroy(deliveryPlate.gameObject);
      Destroy(this.gameObject);
   }


   void Start(){
      //Action due = () => { (_.gs as GameState_Game).inGameInfo.score -= 20; Destroy(this.gameObject); };

      //Timer.CreateTimer_Physics(this.gameObject, 20f, ()=>due());
   }
}


