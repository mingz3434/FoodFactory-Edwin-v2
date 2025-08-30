using UnityEngine;
using System.Collections.Generic;

public class DeliveryPlate : Actor_Game{

   public List<Food> foods;

   public void OnCollisionEnter(Collision collision){
      var go = collision.gameObject;
      var tray = go.GetComponent<FoodTray>();
      if(tray){
         foods.Add(tray.food);
      }

   }
}