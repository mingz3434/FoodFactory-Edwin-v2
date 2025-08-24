using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class DeliveryPlate : MonoBehaviour
{
    public float stackOffset = 0.5f;
    public List<GameObject> stackedFoods = new List<GameObject>();
    //private int maxStack = 5;
    public TMP_Text orderNameText; // 出餐口的 TMP_Text
    public OrderSO order; // 對應的訂單
    private int orderIndex; // 訂單索引
    private OrderManager orderManager;

    public void Initialize(OrderSO order, int orderIndex)
    {
        this.order = order;
        this.orderIndex = orderIndex;
        orderManager = FindFirstObjectByType<OrderManager>();
        if (orderNameText != null)
        {
            orderNameText.text = order.orderName;
            Debug.Log($"Set DeliveryPlate TMP_Text to: 訂單 {order.orderName}");
        }
        else
        {
            Debug.LogWarning($"orderNameText is null for DeliveryPlate of Order {order.orderName}");
        }
    }

    public void UpdateOrderIndex(int newIndex)
    {
        orderIndex = newIndex;
        Debug.Log($"DeliveryPlate for Order {order.orderName} updated orderIndex to {newIndex}");
    }

    void Update()
    {
        if (transform.position.y < -5f && transform.parent == null)
        {
            foreach (GameObject food in stackedFoods)
            {
                Destroy(food);
            }
            stackedFoods.Clear();
            orderManager.RecyclePlate(gameObject, order);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject collided = collision.gameObject;
        if (collided.CompareTag("Food"))
        {
            if (stackedFoods.Contains(collided))
            {
                return;
            }

            DeliveryPlate parentPlate = collided.GetComponentInParent<DeliveryPlate>();
            if (parentPlate == this)
            {
                return;
            }

            StackFood(collided);
        }
    }

    public void StackFood(GameObject food)
    {
        Rigidbody rb = food.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        food.transform.SetParent(transform, false);
        food.transform.localPosition = new Vector3(0, (stackedFoods.Count+1) * stackOffset, 0);
        stackedFoods.Add(food);

        int requiredCount = 0;
        foreach (var req in order.requirements)
        {
            requiredCount += req.quantity;
        }
        if (stackedFoods.Count >= requiredCount)
        {
            CalculateScore();
        }
    }

    void CalculateScore()
    {
        int score = stackedFoods.Count * 10;
        Dictionary<FoodType, int> requiredFoods = new Dictionary<FoodType, int>();
        foreach (var req in order.requirements)
        {
            requiredFoods[req.foodType] = req.quantity;
        }

        foreach (GameObject foodObj in stackedFoods)
        {
            Food food = foodObj.GetComponent<Food>();
            if (food != null && requiredFoods.ContainsKey(food.type) && requiredFoods[food.type] > 0)
            {
                requiredFoods[food.type]--;
            }
            else
            {
                score -= 10;
            }
        }

        foreach (var req in requiredFoods)
        {
            if (req.Value > 0)
            {
                score -= req.Value * 10;
            }
        }

        orderManager.CompleteOrder(orderIndex, score);
        Debug.Log($"Order {order.orderName} scored: {score}");
    }

    public void RemoveFood(GameObject food)
    {
        if (stackedFoods.Contains(food))
        {
            stackedFoods.Remove(food);
            food.transform.SetParent(null,false);
            Rigidbody rb = food.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            for (int i = 0; i < stackedFoods.Count; i++)
            {
                stackedFoods[i].transform.localPosition = new Vector3(0, i * stackOffset, 0);
            }
            Debug.Log("Food removed from stack!");
        }
    }

    void GameOver()
    {
        Time.timeScale = 0;
        Debug.Log("Game Over: Stack complete!");
    }
}