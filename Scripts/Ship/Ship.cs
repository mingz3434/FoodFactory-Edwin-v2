using UnityEngine;
using Mirror;

public class Ship : NetworkBehaviour{

   void Start(){

   }


   void Update(){

      if(isServer){
         Move();
      }
   }

   [Server]
   public void Move(){
      this.transform.Translate(Vector3.forward * Time.deltaTime);
   }
}