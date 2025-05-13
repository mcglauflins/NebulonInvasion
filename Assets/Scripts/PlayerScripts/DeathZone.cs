using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [SerializeField] private bool instantDeath = true;
    [SerializeField] private int damage = 100;
    
    private float lastTriggerTime = 0f;
    private float triggerCooldown = 0.5f;
    
    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - lastTriggerTime < triggerCooldown)
        {
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            PlayerDamagable playerDamagable = other.GetComponent<PlayerDamagable>();
            if (playerDamagable != null && !playerDamagable.IsDead)
            {
                lastTriggerTime = Time.time;
                
                bool canProcessDeath = true;
                
                if (instantDeath && playerDamagable.CurrentHealth <= damage)
                {
                    canProcessDeath = GameManager.HasInstance() && GameManager.Instance != null;
                }
                
                if (canProcessDeath)
                {
                    if (instantDeath)
                    {
                        playerDamagable.TakeDamage(playerDamagable.CurrentHealth);
                    }
                    else
                    {
                        playerDamagable.TakeDamage(damage);
                    }
                    
                    Debug.Log("Player entered death zone - damage applied");
                }
                else
                {
                    Debug.LogWarning("Cannot process player death - GameManager not available");
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.lossyScale);
    }
}