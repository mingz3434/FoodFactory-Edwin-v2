using UnityEngine;
using UnityEngine.Splines;
using _ = GameInstance;
using Unity.Mathematics;
using UnityEngine.UI;
using TMPro;

public class FoodTray : Actor_Game {
   [ReadOnly] public GameState_Game gs;
   [ReadOnly] public Food food;

   public GameObject canvas_GO;
   public Slider slider;
   public TMP_Text exclamationMark_Text;
   public float portionValue;
   public float speed = 5f;
   public bool bPerserveMomentum = true;
   public float yOffset = 0.5f; // 食物在輸送帶上方的偏移
   public Rigidbody rb;
   public bool bInTrack = true;

   public static FoodTray CreateFoodTray(FoodTray prefab, Transform parentTransform){
      var tray = Instantiate(prefab, parentTransform);
      tray.gs = _.gs as GameState_Game;
      var sc = tray.gs.splineContainer;
      tray.transform.position = sc.EvaluatePosition(0f);
      tray.transform.Translate(new Vector3(0,1f,0));
      
      System.Random r = new System.Random();
      int n = r.Next(0,2);
      tray.food = Food.CreateFood(tray, tray.gs.prefabs.food_Prefab, tray.transform, (Food.RawFood)n );

      var suffix = n == 0 ? "Chicken" : "Potato";
      tray.gameObject.name = "FoodTray : " + suffix;
      return tray;
   }

   void Start(){
      rb = this.GetComponent<Rigidbody>();
   }

   void FixedUpdate(){
      if (gs.splineContainer == null || !bInTrack) return;
      portionValue += .03f * Time.fixedDeltaTime;

      var newPosition = gs.splineContainer.EvaluatePosition(portionValue); newPosition.y = 1.5f;
      var tangent = gs.splineContainer.EvaluateTangent(portionValue);
      var faceDirection = tangent;

      if (rb) { rb.MovePosition(newPosition); if (bPerserveMomentum) { rb.linearVelocity = math.normalize(tangent) * speed; } }
      else { transform.position = newPosition; }

      transform.rotation = Quaternion.LookRotation(faceDirection);

      if(portionValue >= .99f) Destroy(this.gameObject);
   }

   void Update(){
      if(!canvas_GO.activeSelf) return;
      var pCharPosition = _.pChar_Game.transform.position;
      var pCharPosition_SameHeight = new Vector3(pCharPosition.x, canvas_GO.transform.position.y, pCharPosition.z);
      var direction = pCharPosition_SameHeight - canvas_GO.transform.position;
      if(direction.magnitude == 0f) return;

      var targetRotationInEuler = (Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180, 0)).eulerAngles;
      canvas_GO.transform.rotation = Quaternion.Euler(0, targetRotationInEuler.y, 0);
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

   public void SnapTo(Transform newParentTransform, bool bResetRotation = true){
      transform.parent = newParentTransform;
      transform.localPosition = Vector3.zero;
      if(bResetRotation) transform.localRotation = Quaternion.identity;
      rb.isKinematic = true;
      rb.useGravity = false;
   }

   public void BounceBack(){
      this.SnapTo(_.pChar_Game.slotTransform);
   }
}