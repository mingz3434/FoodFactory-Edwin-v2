using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Order", menuName = "Order/OrderSO")]
public class OrderSO : ScriptableObject
{
    public string orderName; // 訂單名稱，例如 "訂單 1"
    [Serializable]
    public struct FoodRequirement
    {
        public FoodType foodType; // 食物種類
        public int quantity; // 數量
    }
    public FoodRequirement[] requirements; // 食物需求陣列
}