using UnityEngine;

public class OrbitCamera : MonoBehaviour{
   public Transform player; // Reference to the player character's transform
   public float distance = 5f; // Distance from the player
   public float mouseSensitivity = 100f; // Mouse sensitivity
   public float minPitch = -30f; // Minimum pitch angle
   public float maxPitch = 70f; // Maximum pitch angle

   private float yaw = 0f;
   private float pitch = 0f;

   void LateUpdate(){
      float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 1.5f * Time.deltaTime;
      float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

      yaw += mouseX;
      pitch -= mouseY;
      pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

      Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
      Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
      Vector3 cameraPosition = player.position + offset;

      this.transform.position = cameraPosition;
      this.transform.LookAt(player.position);
   }
}