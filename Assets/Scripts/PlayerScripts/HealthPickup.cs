using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField] private int healthAmount = 25;
    [SerializeField] private bool isPercentage = false;
    
    [Header("Behavior")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private float respawnTime = 30f;
    [SerializeField] private bool onlyHealIfNotFullHealth = true;
    
    [Header("Animation")]
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float bobHeight = 0.2f;
    [SerializeField] private float rotationSpeed = 45f;
    
    [Header("Effects")]
    [SerializeField] private GameObject pickupEffect;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 0.7f;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    private bool isAvailable = true;
    private Vector3 startPosition;
    private float bobTimer = 0f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
        
        if (itemCollider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(1f, 1f);
            itemCollider = boxCollider;
        }
    }
    
    private void Update()
    {
        if (!isAvailable) return;
        


        bobTimer += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTimer) * bobHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
        

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isAvailable) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerDamagable playerDamagable = other.GetComponent<PlayerDamagable>();
            
            if (playerDamagable != null)
            {

                bool shouldHeal = true;
                if (onlyHealIfNotFullHealth && playerDamagable.CurrentHealth >= playerDamagable.MaxHealth)
                {
                    shouldHeal = false;
                }
                
                if (shouldHeal)
                {
                    int healAmount = healthAmount;
                    if (isPercentage)
                    {
                        healAmount = Mathf.RoundToInt(playerDamagable.MaxHealth * (healthAmount / 100f));
                    }
                    
                    playerDamagable.Heal(healAmount);
                    OnPickup();
                }
            }
        }
    }
    
    private void OnPickup()
    {

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
        }
        
        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator RespawnAfterDelay()
    {
        isAvailable = false;
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
            
        if (itemCollider != null)
            itemCollider.enabled = false;
            
        yield return new WaitForSeconds(respawnTime);
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
            
        if (itemCollider != null)
            itemCollider.enabled = true;
            
        isAvailable = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        if (!destroyOnPickup)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}