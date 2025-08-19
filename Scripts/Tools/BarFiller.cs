using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BarFiller : MonoBehaviour{
   public Slider slider; public float fillTime; public float currentTime;
   
   public static BarFiller CreateBarFiller_Loading(Transform parentTransform, Slider slider, float fillTime, Action onFillComplete){
      var bf = parentTransform.gameObject.AddComponent<BarFiller>();
      bf.slider = slider;
      bf.fillTime = fillTime;
      bf.slider.minValue = 0;
      bf.slider.maxValue = 1;
      bf.currentTime = 0;
      bf.StartFill(onFillComplete);
      return bf;
   }

   public void StartFill(Action onFillComplete){
      this.StartCoroutine(FillBarCoroutine(onFillComplete));
   }

   IEnumerator FillBarCoroutine(Action onFillComplete){
      float currentTime = 0f;
      while (currentTime < fillTime){
         slider.value = currentTime / fillTime;
         currentTime += Time.deltaTime;
         yield return null;
      }
      slider.value = 1f;
      onFillComplete?.Invoke();
      Destroy(this);
   }

}