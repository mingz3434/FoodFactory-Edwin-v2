using UnityEngine;

public class Mixer : Machine{

   public GameObject slot;

   public static Mixer CreateMixer(Mixer prefab, Transform parentTransform, Vector3 position){
      var mixer = Instantiate(prefab, parentTransform);
      mixer.transform.position = position;
      return mixer;
   }

   public void Stir(Food food, string newProductName, Sprite newSprite){
      var tray = food.tray;
      var sr = food.GetComponent<SpriteRenderer>();

      Debug.Log($"Mixer: Stirring {newProductName}...");

      tray.SnapTo(this.slot.transform);
      tray.canvas_GO.SetActive(true);

      tray.slider.gameObject.SetActive(true);
      BarFiller.CreateBarFiller_Loading(tray.transform, tray.slider, 3f, () => {
         Debug.Log("Ding!");
         tray.slider.gameObject.SetActive(false);
         tray.exclamationMark_Text.gameObject.SetActive(true);
         tray.exclamationMark_Text.color = Color.green;

         food.productName = newProductName;
         sr.sprite = newSprite;
      });
   }

}