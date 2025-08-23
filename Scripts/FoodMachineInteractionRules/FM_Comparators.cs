using UnityEngine;
using _ = GameInstance;

public static class FM_Comparators{
   public static void Compare(FoodTray tray, Food food, Machine machine){

      if(food.rawFood == Food.RawFood.Chicken){
         Chicken_vs_Machine(tray, food, machine);
      }

      if(food.rawFood == Food.RawFood.Potato){
         Potato_vs_Machine(tray, food, machine);
      }

   }

   public static void Chicken_vs_Machine(FoodTray tray, Food chicken, Machine machine){
      //P: should use productName as key.
      var sr = chicken.GetComponent<SpriteRenderer>();

      if(chicken.productName == "Raw Chicken" && machine is Mixer mixer){
         tray.SnapTo(mixer.slot.transform);
         Debug.Log("Stirred Chicken!");
         chicken.productName = "Seasoned Chicken";
         sr.sprite = (_.gs as GameState_Game).assets.chickenStirred_Sprite;
      }

      if(chicken.productName == "Raw Chicken" && machine is Fryer fryer){
         tray.SnapTo(fryer.slots[0].transform);
         Debug.Log("Fried Big Chicken!");
         chicken.productName = "Fried Chicken";
         sr.sprite = (_.gs as GameState_Game).assets.chickenFried_Sprite;
      }

      if(chicken.productName == "Raw Chicken" && machine is Cutter cutter){
         tray.SnapTo(cutter.slot.transform);
         Debug.Log("Sliced Chicken!");
         chicken.productName = "Sliced Chicken";
         sr.sprite = (_.gs as GameState_Game).assets.chickenSliced_Sprite;
      }

      if(chicken.productName == "Sliced Chicken" && machine is Fryer fryer_){
         tray.SnapTo(fryer_.slots[0].transform);
         Debug.Log("Fried Nuggets!");
         chicken.productName = "Fried Chicken Nuggets";
         sr.sprite = (_.gs as GameState_Game).assets.nugget_Sprite;
      }

   }

   public static void Potato_vs_Machine(FoodTray tray, Food potato, Machine machine){
      var sr = potato.GetComponent<SpriteRenderer>();

      if(potato.productName == "Raw Potato" && machine is Cutter cutter){
         tray.SnapTo(cutter.slot.transform);
         Debug.Log("Sliced Potato!");
         potato.productName = "Sliced Potato";
         sr.sprite = (_.gs as GameState_Game).assets.potatoSliced_Sprite;
      }

      if(potato.productName == "Raw Potato" && machine is Fryer fryer){
         tray.SnapTo(fryer.slots[0].transform);
         Debug.Log("Fried Potato Wedges!");
         potato.productName = "Fried Potato Wedges";
         sr.sprite = (_.gs as GameState_Game).assets.potatoFried_Sprite;
      }

      if(potato.productName == "Sliced Pototo" && machine is Fryer fryer_){
         tray.SnapTo(fryer_.slots[0].transform);
         Debug.Log("Fried Chips!");
         potato.productName = "Fried Potato Chips";
         sr.sprite = (_.gs as GameState_Game).assets.potatoFried_Sprite;
      }
   }
}

public static class FM_Consequences{
   // public static void Chicken
}