using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;


[AddComponentMenu("Variables/Game State Game")]
public class GameState_Game : GameState{
   [Serializable] public struct Assets { public AudioClip game_BGM; public Sprite chickenRaw_Sprite, chickenStirred_Sprite, chickenSliced_Sprite, chickenFried_Sprite, nugget_Sprite, chickenFailed_Sprite; public Sprite potatoRaw_Sprite, potatoSliced_Sprite, potatoFried_Sprite, potatoCompleted_Sprite, potatoFailed_Sprite; }
   [Serializable] public struct Prefabs { public FoodTray foodTray_Prefab; public TrajectoryLine trajectoryLine_Prefab; public Hook hook_Prefab; public Food food_Prefab; public ConveyorBeltSegment conveyorBeltSegment_Prefab; public FoodSpawner foodSpawner_Prefab; }
   [Serializable] public struct ConveyorSettings { public float segmentLength, width; }

   // Spline Container
   public SplineContainer splineContainer;
   [ReadOnly] public Vector3 splineCenter;

   // Assetssss
   public Assets assets; public Prefabs prefabs;
   public ConveyorSettings conveyorSettings = new ConveyorSettings() { segmentLength = 1, width = 1 };

   // Transforms
   public Transform mapTransform;
   public Transform canvasTransform;
   public Transform conveyorBeltContainerTransform;
   public Transform foodTrayOnBeltContainerTransform;

   // BGM Player
   public AudioSource bgmPlayer;


   // Orders
   // public List<Order> orders = new List<Order>();
   public int remainingTime = 300;

   // Machines in Scene
   //! 儘可能唔好use global machine, as they are supposed to handle all their own stuffs by their own.
   // ...
   // ...
   // ...
   

   void Awake(){ _.gs = this; }


   void Start(){
      GenerateConveyors();
      RegularSpawnFood();
   }
   public void StartGame(){
      this.bgmPlayer.clip = this.assets.game_BGM;
      this.bgmPlayer.volume = 0.3f;
      this.bgmPlayer.Play();

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
            var foodSpawner = FoodSpawner.CreateFoodSpawner(this.prefabs.foodSpawner_Prefab, this.conveyorBeltContainerTransform, position, rotation);
         }
         else{
            var conveyorBeltSegment = ConveyorBeltSegment.CreateConveyorBeltSegment(this.prefabs.conveyorBeltSegment_Prefab, this.conveyorBeltContainerTransform, position, trackRotation, i-1);
         }

      }
   }
   
   void RegularSpawnFood(){
      Timer.CreateTimer_Physics(this.gameObject, 1.5f, () => {
         var foodTray = FoodTray.CreateFoodTray(this.prefabs.foodTray_Prefab, this.foodTrayOnBeltContainerTransform);
         RegularSpawnFood();
      });
      
   }
}