using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _ = GameInstance;


[AddComponentMenu("Variables/Game State Game")]
public class GameState_Game : GameState{
   [Serializable] public struct Assets { public AudioClip game_BGM; }
   [Serializable] public struct Prefabs { }

   public Assets assets; public Prefabs prefabs;
   public Transform canvasTransform; public AudioSource bgmPlayer;
   public Transform mapTransform;

   void Awake(){ _.gs = this; }

   public void StartGame(){
      this.bgmPlayer.clip = this.assets.game_BGM;
      this.bgmPlayer.volume = 0.3f;
      this.bgmPlayer.Play();

   }


   

}