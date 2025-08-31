using UnityEngine;
using Mirror;

public class PlayerState_Game : NetworkBehaviour{
   [SyncVar(hook = "OnChangeHp")] public int hp;

   public void OnChangeHp(int oldHp, int newHp){
      Debug.Log($"Hp changed from {oldHp} to {newHp}");
   }
}