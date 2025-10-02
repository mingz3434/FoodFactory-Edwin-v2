using UnityEngine;
using _ = GameInstance;
public class Hook : MonoBehaviour{

   [ReadOnly] public GameState_Game gs;
   [ReadOnly] public Rigidbody rb;

   void Awake(){
      gs = _.gs as GameState_Game;
      rb = this.GetComponent<Rigidbody>();
   }

   void OnCollisionEnter(Collision collision){
      
      var go = collision.gameObject;
      if(!go.CompareTag("Food")) return;
      if(!_.localPlayer.iv.bHookDragging) return;
      Debug.Log("Hook: The collision hit object is a Food.");
      OnFoodTrayHit(go.GetComponent<FoodTray>());
   }

   void OnFoodTrayHit(FoodTray tray){
      Debug.Log("Hook: The collision hit object is a FoodTray.");

      tray.transform.SetParent(_.localPlayer.extras.foodTraySlotTransform);
      tray.transform.localPosition = Vector3.zero;
      tray.RB_ResetStatic();
      tray.Set_NoMoreInTrack();

      this.ReattachHookContainer_ResetTransform(_.localPlayer.extras.hookContainerTransform);
      this.RB_ResetStatic();

      _.localPlayer.SetStatus_Recastable();
   }

   public void ReattachHookContainer_ResetTransform(Transform containerTransform){
      this.transform.SetParent(containerTransform);
      this.transform.localPosition = Vector3.zero;
      this.transform.localRotation = Quaternion.identity;      
   }

   public void RB_ResetStatic(){
      var rb = this.GetComponent<Rigidbody>();
      rb.isKinematic = true;
      rb.useGravity = false;
   }

   public void RB_Activate(){
      var rb = this.GetComponent<Rigidbody>();
      rb.isKinematic = false;
      rb.useGravity = true;
   }


}