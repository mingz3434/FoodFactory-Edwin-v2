using System;
using UnityEngine;
using Mirror;
public class TestPC : NetworkBehaviour{

   public TestCharacter character;
   public TestPS ps;
   public TestPC_RPCM rpcm;

   void Start(){
      character.transform.SetParent(this.transform.parent);
      this.transform.SetParent(null);
      rpcm.character = this.character;
      rpcm.ps = this.ps;
   }

   void Update(){
      if (isLocalPlayer){
         float horizontalInput = Input.GetAxis("Horizontal");
         float verticalInput = Input.GetAxis("Vertical");
         
         // 計算移動向量
         Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized * Time.deltaTime * 2;
         
         // 更新角色位置
         character.Move(movement);
         
         // 將新位置發送到伺服器
         if (movement != Vector3.zero)
         {
               rpcm.UpdatePosition_ServerRPC(character.transform.position);
         }
      }
   }


}

