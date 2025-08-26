using System;
using System.Collections.Generic;
using UnityEngine;
using _ = GameInstance;

public class Order : Actor_Game{

   [ReadOnly] public OrderBox orderBox;
   public List<OrderedFood> orderedFoods = new List<OrderedFood>();
   public int orderNumber;
   public bool isCompleted = false;
   public int waitedTime = 0;

   public static Order CreateOrder(int orderNumber, List<OrderedFood> orderedFoods){
      Order order = new Order();
      order.orderNumber = orderNumber;
      order.orderedFoods = orderedFoods;
      return order;
   }

   public void PairingTheMat(OrderBox orderBox){
      this.orderBox = orderBox;
   }

   public void OnStackedNewFood_Order(){
      if(orderBox.foods.Count == orderedFoods.Count){
         isCompleted = true;
      }
   }

   public void OnStackedFoodCount_Eq_OrderedFoodCount(){

      Action calculateScore = () => {
         int _score = 0;
         foreach(OrderedFood orderedFood in orderedFoods){
            foreach(Food food in orderBox.foods){
               if(food.rawFood == orderedFood.rawFood){
                  _score += 5;
               }
               if(food.productName == orderedFood.productName){
                  _score += 15;
               }
            }
         }
         (_.gs as GameState_Game).score = _score;
      };


      Destroy(orderBox.gameObject);
      Destroy(this.gameObject);
   }


   void Start(){
      Action due = () => { (_.gs as GameState_Game).score -= 20; Destroy(this.gameObject); };

      Timer.CreateTimer_Physics(this.gameObject, 20f, ()=>due());
   }
}

public class OrderedFood{
   public Food.RawFood rawFood;
   public string productName;
   public int quantity;
}