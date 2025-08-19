using System;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;

using Unity.Mathematics;

[AddComponentMenu("Characters/Player Character Game")]
public class PlayerCharacter_Game : Character_Game{

   public GameState_Game gs;
   public float speed = 10f; public float mass = 1f;
   public SplineContainer splineContainer; public SplineContainer GetSplineContainer() { var gs = _.gs as GameState_Game; return gs.splineContainer; }
   Rigidbody rigidbody; public Rigidbody GetRigidbody() { return rigidbody; }
   public float portionValue = 0f; // 當前在 Spline 上的位置 (0-1)
   public Transform slotTransform, hookContainerTransform; // 用於存放食物的容器
   public Vector3 splineCenter; // 閉環的中心點

   public TrajectoryLine trajectoryLine;
   public Hook hook;


   void Awake() { _.pChar_Game = this; }

   void Start(){
      gs = _.gs as GameState_Game;

      Action setPlayerStartingPos = () => {
         var spline = gs.splineContainer.Spline;
         this.transform.position = spline.EvaluatePosition(0);
      };

      Action calculateMapCenter = () => {
         if (!gs.splineContainer) return;

         int samples = 100;
         Vector3 sum = Vector3.zero;
         for (int i = 0; i < samples; i++){
            float t = i / (float)samples;
            sum += (Vector3)gs.splineContainer.EvaluatePosition(t);
         }
         gs.splineCenter = sum / samples;
         gs.splineCenter.y = 3f;
      };

      setPlayerStartingPos();
      calculateMapCenter();

   }
}

