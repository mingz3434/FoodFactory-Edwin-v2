using UnityEngine;
using Mirror;

public class TestCharacter : NetworkBehaviour{



   public void Move(Vector3 movement){
      transform.position += movement*3f;
   }
}