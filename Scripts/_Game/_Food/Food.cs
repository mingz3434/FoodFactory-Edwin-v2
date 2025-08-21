using UnityEngine;
using _ = GameInstance;

public class Food : Actor_Game{

   [ReadOnly] GameState_Game gs;

   public enum RawFood{ Chicken, Potato }
   public enum State{ Raw, Sliced, Stirred, Fried, DishReady }
   public static Food CreateFood(Food prefab, Transform parentTransform, RawFood rawFood){
      var food = Instantiate(prefab, parentTransform);
      food.gs = _.gs as GameState_Game;
      var spriteRenderer = food.GetComponent<SpriteRenderer>();
      switch(rawFood){
         case RawFood.Chicken: spriteRenderer.sprite = food.gs.assets.chickenRaw_Sprite; break;
         case RawFood.Potato: spriteRenderer.sprite = food.gs.assets.potatoRaw_Sprite; break;
         default: break;
      }
      return food;
   }

}