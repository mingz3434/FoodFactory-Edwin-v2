using System;
using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;

using Unity.Mathematics;

[AddComponentMenu("Characters/Player Character Game")]
public class PlayerCharacter_Game : Character_Game{

   public GameState_Game gs;
   public float speed = 10f; public float mass = 1f;
   Rigidbody rigidbody; public Rigidbody GetRigidbody() { return rigidbody; }
   public float portionValue = 0f; // 當前在 Spline 上的位置 (0-1)
   public Transform slotTransform, hookContainerTransform; // 用於存放食物的容器

   



   void Awake() { _.pChar_Game = this;}


}

