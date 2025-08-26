using System;
using UnityEngine;

public class Order_UI : UserWidget{
   public static Order_UI Create_Order_UI(Order_UI prefab, Transform parentTransform, Order order){
      var order_UI = Instantiate(prefab, parentTransform);
      order_UI.GetIdText().text = order.id.ToString();
      order_UI.AddFoodProductRquired("Fried Chicken", 8);

   }
   
   public void AddFoodProductRequired(string name, int quantity){
      var cont = new GameObject("FoodProductRequired"); cont.transform.SetParent(this.transform);
      var text_GO = new GameObject("Text"); text_GO.transform.SetParent(cont.transform); var text = text_GO.AddComponent<TMP_Text>();
      var image_GO = new GameObject("Image"); image_GO.transform.SetParent(cont.transform); var image = image_GO.AddComponent<Image>();
   }
}