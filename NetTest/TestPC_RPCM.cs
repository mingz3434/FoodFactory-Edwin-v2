using UnityEngine;
using Mirror;
using CallServer = Mirror.CommandAttribute;
using BroadcastToClients = Mirror.ClientRpcAttribute;

public class TestPC_RPCM : NetworkBehaviour{
   public TestCharacter character;
   public TestPS ps;
   
   [BroadcastToClients]
   public void UpdatePosition_ClientRPC(Vector3 pos)
   {
      if (!isLocalPlayer)
      {
         // 平滑插值非本地玩家的位置
         character.transform.position = Vector3.Lerp(character.transform.position, pos, Time.deltaTime * 10f);
      }
   }

   [CallServer]
   public void UpdatePosition_ServerRPC(Vector3 pos)
   {
      // 更新伺服器端位置
      character.transform.position = pos;
      
      // 同步到所有客戶端
      UpdatePosition_ClientRPC(pos);
   }

}
