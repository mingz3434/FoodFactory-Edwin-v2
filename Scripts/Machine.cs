using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Machine : Actor_Game {
   public enum MachineType { Cutter, Mixer, Fryer }
   public MachineType type;
   public RecipeSO[] recipes;
   public int maxSlots = 3;
   public Transform[] slotPositions; // 每個槽位有唯一位置

   private List<Coroutine> craftingCoroutines = new List<Coroutine>();
   public bool needsInput;
   private float inputWindow = 3f;

   void OnCollisionEnter(Collision collision) {
      if (collision.gameObject.CompareTag("Food")) {
         // 檢查是否有空閒槽位
         int slotIndex = -1;
         for (int i = 0; i < slotPositions.Length; i++) {
            if (slotPositions[i].childCount == 0) {
               slotIndex = i;
               break;
            }
         }
         if (slotIndex == -1 || slotIndex >= maxSlots) {
            Debug.Log("No available slot in machine!");
            return;
         }

         Food food = collision.gameObject.GetComponent<Food>();
         if (food != null) {
            PlaceFood(food, slotIndex);
         }
      }
   }

   void PlaceFood(Food food, int slotIndex) {
      food.transform.SetParent(slotPositions[slotIndex]);
      food.transform.localPosition = Vector3.zero;
      Rigidbody rb = food.GetComponent<Rigidbody>();
      if (rb != null) {
         rb.isKinematic = true;
         rb.useGravity = false;
      }
      food.transform.rotation = Quaternion.identity; // 靜止時面向上

      bool matched = false;
      foreach (RecipeSO recipe in recipes) {
         if (food.type == recipe.inputFood && type == recipe.requiredMachine) {
            StartCrafting(food, recipe, slotIndex);
            matched = true;
            break;
         }
      }
      if (!matched) {
         Debug.Log("No matching recipe. Waiting for player to retrieve.");
      }
   }

   void StartCrafting(Food food, RecipeSO recipe, int slotIndex) {
      food.UpdateSprite(recipe.processingSprite);
      craftingCoroutines.Add(StartCoroutine(Craft(food, recipe, slotIndex)));
   }

   IEnumerator Craft(Food food, RecipeSO recipe, int slotIndex) {
      float elapsed = 0f;
      while (elapsed < recipe.duration) {
         elapsed += Time.deltaTime;
         food.UpdateProgress(elapsed / recipe.duration);
         yield return null;
      }

      food.HideProgress();

      if (type == MachineType.Mixer) {
         food.type = recipe.outputFood;
         food.UpdateSprite(recipe.outputSprite);

         needsInput = true;
         float timer = 0f;
         while (timer < inputWindow) {
            timer += Time.deltaTime;
            yield return null;
         }
         if (needsInput) {
            Destroy(food.gameObject);
            Debug.Log("Mixer: Food destroyed.");
            RemoveFood(slotIndex);
            yield break;
         }
      }
      else {
         food.type = recipe.outputFood;
         food.UpdateSprite(recipe.outputSprite);
      }

      food.transform.SetParent(null);
      Rigidbody rb = food.GetComponent<Rigidbody>();
      if (rb != null) {
         rb.isKinematic = false;
         rb.useGravity = true;
      }
      food.transform.rotation = Quaternion.identity; // 完成時面向上
      RemoveFood(slotIndex);
   }

   public void StopMixer() {
      if (type == MachineType.Mixer && needsInput) {
         needsInput = false;
         Debug.Log("Mixer stopped.");
      }
   }

   public void RemoveFood(int slotIndex) {
      if (slotIndex < craftingCoroutines.Count) {
         if (craftingCoroutines[slotIndex] != null) {
            StopCoroutine(craftingCoroutines[slotIndex]);
         }
         craftingCoroutines.RemoveAt(slotIndex);
      }
   }

   public Food GetFoodAtSlot(int slotIndex) {
      if (slotIndex < slotPositions.Length && slotPositions[slotIndex].childCount > 0) {
         return slotPositions[slotIndex].GetChild(0).GetComponent<Food>();
      }
      return null;
   }
}

public class RecipeSO : ScriptableObject{
   public FoodType inputFood; public Sprite inputSprite;
   public Machine.MachineType requiredMachine;
   public Sprite processingSprite;
   public Sprite outputSprite;
   public FoodType outputFood;
   public float duration = 5f;
}