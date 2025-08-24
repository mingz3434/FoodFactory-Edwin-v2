using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Game/Recipe")]
public class RecipeSO : ScriptableObject
{
    public FoodType inputFood;      // 原材料類型
    public MachineType requiredMachine; // 所需機器
    public FoodType outputFood;     // 輸出類型
    public float duration = 5f;     // 製作時間 (秒)
}

public enum MachineType
{
    Cutter,
    Mixer,
    Fryer
}