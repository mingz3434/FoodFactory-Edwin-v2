using System;
using System.Collections;
using UnityEngine;

public class PlayOneShotter : MonoBehaviour{

   public static PlayOneShotter CreatePlayOneShotter_ClipTime(GameObject creator, AudioClip clip, Action onFinished){
      var playOneShotter = creator.AddComponent<PlayOneShotter>();
      playOneShotter.StartTimerAndPlay_ClipTime(clip, onFinished);
      return playOneShotter;
   }

   public void StartTimerAndPlay_ClipTime(AudioClip clip, Action onFinished){
      var audioSource = this.gameObject.AddComponent<AudioSource>();
      audioSource.volume = 0.3f;
      audioSource.PlayOneShot(clip);
      StartCoroutine(TimerCoroutine_ClipTime(clip, onFinished));
   }

   IEnumerator TimerCoroutine_ClipTime(AudioClip clip, Action onFinished){
      yield return new WaitForSeconds(clip.length);
      onFinished?.Invoke();
      Destroy(this);
   }
   
   // ===================================


   public static PlayOneShotter CreatePlayOneShotter_CustomTime(GameObject creator, AudioClip clip, float time, Action onFinished){
      var playOneShotter = creator.AddComponent<PlayOneShotter>();
      playOneShotter.StartTimerAndPlay_CustomTime(clip, time, onFinished);
      return playOneShotter;      
   }

   public void StartTimerAndPlay_CustomTime(AudioClip clip, float time, Action onFinished){
      var audioSource = this.gameObject.AddComponent<AudioSource>();
      audioSource.volume = 0.3f;
      audioSource.PlayOneShot(clip);
      StartCoroutine(TimerCoroutine_CustomTime(time, onFinished));
   }

   IEnumerator TimerCoroutine_CustomTime(float time, Action onFinished){
      yield return new WaitForSeconds(time);
      onFinished?.Invoke();
      Destroy(this);
   }


}