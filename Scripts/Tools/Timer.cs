using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour{

   public static Timer CreateTimer(GameObject creator, float waitDuration, Action onFinished){
      var timer = creator.AddComponent<Timer>();
      timer.StartTimer(waitDuration, onFinished);
      return timer;
   }

   public void StartTimer(float waitDuration, Action onFinished){
      StartCoroutine(TimerCoroutine(waitDuration, onFinished));
   }

   private IEnumerator TimerCoroutine(float waitDuration, Action onFinished){
      float elapsedTime = 0f;
      while (elapsedTime < waitDuration){
         elapsedTime += Time.fixedDeltaTime;
         yield return new WaitForFixedUpdate();
      }
      onFinished?.Invoke();
      Destroy(this);
   }

   // ************************************************

   public static Timer CreateLoopingTimer(GameObject creator, float waitDuration, Action onFinished, Func<bool> breakCondition){
      var timer = creator.AddComponent<Timer>();
      timer.StartLoopingTimer(waitDuration, onFinished, breakCondition);
      return timer;
   }

   public void StartLoopingTimer(float waitDuration, Action onFinished, Func<bool> breakCondition){
      StartCoroutine(LoopingTimerCoroutine(waitDuration, onFinished, breakCondition));
   }

   private IEnumerator LoopingTimerCoroutine(float waitDuration, Action onFinished, Func<bool> breakCondition){
      while (!breakCondition()){
         yield return new WaitForSeconds(waitDuration);
         onFinished?.Invoke();
      }
      Destroy(this);
   }
}