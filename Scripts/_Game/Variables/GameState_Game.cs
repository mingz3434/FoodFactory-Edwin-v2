using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;


[AddComponentMenu("Variables/Game State Game")]
public class GameState_Game : GameState{
   [Serializable] public struct Assets { public AudioClip game_BGM; }
   [Serializable] public struct Prefabs { public Food chickenRaw_Prefab; public Food potatoRaw_Prefab; public TrajectoryLine trajectoryLine_Prefab; public Hook hook_Prefab; }

   // 0: Assetssss
   public Assets assets; public Prefabs prefabs;
   
   // 1: Transformssss
   public Vector3 splineCenter;
   public Transform mapTransform;
   public Transform canvasTransform;

   // 2: Spline Container
   public SplineContainer splineContainer;

   // 3: BGM Player
   public AudioSource bgmPlayer;

   // 4: Machines in Scene
   public GameObject mixer;
   public GameObject fryer;
   public GameObject cutter;
   public GameObject conveyorController;
   public GameObject foodSpawner;
   

   void Awake(){ _.gs = this; }

   public void StartGame(){
      this.bgmPlayer.clip = this.assets.game_BGM;
      this.bgmPlayer.volume = 0.3f;
      this.bgmPlayer.Play();

   }


   

}