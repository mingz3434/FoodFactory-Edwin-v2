using System;
using UnityEngine;
using _ = GameInstance;

[AddComponentMenu("Characters/Player Character Game")]
public class PlayerCharacter_Game : Character_Game{

   public int speed = 350; public int GetSpeed(){ return speed; }
   public Rigidbody2D GetRigidbody2D(){ return GetComponent<Rigidbody2D>(); }

   void Awake() { _.pChar_Game = this; }
   //* Native


}



