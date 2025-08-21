using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;


[AddComponentMenu("Variables/Game State Game")]
public class GameState_Game : GameState{
   [Serializable] public struct Assets { public AudioClip game_BGM; public Sprite chickenRaw_Sprite; public Sprite potatoRaw_Sprite; public Mesh conveyorSource_Mesh; public Mesh conveyorSegment_Mesh; }
   [Serializable] public struct Prefabs { public FoodTray foodTray_Prefab; public TrajectoryLine trajectoryLine_Prefab; public Hook hook_Prefab; public Food food_Prefab; public ConveyorSegment conveyorSegment_Prefab; }
   [Serializable] public struct ConveyorSettings { public float segmentLength, width; }


   // 2: Spline Container
   public SplineContainer splineContainer;
   [ReadOnly]public Vector3 splineCenter;

   // 0: Assetssss
   public Assets assets; public Prefabs prefabs;
   public ConveyorSettings conveyors = new ConveyorSettings() { segmentLength = 1, width = 1 };

   // 1: Transformssss
   
   public Transform mapTransform;
   public Transform canvasTransform;



   // 3: BGM Player
   public AudioSource bgmPlayer;

   // 4: Machines in Scene
   public GameObject mixer;
   public GameObject fryer;
   public GameObject cutter;
   public GameObject conveyorController;
   public GameObject foodSpawner;
   

   void Awake(){ _.gs = this; }


   void Start(){
      GenerateConveyors();
      FoodTray.CreateFoodTray(this.prefabs.foodTray_Prefab, this.mapTransform);
   }
   public void StartGame(){
      this.bgmPlayer.clip = this.assets.game_BGM;
      this.bgmPlayer.volume = 0.3f;
      this.bgmPlayer.Play();

   }

   void GenerateConveyors(){
      var spline = this.splineContainer.Spline;
      var segmentCount = Mathf.CeilToInt(spline.GetLength() / conveyors.segmentLength);
      for (int i = 0; i < segmentCount; i++){
         var portionValue = i * conveyors.segmentLength / spline.GetLength();
         var position = spline.EvaluatePosition(portionValue); position.y += .5f; var tangent = spline.EvaluateTangent(portionValue); var up = spline.EvaluateUpVector(portionValue);
         
         var cs = ConveyorSegment.CreateConveyorSegment(this.prefabs.conveyorSegment_Prefab, this.mapTransform, i);
         cs.transform.position = position;
         cs.transform.rotation = Quaternion.LookRotation(tangent, up);

         if(i==0){
            cs.transform.localScale = new Vector3(1,2,1);
            var filter = cs.GetComponent<MeshFilter>();
            filter.mesh = this.assets.conveyorSource_Mesh;
         }
         else{
            cs.transform.localScale = new Vector3(1,1,1);
            cs.transform.Rotate(90,0,0,Space.Self);
            var filter = cs.GetComponent<MeshFilter>();
            filter.mesh = this.assets.conveyorSegment_Mesh;
         }

      }
   }
   

}