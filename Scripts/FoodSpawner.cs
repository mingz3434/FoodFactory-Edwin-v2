using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab; // Food Prefab (包含 Food.cs 和 SpriteRenderer)
    public SplineContainer spline;
    public float spawnInterval = 2f;
    public float yOffset = 0.5f;
    public FoodDataSO[] foodData; // 食物數據列表 (Inspector 分配)
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnFood();
        }
    }

    void SpawnFood()
    {
        if (foodData.Length == 0) return;

        FoodDataSO data = foodData[Random.Range(0, foodData.Length)];
        Vector3 basePosition = spline.EvaluatePosition(0f);
        Vector3 up = spline.EvaluateUpVector(0f);
        Vector3 spawnPosition = basePosition + up * yOffset;
        GameObject foodObj = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        Food food = foodObj.GetComponent<Food>();
        food.type = data.type;
        food.UpdateSprite(data.initialSprite); // 設置初始 Sprite

        ConveyorMover mover = foodObj.AddComponent<ConveyorMover>();
        mover.spline = spline;
        mover.speed = 5f;
        mover.yOffset = yOffset;

        DestroyWhenEnd destroyScript = foodObj.AddComponent<DestroyWhenEnd>();
        destroyScript.mover = mover;
    }
}