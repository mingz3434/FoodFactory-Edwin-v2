using UnityEngine;
using UnityEngine.UI;

public enum FoodType{
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

public class Food : Actor_Game{

   public static Food CreateFood(FoodType type, Transform parentTransform, Vector3 position){
      Food _food;
      switch (type){
         case FoodType.Chicken_Raw: Instantiate(Resources.Load("Prefabs/Food/Chicken_Raw"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Chicken_Cut: Instantiate(Resources.Load("Prefabs/Food/Chicken_Cut"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Chicken_Strried: Instantiate(Resources.Load("Prefabs/Food/Chicken_Strried"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Chicken_Fried_a: Instantiate(Resources.Load("Prefabs/Food/Chicken_Fried_a"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Chicken_Fried_b: Instantiate(Resources.Load("Prefabs/Food/Chicken_Fried_b"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Chicken_Failed: Instantiate(Resources.Load("Prefabs/Food/Chicken_Failed"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Potato_Raw: Instantiate(Resources.Load("Prefabs/Food/Potato_Raw"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Potato_Cut: Instantiate(Resources.Load("Prefabs/Food/Potato_Cut"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Potato_Fried: Instantiate(Resources.Load("Prefabs/Food/Potato_Fried"), position, parentTransform); _food.transform.position = position; break;
         case FoodType.Potato_Failed: Instantiate(Resources.Load("Prefabs/Food/Potato_Failed"), position, parentTransform); _food.transform.position = position; break;
         default: Debug.Log("FoodType not found"); break; //P: which not supposed to happen.
        }
   }

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