using System;
using UnityEngine;

[AddComponentMenu("Game Instance/Game Instance")]
public class GameInstance : MonoBehaviour{

   public static GameInstance gameInstance; public static GameMode gm; public static GameState gs; public static PlayerController pc; public static PlayerState ps; public static PlayerCharacter_Game pChar_Game;

   void Awake(){
      gameInstance = this; DontDestroyOnLoad(this.gameObject);
   }

}






