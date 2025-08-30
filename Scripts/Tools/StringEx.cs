public static class StringEx{
   public static string ToSpriteFileName(this string productName){
      switch(productName){
         case "Fried Chicken" : return "ChickenFried";
         case "Cut Chicken" : return "ChickenCut";
         case "Failed Chicken" : return "ChickenFailed";
         case "Chicken Nuggets" : return "ChickenNuggets";
         case "Raw Chicken" : return "ChickenRaw";
         default : return productName;

      }
   }

}