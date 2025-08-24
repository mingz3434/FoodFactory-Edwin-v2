using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class OrderManager : MonoBehaviour
{
    public Transform orderList; // OrderList UI（Panel 或 ScrollView Content）
    public GameObject orderPrefab; // Order Prefab
    public GameObject productPrefab; // Product Prefab
    public TMP_Text remainingOrdersText; // 顯示 "剩餘訂單: X"
    public OrderSO[] orders; // Inspector 配置的所有訂單
    public FoodDataSO foodData; // 用於查找 FoodType 的 Sprite
    private List<GameObject> orderInstances = new List<GameObject>(); // 已顯示的 Order UI
    private List<OrderSO> activeOrders = new List<OrderSO>(); // orderInstances 對應的訂單
    private List<OrderSO> pendingOrders = new List<OrderSO>(); // 待處理訂單
    private List<GameObject> availablePlates = new List<GameObject>(); // 場上出餐口
    private Dictionary<string, int> orderOriginalIndices = new Dictionary<string, int>(); // 儲存訂單原始索引
    private Dictionary<OrderSO, GameObject> orderToPlate = new Dictionary<OrderSO, GameObject>(); // 訂單到出餐口的映射
    private GameManager gameManager;
    private PlayerMovement playerMovement;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        //Debug.Log($"Total orders configured: {orders.Length}");
        for (int i = 0; i < orders.Length; i++)
        {
            orderOriginalIndices[orders[i].orderName] = i;
        }
        pendingOrders.AddRange(orders);
        GenerateInitialOrders();
        UpdateRemainingOrders();
        UpdateAvailablePlateUI();
    }

    void GenerateInitialOrders()
    {
        int maxOrders = Mathf.Min(3, orders.Length); // 最多 3 個訂單 UI
        for (int i = 0; i < maxOrders; i++)
        {
            GenerateOrder(orders[i], i);
            activeOrders.Add(orders[i]);
        }
        //Debug.Log($"Generated {orderInstances.Count} initial order UIs, activeOrders: {string.Join(", ", activeOrders.Select(o => o.orderName))}");
    }

    void GenerateOrder(OrderSO order, int orderIndex)
    {
        GameObject orderObj = Instantiate(orderPrefab, orderList);
        TMP_Text orderNameText = orderObj.GetComponentInChildren<TMP_Text>();
        if (orderNameText != null)
        {
            orderNameText.text = "訂單 " + order.orderName;
        }
        else
        {
            Debug.LogWarning($"Order Prefab missing TMP_Text for Order {order.orderName}");
        }

        foreach (OrderSO.FoodRequirement req in order.requirements)
        {
            GameObject productObj = Instantiate(productPrefab, orderObj.transform);
            TMP_Text quantityText = productObj.GetComponentInChildren<TMP_Text>();
            Image foodImage = productObj.GetComponentInChildren<Image>();

            if (quantityText != null)
            {
                quantityText.text = $"[{req.quantity}]x";
            }
            if (foodImage != null && foodData != null)
            {
                Sprite sprite = foodData.GetSpriteForFoodType(req.foodType);
                foodImage.sprite = sprite;
            }
        }

        orderInstances.Add(orderObj);
        //Debug.Log($"Generated Order UI for Order {order.orderName} at index {orderIndex}");
    }

    public bool CanGeneratePlate()
    {
        return availablePlates.Count < 3 && activeOrders.Any(o => !orderToPlate.ContainsKey(o));
    }

    public GameObject GenerateDeliveryPlate()
    {
        if (!CanGeneratePlate())
        {
            Debug.Log("Cannot generate plate: max 3 plates or no available orders in activeOrders.");
            return null;
        }

        // 從 activeOrders 中選擇第一個未分配出餐口的訂單
        OrderSO order = activeOrders.First(o => !orderToPlate.ContainsKey(o));
        int orderIndex = activeOrders.IndexOf(order);
        GameObject plate = Instantiate(playerMovement.deliveryPlatePrefab, playerMovement.objectContainer.position, Quaternion.identity);
        plate.transform.SetParent(playerMovement.objectContainer);
        plate.transform.localPosition = Vector3.zero;
        DeliveryPlate plateScript = plate.GetComponent<DeliveryPlate>();
        if (plateScript != null)
        {
            plateScript.Initialize(order, orderIndex);
        }
        availablePlates.Add(plate);
        orderToPlate[order] = plate;
        UpdateAvailablePlateUI();
        UpdateRemainingOrders();
        //Debug.Log($"Generated DeliveryPlate for Order {order.orderName}, orderIndex: {orderIndex}, activeOrders: {string.Join(", ", activeOrders.Select(o => o.orderName))}");
        return plate;
    }

    public void CompleteOrder(int orderIndex, int score)
    {
        if (orderIndex >= 0 && orderIndex < activeOrders.Count && orderIndex < orderInstances.Count)
        {
            // 移除完成的訂單
            OrderSO completedOrder = activeOrders[orderIndex];
            Destroy(orderInstances[orderIndex]);
            orderInstances.RemoveAt(orderIndex);
            activeOrders.RemoveAt(orderIndex);
            pendingOrders.RemoveAt(0);

            // 移除對應的出餐口
            if (orderToPlate.ContainsKey(completedOrder))
            {
                int plateIndex = availablePlates.IndexOf(orderToPlate[completedOrder]);
                if (plateIndex >= 0)
                {
                    Destroy(availablePlates[plateIndex]);
                    availablePlates.RemoveAt(plateIndex);
                }
                orderToPlate.Remove(completedOrder);
            }

            // 更新所有出餐口的 orderIndex
            foreach (GameObject plate in availablePlates)
            {
                DeliveryPlate plateScript = plate.GetComponent<DeliveryPlate>();
                if (plateScript != null)
                {
                    OrderSO plateOrder = orderToPlate.FirstOrDefault(x => x.Value == plate).Key;
                    if (plateOrder != null)
                    {
                        int newIndex = activeOrders.IndexOf(plateOrder);
                        if (newIndex >= 0)
                        {
                            plateScript.UpdateOrderIndex(newIndex);
                            Debug.Log($"Updated orderIndex for plate {plateOrder.orderName} to {newIndex}");
                        }
                    }
                }
            }

            // 補充新訂單（如果有待處理訂單）
            if (pendingOrders.Count > orderInstances.Count)
            {
                OrderSO nextOrder = pendingOrders[orderInstances.Count];
                GenerateOrder(nextOrder, orderInstances.Count);
                activeOrders.Add(nextOrder);

                //Debug.Log($"Added new Order UI: 訂單 {nextOrder.orderName}");
            }

            gameManager.AddScore(score);
            UpdateRemainingOrders();
            UpdateAvailablePlateUI();
            if (pendingOrders.Count == 0)
            {
                gameManager.WinGame();
            }
            //Debug.Log($"Order {completedOrder.orderName} completed. Score: {score}, Remaining orders: {orderInstances.Count + pendingOrders.Count}, orderInstances: {string.Join(", ", orderInstances.Select(o => o.GetComponentInChildren<TMP_Text>().text))}");
        }
        else
        {
            Debug.LogWarning($"Invalid order index: {orderIndex}, activeOrders: {activeOrders.Count}, orderInstances: {orderInstances.Count}");
        }
    }

    public void RecyclePlate(GameObject plate, OrderSO order)
    {
        int index = availablePlates.IndexOf(plate);
        if (index >= 0)
        {
            availablePlates.RemoveAt(index);
            orderToPlate.Remove(order);
            Destroy(plate);

            // 如果 orderInstances 未滿，重新加入 activeOrders 和 orderInstances
            if (orderInstances.Count < 3)
            {
                GenerateOrder(order, orderInstances.Count);
                activeOrders.Add(order);
                //Debug.Log($"Recycled Order {order.orderName} added to activeOrders");
            }
            else
            {
                // 按原始順序插入 pendingOrders
                int originalIndex = orderOriginalIndices[order.orderName];
                int insertIndex = pendingOrders.FindIndex(o => orderOriginalIndices[o.orderName] > originalIndex);
                if (insertIndex == -1)
                {
                    pendingOrders.Add(order);
                }
                else
                {
                    pendingOrders.Insert(insertIndex, order);
                }
                //Debug.Log($"Recycled Order {order.orderName} added to pendingOrders[{insertIndex}]");
            }

            // 更新所有出餐口的 orderIndex
            foreach (GameObject p in availablePlates)
            {
                DeliveryPlate plateScript = p.GetComponent<DeliveryPlate>();
                if (plateScript != null)
                {
                    OrderSO plateOrder = orderToPlate.FirstOrDefault(x => x.Value == p).Key;
                    if (plateOrder != null)
                    {
                        int newIndex = activeOrders.IndexOf(plateOrder);
                        if (newIndex >= 0)
                        {
                            plateScript.UpdateOrderIndex(newIndex);
                            Debug.Log($"Updated orderIndex for plate {plateOrder.orderName} to {newIndex}");
                        }
                    }
                }
            }

            UpdateAvailablePlateUI();
            UpdateRemainingOrders();
            //Debug.Log($"Plate recycled for Order {order.orderName}, activeOrders: {string.Join(", ", activeOrders.Select(o => o.orderName))}, pendingOrders: {string.Join(", ", pendingOrders.Select(o => o.orderName))}");
        }
    }

    void UpdateRemainingOrders()
    {
        if (remainingOrdersText != null)
        {
            remainingOrdersText.text = "剩餘訂單: " + pendingOrders.Count.ToString();
        }
    }

    void UpdateAvailablePlateUI()
    {
        // 優先檢查 activeOrders 中未分配出餐口的訂單
        OrderSO nextOrder = activeOrders.FirstOrDefault(o => !orderToPlate.ContainsKey(o));
        if (nextOrder != null)
        {
            gameManager.UpdateAvailablePlateUI(nextOrder.orderName);
            //Debug.Log($"AvailablePlateUI set to: 訂單 {nextOrder.orderName} (from activeOrders)");
        }
        else if (pendingOrders.Count > 0 && orderInstances.Count < 3)
        {
            // 如果 activeOrders 無可用訂單，且 UI 未滿，顯示 pendingOrders[0]
            nextOrder = pendingOrders[0];
            gameManager.UpdateAvailablePlateUI(nextOrder.orderName);
            //Debug.Log($"AvailablePlateUI set to: 訂單 {nextOrder.orderName} (from pendingOrders)");
        }
        else
        {
            gameManager.UpdateAvailablePlateUI(null);
            Debug.Log("No available orders, disabling available plate UI");
        }
    }
}