using UnityEngine;

public class Cutter : Machine{

   public GameObject slot;

   public static Cutter CreateCutter(Cutter prefab, Transform parentTransform, Vector3 position){
      var cutter = Instantiate(prefab, parentTransform);
      cutter.transform.position = position;
      return cutter;
   }

   public void Slice(Food food, string newProductName, Sprite newSprite){
      var tray = food.tray;
      var sr = food.GetComponent<SpriteRenderer>();

      Debug.Log($"Cutter: Cutting {newProductName}...");

      tray.SnapTo(this.slot.transform);
      tray.canvas_GO.SetActive(true);

      tray.slider.gameObject.SetActive(true);
      BarFiller.CreateBarFiller_Loading(tray.transform, tray.slider, 2f, () => {
         Debug.Log("Cutting complete!");
         tray.slider.gameObject.SetActive(false);
         tray.exclamationMark_Text.gameObject.SetActive(true);
         tray.exclamationMark_Text.color = Color.green;

         food.productName = newProductName;
         sr.sprite = newSprite;
      });
   }
}