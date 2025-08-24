using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Machine : MonoBehaviour
{
    public MachineType type;
    public RecipeSO[] recipes;
    public int maxSlots = 3;
    public Transform[] slotPositions; // 每個槽位有唯一位置
    public FoodDataSO foodData; // 用於查找 Sprite

    private Coroutine[] craftingCoroutines;
    public bool needsInput;
    private float inputWindow = 3f;

    private void Start()
    {
        craftingCoroutines = new Coroutine[slotPositions.Length];
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            int slotIndex = -1;
            for (int i = 0; i < slotPositions.Length; i++)
            {
                if (slotPositions[i].childCount == 0)
                {
                    slotIndex = i;
                    break;
                }
            }
            if (slotIndex == -1 || slotIndex >= maxSlots)
            {
                Debug.Log("No available slot in machine!");
                return;
            }

            Food food = collision.gameObject.GetComponent<Food>();
            if (food != null)
            {
                PlaceFood(food, slotIndex);
            }
        }
    }

    void PlaceFood(Food food, int slotIndex)
    {
        food.transform.SetParent(slotPositions[slotIndex]);
        food.transform.localPosition = Vector3.zero;
        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        food.transform.rotation = Quaternion.identity;

        bool matched = false;
        foreach (RecipeSO recipe in recipes)
        {
            if (food.type == recipe.inputFood && type == recipe.requiredMachine)
            {
                StartCrafting(food, recipe, slotIndex);
                matched = true;
                break;
            }
        }
        if (!matched)
        {
            Debug.Log("No matching recipe. Waiting for player to retrieve.");
        }
    }

    void StartCrafting(Food food, RecipeSO recipe, int slotIndex)
    {
        Sprite processingSprite = foodData.GetSpriteForFoodType(recipe.inputFood); // 使用 inputFood 的 Sprite
        if (processingSprite != null)
        {
            food.UpdateSprite(processingSprite);
        }
        else
        {
            Debug.LogWarning($"No sprite found for {recipe.inputFood} in FoodDataSO.");
        }
        craftingCoroutines[slotIndex] = StartCoroutine(Craft(food, recipe, slotIndex));
    }

    IEnumerator Craft(Food food, RecipeSO recipe, int slotIndex)
    {
        float elapsed = 0f;
        while (elapsed < recipe.duration)
        {
            elapsed += Time.deltaTime;
            food.UpdateProgress(elapsed / recipe.duration);
            yield return null;
        }

        food.HideProgress();

        if (type == MachineType.Mixer)
        {
            food.type = recipe.outputFood;
            Sprite outputSprite = foodData.GetSpriteForFoodType(recipe.outputFood);
            if (outputSprite != null)
            {
                food.UpdateSprite(outputSprite);
            }
            else
            {
                Debug.LogWarning($"No sprite found for {recipe.outputFood} in FoodDataSO.");
            }

            needsInput = true;
            float timer = 0f;
            while (timer < inputWindow)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (needsInput)
            {
                Destroy(food.gameObject);
                Debug.Log("Mixer: Food destroyed.");
                RemoveFood(slotIndex);
                yield break;
            }
        }
        else
        {
            food.type = recipe.outputFood;
            Sprite outputSprite = foodData.GetSpriteForFoodType(recipe.outputFood);
            if (outputSprite != null)
            {
                food.UpdateSprite(outputSprite);
            }
            else
            {
                Debug.LogWarning($"No sprite found for {recipe.outputFood} in FoodDataSO.");
            }
        }

        food.transform.SetParent(null);
        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        food.transform.rotation = Quaternion.identity;
        RemoveFood(slotIndex);
    }

    public void StopMixer()
    {
        if (type == MachineType.Mixer && needsInput)
        {
            needsInput = false;
            Debug.Log("Mixer stopped.");
        }
    }

    public void RemoveFood(int slotIndex)
    {
        if (slotIndex < craftingCoroutines.Length)
        {
            if (craftingCoroutines[slotIndex] != null)
            {
                StopCoroutine(craftingCoroutines[slotIndex]);
            }
            craftingCoroutines[slotIndex] = null;
        }
    }

    public Food GetFoodAtSlot(int slotIndex)
    {
        if (slotIndex < slotPositions.Length && slotPositions[slotIndex].childCount > 0)
        {
            return slotPositions[slotIndex].GetChild(0).GetComponent<Food>();
        }
        return null;
    }
}