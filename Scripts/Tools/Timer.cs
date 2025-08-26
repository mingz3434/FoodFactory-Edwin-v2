using System;
using System.Collections;
using UnityEngine;

public class Timer : MonoBehaviour{

   public static Timer CreateTimer_Physics(GameObject creator, float waitDuration, Action onFinished){
      var timer = creator.AddComponent<Timer>();
      timer.StartTimer_Physics(waitDuration, onFinished);
      return timer;
   }

   public void StartTimer_Physics(float waitDuration, Action onFinished){
      StartCoroutine(TimerCoroutine_Physics(waitDuration, onFinished));
   }

   private IEnumerator TimerCoroutine_Physics(float waitDuration, Action onFinished){
      float elapsedTime = 0f;
      while (elapsedTime < waitDuration){
         elapsedTime += Time.fixedDeltaTime;
         yield return new WaitForFixedUpdate();
      }
      onFinished?.Invoke();
      Destroy(this);
   }

   // ************************************************

      public static Timer CreateTimer_NoPhysics(GameObject creator, float waitDuration, Action onFinished){
      var timer = creator.AddComponent<Timer>();
      timer.StartTimer_NoPhysics(waitDuration, onFinished);
      return timer;
   }

   public void StartTimer_NoPhysics(float waitDuration, Action onFinished){
      StartCoroutine(TimerCoroutine_NoPhysics(waitDuration, onFinished));
   }

   private IEnumerator TimerCoroutine_NoPhysics(float waitDuration, Action onFinished){
      float elapsedTime = 0f;
      while (elapsedTime < waitDuration){
         elapsedTime += Time.deltaTime;
         yield return null;
      }
      onFinished?.Invoke();
      Destroy(this);
   }

   // ************************************************

   public static Timer CreateLoopingTimer_Physics(GameObject creator, float waitDuration, Action onInterval, bool until){
      var timer = creator.AddComponent<Timer>();
      timer.StartLoopingTimer_Physics(waitDuration, onInterval, until);
      return timer;
   }

   public void StartLoopingTimer_Physics(float waitDuration, Action onInterval, bool until){
      StartCoroutine(LoopingTimerCoroutine_Physics(waitDuration, onInterval, until));
   }

   private IEnumerator LoopingTimerCoroutine_Physics(float waitDuration, Action onInterval, bool until){
      while (!until){
         float elapsedTime = 0f;
         while (elapsedTime < waitDuration){
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
         }
         onInterval?.Invoke();
      }
      Destroy(this);
   }


   // *************************************************

   public static Timer CreateLoopingTimer_NoPhysics(GameObject creator, float waitDuration, Action onInterval, bool until){
      var timer = creator.AddComponent<Timer>();
      timer.StartLoopingTimer_NoPhysics(waitDuration, onInterval, until);
      return timer;
   }

   public void StartLoopingTimer_NoPhysics(float waitDuration, Action onInterval, bool until){
      StartCoroutine(LoopingTimerCoroutine_NoPhysics(waitDuration, onInterval, until));
   }

   private IEnumerator LoopingTimerCoroutine_NoPhysics(float waitDuration, Action onInterval, bool until){
      while (!until){
         float elapsedTime = 0f;
         while (elapsedTime < waitDuration){
            elapsedTime += Time.deltaTime;
            yield return null;
         }
         onInterval?.Invoke();
      }
      Destroy(this);
   }
}