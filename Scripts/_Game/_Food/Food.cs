using UnityEngine;
using _ = GameInstance;
using Mirror;

public class Food : NetworkBehaviour{
   public FoodTray tray;
   public enum RawFood { Chicken, Potato }

   [SyncVar(hook = nameof(OnRawFoodChanged))] public RawFood rawFood;
   public string productName;
   private SpriteRenderer spriteRenderer;

   void Awake() { spriteRenderer = GetComponent<SpriteRenderer>(); }
   void Start() { UpdateSprite(rawFood); }
   void OnRawFoodChanged(RawFood oldValue, RawFood newValue) { UpdateSprite(newValue); }

   void UpdateSprite(RawFood foodType){
      var gs = _.gs as GameState_Game;
      if (!gs) { Debug.LogError("GameState_Game is null!"); return; }

      switch (foodType){
         case RawFood.Chicken:
            var chickenSprite = gs.assets.chickenRaw_Sprite;
            if (!chickenSprite) { Debug.LogError("chickenRaw_Sprite is null!"); return; }
            spriteRenderer.sprite = chickenSprite;
            productName = "Raw Chicken";
            gameObject.name = "Raw Chicken";
            break;

         case RawFood.Potato:
            var potatoSprite = gs.assets.potatoRaw_Sprite;
            if (!potatoSprite) { Debug.LogError("potatoRaw_Sprite is null!"); return; }
            spriteRenderer.sprite = potatoSprite;
            productName = "Raw Potato";
            gameObject.name = "Raw Potato";
            break;

         default:
            Debug.LogError("Invalid food type: " + foodType);
            break;

      }
   }
}