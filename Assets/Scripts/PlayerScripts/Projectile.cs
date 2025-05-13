using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 1;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private AudioClip shootSound; 
    [SerializeField] private float soundVolume = 1f; 

    private float direction;
    private bool hasHit = false;
    private Animator animator;
    private Vector3 originalScale; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale; 
    }

    public void SetDirection(float dir)
    {
        direction = dir;

        if (dir < 0)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = originalScale;
        }
        
        if (shootSound != null)
        {
            AudioSource.PlayClipAtPoint(shootSound, transform.position, soundVolume);
        }
    }

    void Start()
    {
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        if (!hasHit)
        {
            transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        UnityEngine.Debug.Log("Bullet hit: " + collision.gameObject.name);

        if (hasHit) return;

        bool shouldRegisterHit = collision.CompareTag("Enemy") || 
                                collision.CompareTag("Ground") ||
                                collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                                collision.GetComponent<IDamageable>() != null;

        if (!shouldRegisterHit)
        {
            UnityEngine.Debug.Log("Hit doesn't qualify for registration");
            return;
        }

        UnityEngine.Debug.Log("Registering hit");
        hasHit = true;

        speed = 0;

        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable != null)
        {
            UnityEngine.Debug.Log("Applying damage");
            damageable.TakeDamage(damage);
        }

        if (animator != null)
        {
            UnityEngine.Debug.Log("Playing hit animation");
            animator.SetTrigger("hit");

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            float destroyDelay = 0.5f; 

            if (clipInfo.Length > 0)
            {
                destroyDelay = clipInfo[0].clip.length;
                UnityEngine.Debug.Log("Animation length: " + destroyDelay);
            }
            else
            {
                UnityEngine.Debug.Log("No clip info found, using default delay: " + destroyDelay);
            }

            Destroy(gameObject, destroyDelay);
        }
        else
        {
            UnityEngine.Debug.Log("No animator found, destroying immediately");
            Destroy(gameObject);
        }
    }
}