using UnityEngine;

public class SimpleDeathCameraHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject cameraObject;
    
    [Header("Settings")]
    [SerializeField] private bool enableLogging = true;
    
    private bool isPlayerDead = false;
    private Vector3 frozenPosition;
    private Vector3 frozenRotation;
    
    private MonoBehaviour[] cameraScripts;
    private bool[] originalEnabledStates;
    
    private void Awake()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        if (cameraObject == null)
        {
            cameraObject = Camera.main.gameObject;
        }
        
        if (cameraObject != null)
        {
            cameraScripts = cameraObject.GetComponents<MonoBehaviour>();
            originalEnabledStates = new bool[cameraScripts.Length];
        }
    }
    
    private void Start()
    {
        PlayerDamagable.OnPlayerDeath += FreezeCameraImmediate;
        PlayerDamagable.OnPlayerRespawn += UnfreezeCamera;
        
        LogMessage("SimpleDeathCameraHandler initialized");
    }
    
    private void LateUpdate()
    {
        if (isPlayerDead && cameraObject != null)
        {
            cameraObject.transform.position = frozenPosition;
            cameraObject.transform.eulerAngles = frozenRotation;
        }
    }
    
    private void FreezeCameraImmediate()
    {
        if (cameraObject == null) return;
        
        LogMessage("FREEZING CAMERA - IMMEDIATE ACTION");
        
        frozenPosition = cameraObject.transform.position;
        frozenRotation = cameraObject.transform.eulerAngles;
        
        DisableCameraScripts();
        
        isPlayerDead = true;
        
        cameraObject.transform.position = frozenPosition;
        cameraObject.transform.eulerAngles = frozenRotation;
        
        LogMessage("Camera frozen at: " + frozenPosition);
    }
    
    private void UnfreezeCamera()
    {
        if (cameraObject == null) return;
        
        LogMessage("Unfreezing camera");
        isPlayerDead = false;
        
        EnableCameraScripts();
    }
    
    private void DisableCameraScripts()
    {
        if (cameraObject == null || cameraScripts == null) return;
        
        for (int i = 0; i < cameraScripts.Length; i++)
        {
            MonoBehaviour script = cameraScripts[i];
            
            if (script == this || script.GetType() == typeof(Camera)) 
                continue;
            
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Camera") || 
                scriptName.Contains("Follow") || 
                scriptName.Contains("CineVirtual") || 
                scriptName.Contains("cinemachine") ||
                scriptName.Contains("Cinemachine"))
            {
                originalEnabledStates[i] = script.enabled;
                
                script.enabled = false;
                LogMessage("Disabled: " + scriptName);
            }
        }
    }
    
    private void EnableCameraScripts()
    {
        if (cameraObject == null || cameraScripts == null) return;
        
        for (int i = 0; i < cameraScripts.Length; i++)
        {
            MonoBehaviour script = cameraScripts[i];
            
            if (script == this || script.GetType() == typeof(Camera)) 
                continue;
            
            string scriptName = script.GetType().Name;
            if (scriptName.Contains("Camera") || 
                scriptName.Contains("Follow") || 
                scriptName.Contains("CineVirtual") || 
                scriptName.Contains("cinemachine") ||
                scriptName.Contains("Cinemachine"))
            {
                script.enabled = originalEnabledStates[i];
                LogMessage("Re-enabled: " + scriptName);
            }
        }
    }
    
    private void LogMessage(string message)
    {
        if (enableLogging)
        {
            Debug.Log("[DeathCameraHandler] " + message);
        }
    }
    
    private void OnDestroy()
    {
        PlayerDamagable.OnPlayerDeath -= FreezeCameraImmediate;
        PlayerDamagable.OnPlayerRespawn -= UnfreezeCamera;
    }
}