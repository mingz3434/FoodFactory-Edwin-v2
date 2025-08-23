using UnityEngine;
using _ = GameInstance;
public class Hook : Actor_Game{

   [ReadOnly] public GameState_Game gs;
   [ReadOnly] public PlayerController_Game pc;
   [ReadOnly] public PlayerCharacter_Game pChar;

   public static Hook CreateHook(Hook prefab, Transform parentTransform){
      var hook = Instantiate(prefab, parentTransform);
      hook.gameObject.name = prefab.gameObject.name;
      return hook;
   }

   void Awake(){
      gs = _.gs as GameState_Game;
      pc = _.pc as PlayerController_Game;
      pChar = _.pChar_Game;
   }

   void OnCollisionEnter(Collision collision){
      var go = collision.gameObject;
      if(!go.CompareTag("Food")) return;
      Debug.Log("Hook: The collision hit object is a Food.");

      var foodTray = go.GetComponent<FoodTray>();
      if(!foodTray) { Debug.Log("Hook: The collision hit object is not a FoodTray."); return; }
      Debug.Log("Hook: The collision hit object is a FoodTray.");

      foodTray.transform.SetParent(pChar.slotTransform);
      foodTray.gameObject.transform.localPosition = Vector3.zero;
      foodTray.rb.isKinematic = true;
      foodTray.rb.useGravity = false;

      this.ReattachHookContainer_ResetTransform(pChar.hookContainerTransform);
      this.ResetRigidbody();
      pc.status.bProjectileRecastLocked = false;
   }

   public void ReattachHookContainer_ResetTransform(Transform containerTransform){
      this.transform.SetParent(containerTransform);
      this.transform.localPosition = Vector3.zero;
      this.transform.localRotation = Quaternion.identity;      
   }

   public void ResetRigidbody(){
      var rb = this.GetComponent<Rigidbody>();
      rb.isKinematic = true;
      rb.useGravity = false;
   }
}