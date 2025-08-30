using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;


[AddComponentMenu("Variables/Game State Game")]
public class GameState_Game : GameState{
   [Serializable] public struct Assets { public AudioClip game_BGM; public Sprite chickenRaw_Sprite, chickenStirred_Sprite, chickenSliced_Sprite, chickenFried_Sprite, nugget_Sprite, chickenFailed_Sprite; public Sprite potatoRaw_Sprite, potatoSliced_Sprite, potatoFried_Sprite, potatoCompleted_Sprite, potatoFailed_Sprite; }
   [Serializable] public struct Prefabs { public FoodTray foodTray_Prefab; public TrajectoryLine trajectoryLine_Prefab; public Hook hook_Prefab; public Food food_Prefab; public ConveyorBeltSegment conveyorBeltSegment_Prefab; public FoodSpawner foodSpawner_Prefab; public Order order_Prefab; }
   [Serializable] public struct ConveyorSettings { public float segmentLength, width; }
   [Serializable] public struct Transforms { public Transform mapTransform, canvasTransform, conveyorBeltContainerTransform, foodTrayOnBeltContainerTransform, orderTransform; }
   [Serializable] public struct InGameInfo { public int remainingTime, score; public int totalOrdersRequired_GR, remainingOrders_Int_GR; public List<Order> pendingOrders; public int latestOrderId; } // GR for Game Round.

   [ReadOnly] public PlayerController_Game localPC;
   public SplineContainer splineContainer;
   [ReadOnly] public Vector3 splineCenter;

   public Assets assets; public Prefabs prefabs;
   public ConveyorSettings conveyorSettings = new ConveyorSettings() { segmentLength = 1, width = 1 };
   public Transforms transforms;
   public InGameInfo inGameInfo  = new InGameInfo() { remainingTime = 300 };
   public AudioSource bgmPlayer;

   void Awake(){ _.gs = this; }

   void Start(){
      PlayBGM();
      StartTimer();
      GenerateConveyors();
      RegularSpawnFood();
      // RegularAddNewOrder();
      AddPendingOrder(1, new Dictionary<Food, int>() { { Food.Create_NonActing_Food(this.transforms.orderTransform, Food.RawFood.Chicken, "Raw Chicken"), 2 } });
   }

   void PlayBGM(){
      this.bgmPlayer.clip = this.assets.game_BGM;
      this.bgmPlayer.volume = 0.3f;
      this.bgmPlayer.Play();
   }

   void StartTimer(){
      Timer.CreateLoopingTimer_NoPhysics(this.gameObject, 1f, ()=>this.inGameInfo.remainingTime-=1, this.inGameInfo.remainingTime<0 );
   }

   void GenerateConveyors(){
      var spline = this.splineContainer.Spline;
      var segmentCount = Mathf.CeilToInt(spline.GetLength() / this.conveyorSettings.segmentLength);
      for (int i = 0; i < segmentCount; i++){
         var portionValue = i * this.conveyorSettings.segmentLength / spline.GetLength();
         var position = spline.EvaluatePosition(portionValue); position.y += .5f; var tangent = spline.EvaluateTangent(portionValue); var up = spline.EvaluateUpVector(portionValue);
         var rotation = Quaternion.LookRotation(tangent, up);
         var trackRotation = rotation * Quaternion.Euler(90, 0, 0);

         if(i==0){
            var foodSpawner = FoodSpawner.CreateFoodSpawner(this.prefabs.foodSpawner_Prefab, this.transforms.conveyorBeltContainerTransform, position, rotation);
         }
         else{
            var conveyorBeltSegment = ConveyorBeltSegment.CreateConveyorBeltSegment(this.prefabs.conveyorBeltSegment_Prefab, this.transforms.conveyorBeltContainerTransform, position, trackRotation, i-1);
         }

      }
   }
   
   void RegularSpawnFood(){
      Timer.CreateTimer_Physics(this.gameObject, 1.5f, () => {
         var foodTray = FoodTray.CreateFoodTray(this.prefabs.foodTray_Prefab, this.transforms.foodTrayOnBeltContainerTransform);
         RegularSpawnFood();
      });
   }

   // ! Pending orders cap at three.
   public void AddPendingOrder(int orderId, Dictionary<Food, int> foods){
      var order = Order.CreateEmptyOrder(this.prefabs.order_Prefab, this.transforms.orderTransform, orderId);
      foreach(var kvp in foods){ order.AddSuborder(kvp.Key, kvp.Value); }
      this.inGameInfo.pendingOrders.Add(order);
      var hud = (_.pc as PlayerController_Game).hud_Inst;
      var orderUI = Order_UI.CreateEmptyOrderUI(hud.order_UI_Prefab, hud.ordersContainer.transform).UI_Set_UI_ByData(order);
   }

   public void RegularAddNewOrder(){
      if(this.inGameInfo.pendingOrders.Count < 3){
         var newId = this.inGameInfo.latestOrderId + 1;
         var howManyKinds = UnityEngine.Random.Range(1, 5);

         var foods = new Dictionary<Food, int>();
         for(int i = 0; i < howManyKinds; i++){
            var food = GetRandomFood();
            var howMany = UnityEngine.Random.Range(2, 13);
            foods.Add(food, howMany);
         }

         this.AddPendingOrder(newId, foods);

      }
   }

   public Food GetRandomFood(){
      System.Random r = new System.Random();
      int result = r.Next(0,1);
      switch(result){
         case 0: return Food.Create_NonActing_Food(this.transforms.orderTransform, Food.RawFood.Chicken, "Fried Chicken");
         // case 1: return "Pizza";
         // case 2: return "Pasta";
         // case 3: return "Sushi";
         // case 4: return "Pancakes";
         // case 5: return "Waffles";
         // case 6: return "Tacos";
         // case 7: return "Pasta";
         // case 8: return "Sushi";
         // case 9: return "Pancakes";
         default: return Food.Create_NonActing_Food(this.transforms.orderTransform, 0, "Fried Chicken");
      }
   }
}