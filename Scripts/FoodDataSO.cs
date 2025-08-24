using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Game/FoodData")]
public class FoodDataSO : ScriptableObject
{
    [System.Serializable]
    public struct FoodSprite
    {
        public FoodType foodType;
        public Sprite sprite; // 與 RecipeSO.inputSprite 或 outputSprite 一致
    }

    public FoodSprite[] foodSprites;

    public Sprite GetSpriteForFoodType(FoodType type)
    {
        foreach (var foodSprite in foodSprites)
        {
            if (foodSprite.foodType == type)
            {
                return foodSprite.sprite;
            }
        }
        Debug.LogWarning($"No sprite found for FoodType: {type}");
        return null;
    }
}