using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;
using Unity.Mathematics;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class FoodTray : Actor_Game {

   public Food food;

   public GameObject canvas_GO;
   public Slider slider;
   public TMP_Text exclamationMark_Text;
   public float portionValue;
   public float speed = 10f;
   public bool bPerserveMomentum = true;
   public float yOffset = 0.5f; // 食物在輸送帶上方的偏移
   public Rigidbody rb;
   [SyncVar (hook="OnInTrackChanged")] public bool bInTrack = true;
   public void OnInTrackChanged(bool oldValue, bool newValue){
      if(newValue == false){
         RB_ResetStatic();
         T_ResetPositionRotation();
      }
   }

   public static FoodTray CreateFoodTray(FoodTray prefab, Transform parentTransform){
      
      var tray = Instantiate(prefab, parentTransform);

      var sc = (_.gs as GameState_Game).splineContainer;
      tray.transform.position = sc.EvaluatePosition(0f);
      tray.transform.Translate(new Vector3(0,1f,0));

      var tangent = sc.EvaluateTangent(0f);
      tray.transform.rotation = Quaternion.LookRotation(tangent);

      NetworkServer.Spawn(tray.gameObject);

      System.Random r = new System.Random();
      int n = r.Next(0,2);
      tray.food.rawFood = (Food.RawFood)n;

      var suffix = n == 0 ? "Chicken" : "Potato";
      tray.gameObject.name = "FoodTray : " + suffix;

      return tray;
   }

   void Start(){
      rb = this.GetComponent<Rigidbody>();
   }

   void FixedUpdate(){
      if ((_.gs as GameState_Game).splineContainer == null || !bInTrack) return;
      portionValue += .06f * Time.fixedDeltaTime;

      var newPosition = (_.gs as GameState_Game).splineContainer.EvaluatePosition(portionValue); newPosition.y = 1.5f;
      var tangent = (_.gs as GameState_Game).splineContainer.EvaluateTangent(portionValue);
      var faceDirection = tangent;

      if (rb) { rb.MovePosition(newPosition); if (bPerserveMomentum) { rb.linearVelocity = math.normalize(tangent) * speed; } }
      else { transform.position = newPosition; }

      transform.rotation = Quaternion.LookRotation(faceDirection);

      if(portionValue >= .99f) Destroy(this.gameObject);
   }

   void Update(){
      void alwaysFacePlayer(){
         if(!canvas_GO.activeSelf) return;
         var pCharPosition = _.localCharacter.transform.position;
         var pCharPosition_InSameHeight = new Vector3(pCharPosition.x, canvas_GO.transform.position.y, pCharPosition.z);

         // calculate direction of look at player horizontally
         var direction = pCharPosition_InSameHeight - canvas_GO.transform.position;
         if(direction.magnitude == 0f) return;

         // flip
         var targetRotationInEuler = (Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0)).eulerAngles;
         canvas_GO.transform.rotation = Quaternion.Euler(0, targetRotationInEuler.y, 0);
      }

      alwaysFacePlayer();

   }

   void OnCollisionEnter(Collision collision){
      var go = collision.gameObject;
      var machine = go.GetComponent<Machine>();
      if (machine is Mixer mixer) { OnCollidedWith_Mixer(mixer); };
      if (machine is Fryer fryer) { OnCollidedWith_Fryer(fryer); }
      if (machine is Cutter cutter) { OnCollidedWith_Cutter(cutter); }
   }

   //! Comparators...?

   void OnCollidedWith_Mixer(Mixer mixer){
      var slot = mixer.slot;
      if (slot.transform.childCount == 0) {
         Debug.Log("FoodTray: Slot available, attaching the food tray to the mixer...");
         FM_Comparators.Compare(this.food, mixer);
      }
      else { Debug.Log("FoodTray: Slot used!"); }
   }

   void OnCollidedWith_Fryer(Fryer fryer){
      var slots = fryer.slots;
      bool bAvailable = false;
      int availableSlotId = -1;
      for (int i = 0; i < slots.Length; i++) {
         if (slots[i].transform.childCount == 0) {
            bAvailable = true;
            availableSlotId = i;
            break;
         }
      }
      if (bAvailable) {
         Debug.Log("FoodTray: Slot available, attaching the food tray to the fryer...");
         FM_Comparators.Compare(this.food, fryer, availableSlotId);
      }
   }

   void OnCollidedWith_Cutter(Cutter cutter){
      var slot = cutter.slot;
      if (slot.transform.childCount == 0) {
         Debug.Log("FoodTray: Slot available, attaching the food tray to the cutter...");
         FM_Comparators.Compare(this.food, cutter);
      }
   }

   public void SnapTo(Transform newParentTransform){
      transform.parent = newParentTransform;
      T_ResetPositionRotation();
      RB_ResetStatic();
   }

   public void BounceBack(){
      // this.SnapTo(_.localPlayer.extras.foodTraySlotTransform);
   }

   public void RB_Activate(){
      rb.isKinematic = false;
      rb.useGravity = true;
   }

   public void RB_ResetStatic(){
      rb.isKinematic = true;
      rb.useGravity = false;
   }

   public void Set_NoMoreInTrack(){
      if (this.bInTrack) this.bInTrack = false;
   }

   public void T_ResetPositionRotation(){
      transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
   }
}