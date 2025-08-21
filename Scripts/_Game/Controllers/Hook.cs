using UnityEngine;
public class Hook : Actor_Game{
   public static Hook CreateHook(Hook prefab, Transform parentTransform){
      return Instantiate(prefab, parentTransform);
   }
   public Transform objectContainer; // 玩家的 ObjectContainer
   private PlayerController_Game playerMovement;

   // void Start()
   // {
   //    playerMovement = GetComponentInParent<PlayerController_Game>();
   //    if (playerMovement == null)
   //    {
   //          playerMovement = FindObjectOfType<PlayerController_Game>();
   //    }
   // }

   // void OnCollisionEnter(Collision collision)
   // {
   //    if (collision.gameObject.CompareTag("Food"))
   //    {
   //       Food food = collision.gameObject.GetComponent<Food>();
   //       if (food != null)
   //       {
   //          Machine machine = null;
   //          if (food.transform.parent != null)
   //          {
   //             machine = food.transform.parent.GetComponentInParent<Machine>();
   //          }

   //          if (machine != null)
   //          {
   //             int slotIndex = -1;
   //             for (int i = 0; i < machine.slotPositions.Length; i++)
   //             {
   //                   if (machine.slotPositions[i] == food.transform.parent)
   //                   {
   //                      slotIndex = i;
   //                      break;
   //                   }
   //             }
   //             if (slotIndex >= 0)
   //             {
   //                   machine.RemoveFood(slotIndex);
   //             }
   //          }

   //          Destroy(food.GetComponent<FoodTray>());
   //          Rigidbody foodRb = food.GetComponent<Rigidbody>();
   //          if (foodRb != null)
   //          {
   //             foodRb.isKinematic = true;
   //             foodRb.useGravity = false;
   //             foodRb.linearVelocity = Vector3.zero;
   //             foodRb.angularVelocity = Vector3.zero;
   //          }
   //          food.transform.SetParent(objectContainer);
   //          food.transform.localPosition = Vector3.zero;
   //          //food.HideProgress();
   //          Debug.Log("Food hooked back to ObjectContainer!");
   //          playerMovement.ResetHook();
   //       }
   //    }
   // }
}