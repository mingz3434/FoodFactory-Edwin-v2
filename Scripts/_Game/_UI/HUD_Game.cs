using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_Game : UserWidget{

   public static HUD_Game CreateHUD(HUD_Game prefab, Transform parentTransform){
      return Instantiate(prefab, parentTransform);
   }

   public GameObject coins_Scores_Abilities;
   public GameObject progressBarContainer;
   public GameObject items;

   public TMP_Text GetCoinText(){return coins_Scores_Abilities.transform.GetChild(0).GetComponent<TMP_Text>(); }
   public TMP_Text GetPlayerNameText(){return coins_Scores_Abilities.transform.GetChild(1).GetComponent<TMP_Text>(); }  
   public TMP_Text GetScoreText(){return coins_Scores_Abilities.transform.GetChild(2).GetComponent<TMP_Text>(); }

   public Slider GetSlider(){return progressBarContainer.GetComponent<Slider>(); }

   public GameObject GetItems(){return items;}

}