// using UnityEngine;
// using System.Collections;

// public class CameraShake : MonoBehaviour
// {
//     public static CameraShake Instance { get; private set; }
    
//     [Header("Shake Settings")]
//     [SerializeField] private float defaultShakeDuration = 0.5f;
//     [SerializeField] private float defaultShakeStrength = 1.0f;
//     [SerializeField] private float defaultShakeFadeTime = 0.3f;
    
//     private Vector3 originalPosition;
//     private Coroutine shakeCoroutine;
    
//     private void Awake()
//     {
//         // Set up singleton
//         if (Instance == null)
//         {
//             Instance = this;
//         }
//         else
//         {
//             Destroy(this);
//         }
        
//         originalPosition = transform.localPosition;
//     }
    
//     public void ShakeCamera()
//     {
//         ShakeCamera(defaultShakeDuration, defaultShakeStrength);
//     }
    
//     public void ShakeCamera(float duration, float strength)
//     {
//         Debug.Log("ShakeCamera called with duration: " + duration + " and strength: " + strength);
        
//         // Stop any ongoing shake
//         if (shakeCoroutine != null)
//         {
//             StopCoroutine(shakeCoroutine);
//         }
        
//         // Start a new shake
//         shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, strength));
//     }
    
//     private IEnumerator ShakeCoroutine(float duration, float strength)
//     {
//         Debug.Log("Starting camera shake coroutine");
        
//         // Store original position
//         originalPosition = transform.localPosition;
//         float elapsed = 0f;
        
//         while (elapsed < duration)
//         {
//             float currentStrength = strength;
            
//             // Calculate fade based on remaining time
//             if (elapsed > duration - defaultShakeFadeTime)
//             {
//                 float t = (elapsed - (duration - defaultShakeFadeTime)) / defaultShakeFadeTime;
//                 currentStrength = strength * (1f - t);
//             }
            
//             // Create random offset
//             Vector3 shakeOffset = Random.insideUnitSphere * currentStrength;
//             shakeOffset.z = 0; // Keep z position unchanged
            
//             // Apply shake
//             transform.localPosition = originalPosition + shakeOffset;
            
//             elapsed += Time.deltaTime;
//             yield return null;
//         }
        
//         // Reset to original position
//         transform.localPosition = originalPosition;
//         shakeCoroutine = null;
        
//         Debug.Log("Camera shake completed");
//     }
// }