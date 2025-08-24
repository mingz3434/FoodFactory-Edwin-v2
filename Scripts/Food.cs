using UnityEngine;
using UnityEngine.UI;

public enum FoodType
{
    Chicken_Raw,
    Chicken_Cut,
    Chicken_Strried,
    Chicken_Fried_a,
    Chicken_Fried_b,
    Chicken_Failed,
    Potato_Raw,
    Potato_Cut,
    Potato_Fried,
    Potato_Failed,
}

public class Food : MonoBehaviour
{
    public FoodType type;
    public Image ImgFood; // 附加到 Food Prefab 的 SpriteRenderer
    public Slider progressSlider;

    void Start()
    {
        if (ImgFood == null)
        {
            ImgFood = GetComponent<Image>();
        }
        if (progressSlider == null)
        {
            progressSlider = GetComponentInChildren<Slider>();
        }
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 靜止時面向上
        Rigidbody rb = GetComponent<Rigidbody>();
        if ((rb != null && rb.isKinematic) || GetComponent<ConveyorMover>() == null)
        {
            transform.rotation = Quaternion.identity; // y 軸向上
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject collided = collision.gameObject;
        // 檢查是否碰撞出餐口或其子物件
        DeliveryPlate plate = collided.GetComponent<DeliveryPlate>();
        if (plate == null && collided.transform.parent != null)
        {
            plate = collided.transform.parent.GetComponent<DeliveryPlate>();
        }

        if (plate != null)
        {
            // 避免重複堆疊
            if (plate.GetComponent<DeliveryPlate>().stackedFoods.Contains(gameObject))
            {
                return;
            }
            plate.StackFood(gameObject);
        }
    }

    public void UpdateSprite(Sprite newSprite)
    {
        if (ImgFood != null)
        {
            ImgFood.sprite = newSprite;
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(true);
            progressSlider.value = progress; // 0-1
        }
    }

    public void HideProgress()
    {
        if (progressSlider != null)
        {
            progressSlider.gameObject.SetActive(false);
        }
    }
}