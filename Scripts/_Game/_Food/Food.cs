using UnityEngine;
using _ = GameInstance;

public class Food : Actor_Game{

   [ReadOnly] public FoodTray tray;
   [ReadOnly] GameState_Game gs;

   public enum RawFood{ Chicken, Potato }
   
   [SerializeField] public RawFood rawFood;
   public string productName = ""; //! As key!!!

   public static Food Create_NonActing_Food(Transform parentTransform, RawFood rawFood, string productName){
      var food = new GameObject("Non-acting: " + productName).AddComponent<Food>();
      food.rawFood = rawFood;
      food.productName = productName;
      food.transform.SetParent(parentTransform);
      food.gameObject.SetActive(false);
      return food;
   }

   public static Food CreateFood(FoodTray tray, Food prefab, Transform parentTransform, RawFood rawFood){
      var food = Instantiate(prefab, parentTransform);
      food.tray = tray;
      food.gs = _.gs as GameState_Game;
      var spriteRenderer = food.GetComponent<SpriteRenderer>();
      switch(rawFood){
         case RawFood.Chicken: spriteRenderer.sprite = food.gs.assets.chickenRaw_Sprite; food.gameObject.name = "Raw Chicken"; food.rawFood = rawFood; break;
         case RawFood.Potato: spriteRenderer.sprite = food.gs.assets.potatoRaw_Sprite; food.gameObject.name = "Raw Potato"; food.rawFood = rawFood; break;
         default: break;
      }
      food.productName = rawFood==RawFood.Chicken ? "Raw Chicken" : "Raw Potato";
      return food;
   }

}