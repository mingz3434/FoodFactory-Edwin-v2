using UnityEngine;

public class Fryer : Machine{

   public GameObject[] slots;

   public static Fryer CreateFryer(Fryer prefab, Transform parentTransform, Vector3 position){
      var fryer = Instantiate(prefab, parentTransform);
      fryer.transform.position = position;
      return fryer;
   }

   public void Fry(Food food, string newProductName, Sprite newSprite, int availableSlotId){
      var tray = food.tray;
      var sr = food.GetComponent<SpriteRenderer>();

      Debug.Log($"Fryer: Frying {newProductName}...");

      tray.SnapTo(this.slots[availableSlotId].transform);
      tray.canvas_GO.SetActive(true);

      tray.slider.gameObject.SetActive(true);
      BarFiller.CreateBarFiller_Loading(tray.transform, tray.slider, 4f, () => {
         Debug.Log("Frying complete!");
         tray.slider.gameObject.SetActive(false);
         tray.exclamationMark_Text.gameObject.SetActive(true);
         tray.exclamationMark_Text.color = Color.yellow;

         food.productName = newProductName;
         sr.sprite = newSprite;
      });
   }

}