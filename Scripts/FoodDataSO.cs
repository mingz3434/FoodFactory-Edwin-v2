using UnityEngine;

[CreateAssetMenu(fileName = "FoodData", menuName = "Game/FoodData")]
public class FoodDataSO : ScriptableObject
{
    public FoodType type;
    public Sprite initialSprite; // 初始 Sprite，與 RecipeSO.inputSprite 一致
}