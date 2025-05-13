using UnityEngine;

public class LizardProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip shootSound; // New audio clip for shooting
    [SerializeField] private float soundVolume = 1f; // Volume control
    
    private Vector2 direction;
    private bool hasHit = false;
    private Animator animator;
    private Vector3 originalScale;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
    }
    
    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        float xScale = Mathf.Abs(originalScale.x);
        
        if (dir.x < 0)
        {
            transform.localScale = new Vector3(-xScale, originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = new Vector3(xScale, originalScale.y, originalScale.z);
        }
        
        // Play shooting sound immediately after direction is set
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, soundVolume);
        }
    }
    
    private void Update()
    {
        if (!hasHit)
        {
            transform.position += new Vector3(direction.x, direction.y, 0) * speed * Time.deltaTime;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;
        
        PlayerDamagable playerDamagable = collision.GetComponent<PlayerDamagable>();
        if (playerDamagable != null)
        {
            playerDamagable.TakeDamage(damage);
            hasHit = true;
        }
        else if (collision.CompareTag("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            hasHit = true;
        }
        
        if (hasHit)
        {
            speed = 0;
            
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            if (animator != null && animator.HasState(0, Animator.StringToHash("hit")))
            {
                animator.SetTrigger("hit");
                Destroy(gameObject, 0.5f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}