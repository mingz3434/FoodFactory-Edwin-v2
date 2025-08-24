using UnityEngine;

public class HookCollider : MonoBehaviour
{
    public Transform objectContainer;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Food"))
        {
            Food food = collision.gameObject.GetComponent<Food>();
            if (food != null)
            {
                // 檢查是否在機器中
                Machine machine = null;
                if (food.transform.parent != null)
                {
                    machine = food.transform.parent.GetComponentInParent<Machine>();
                }
                if (machine != null)
                {
                    int slotIndex = -1;
                    for (int i = 0; i < machine.slotPositions.Length; i++)
                    {
                        if (machine.slotPositions[i] == food.transform.parent)
                        {
                            slotIndex = i;
                            break;
                        }
                    }
                    if (slotIndex >= 0)
                    {
                        machine.RemoveFood(slotIndex);
                    }
                    Debug.Log("Hook " + slotIndex.ToString());
                }

                // 檢查食物是否在出餐口中
                DeliveryPlate plate = null;
                if (food.transform.parent != null)
                {
                    plate = food.transform.parent.GetComponent<DeliveryPlate>();
                }

                if (plate != null)
                {
                    plate.RemoveFood(food.gameObject); // 從 stack 移除
                }

                food.gameObject.SetActive(false);
                Destroy(food.GetComponent<ConveyorMover>());
                Rigidbody foodRb = food.GetComponent<Rigidbody>();
                if (foodRb != null)
                {
                    foodRb.isKinematic = true;
                    foodRb.useGravity = false;
                    //foodRb.linearVelocity = Vector3.zero;
                    //foodRb.angularVelocity = Vector3.zero;
                }
                food.transform.SetParent(objectContainer);
                food.transform.localPosition = Vector3.zero;
                food.gameObject.SetActive(true);
                food.HideProgress();
                //Debug.Log("Food hooked back to ObjectContainer!");
                playerMovement.ResetHook();
            }
        }
    }
}