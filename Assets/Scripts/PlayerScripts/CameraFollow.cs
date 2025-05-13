using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothTime = 0.3f;
    
    [Header("Death Settings")]
    [SerializeField] private bool freezeOnDeath = true;    
    
    [Header("Boundaries (Optional)")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 5f;
    
    private Vector3 velocity = Vector3.zero;
    private PlayerDamagable playerDamagable;
    private bool isPlayerDead = false;
    private Vector3 deathPosition;
    private Transform originalTarget;
    
    private void Start()
    {
        originalTarget = target;
        
        if (target != null)
        {
            playerDamagable = target.GetComponent<PlayerDamagable>();
            if (playerDamagable != null)
            {
                PlayerDamagable.OnPlayerDeath += HandlePlayerDeath;
                PlayerDamagable.OnPlayerRespawn += HandlePlayerRespawn;
                
                Debug.Log("Camera: Subscribed to player death/respawn events");
            }
        }
    }
    
    private void OnDestroy()
    {
        PlayerDamagable.OnPlayerDeath -= HandlePlayerDeath;
        PlayerDamagable.OnPlayerRespawn -= HandlePlayerRespawn;
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        if (isPlayerDead && freezeOnDeath)
        {
            return;
        }
        
        Vector3 targetPos = target.position + offset;
        
        if (useBoundaries)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
    
    private void HandlePlayerDeath()
    {
        deathPosition = transform.position;
        isPlayerDead = true;
        
        target = null;
        
        Debug.Log("Camera: Player died - camera frozen at " + deathPosition);
    }
    
    private void HandlePlayerRespawn()
    {
        isPlayerDead = false;
        target = originalTarget;
        Debug.Log("Camera: Player respawned, resuming normal follow");
    }
}