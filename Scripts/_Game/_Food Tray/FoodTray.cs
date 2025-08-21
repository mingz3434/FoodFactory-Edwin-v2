using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;
using Unity.Mathematics;

public class FoodTray : Actor_Game {
   [ReadOnly] public GameState_Game gs;
   [ReadOnly] public Food food;
   public float portionValue;
   public float speed = 5f;
   public bool bPerserveMomentum = true;
   public float yOffset = 0.5f; // 食物在輸送帶上方的偏移
   private Rigidbody rb;

   public static FoodTray CreateFoodTray(FoodTray prefab, Transform parentTransform){
      var tray = Instantiate(prefab, parentTransform);
      tray.gs = _.gs as GameState_Game;
      var sc = tray.gs.splineContainer;
      tray.transform.position = sc.EvaluatePosition(0f);
      tray.transform.Translate(new Vector3(0,1f,0));
      
      System.Random r = new System.Random();
      tray.food = Food.CreateFood(tray.gs.prefabs.food_Prefab, tray.transform, (Food.RawFood) r.Next(0,2));

      return tray;
   }

   void Start(){
      rb = this.GetComponent<Rigidbody>();
   }

   void FixedUpdate(){
      if (gs.splineContainer == null) return;
      portionValue += .03f * Time.fixedDeltaTime;

      var newPosition = gs.splineContainer.EvaluatePosition(portionValue); newPosition.y = 1.5f;
      var tangent = gs.splineContainer.EvaluateTangent(portionValue);
      var faceDirection = tangent;

      if (rb) { rb.MovePosition(newPosition); if (bPerserveMomentum) { rb.linearVelocity = math.normalize(tangent) * speed; } }
      else { transform.position = newPosition; }

      transform.rotation = Quaternion.LookRotation(faceDirection);

      if(portionValue >= .99f) Destroy(this.gameObject);
   }
}