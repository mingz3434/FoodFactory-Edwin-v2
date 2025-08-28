using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Order_UI : UserWidget{

   public GameObject suborder_Prefab;

   public TMP_Text GetIdText(){ return this.transform.GetChild(0).GetComponent<TMP_Text>(); }

   public void SetIdText(string text){ this.transform.GetChild(0).GetComponent<TMP_Text>().text = text; }

   public void UI_AddSuborder(Food food, int quantity){
      var suborder = Instantiate(suborder_Prefab, this.transform);
      var text = suborder.transform.GetChild(0).GetComponent<TMP_Text>();
      var image = suborder.transform.GetChild(1).GetComponent<Image>();
      text.text = $"{quantity}x";
      image.sprite = Resources.Load<Sprite>($"Sprites/Food/{food.productName.ToSpriteFileName()}");
   }

   public static Order_UI CreateEmptyOrderUI(Order_UI prefab, Transform containerTransform){
      var order_UI = Instantiate(prefab, containerTransform);
      return order_UI;
   }

   public Order_UI UI_Set_UI_ByData(Order order){

      this.SetIdText($"Order {order.orderId.ToString()}");

      var map = order.orderedFood_Quantity_Map;
      foreach(var orderedFood_Quantity in map){
         this.UI_AddSuborder(orderedFood_Quantity.Key, orderedFood_Quantity.Value);
      }

      return this;
   }
}