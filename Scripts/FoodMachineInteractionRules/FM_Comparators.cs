using UnityEngine;
using _ = GameInstance;

public static class FM_Comparators{
   // Comparators for checking, less likely to do real stuffs.

   //P: 用原型食物做第一步篩選
   public static void Compare(Food food, Machine machine, int availableSlotId = -1){
      if(food.rawFood == Food.RawFood.Chicken){ Chicken_vs_Machine_Check(food, machine, availableSlotId); }
      if(food.rawFood == Food.RawFood.Potato){ Potato_vs_Machine_Check(food, machine, availableSlotId); }
   }

   //P: Should use productName as key.
   public static void Chicken_vs_Machine_Check(Food chicken, Machine machine, int availableSlotId = 0){
      var tray = chicken.tray;
      var sr = chicken.GetComponent<SpriteRenderer>();
      var assets = (_.gs as GameState_Game).assets;

      if(chicken.productName == "Raw Chicken" && machine is Mixer mixer){ mixer.Stir(chicken, "Stirred Chicken", assets.chickenStirred_Sprite); }
      if(chicken.productName == "Raw Chicken" && machine is Fryer fryer){ fryer.Fry(chicken, "Fried Chicken", assets.chickenFried_Sprite, availableSlotId); }
      if(chicken.productName == "Raw Chicken" && machine is Cutter cutter){ cutter.Slice(chicken, "Sliced Chicken", assets.chickenSliced_Sprite); }
      if(chicken.productName == "Sliced Chicken" && machine is Fryer fryer_){ fryer_.Fry(chicken, "Chicken Nuggets", assets.nugget_Sprite, availableSlotId); }
   }

   public static void Potato_vs_Machine_Check(Food potato, Machine machine, int availableSlotId){
      var tray = potato.tray;
      var sr = potato.GetComponent<SpriteRenderer>();
      var assets = (_.gs as GameState_Game).assets;

      if(potato.productName == "Raw Potato" && machine is Cutter cutter){ cutter.Slice(potato, "Sliced Potato", assets.potatoSliced_Sprite); }
      if(potato.productName == "Raw Potato" && machine is Fryer fryer){ fryer.Fry(potato, "Fried Potato", assets.potatoFried_Sprite, availableSlotId); }
      if(potato.productName == "Sliced Potato" && machine is Fryer fryer_){ fryer_.Fry(potato, "Fried Potato Wedges", assets.potatoFried_Sprite, availableSlotId); }
   }
}