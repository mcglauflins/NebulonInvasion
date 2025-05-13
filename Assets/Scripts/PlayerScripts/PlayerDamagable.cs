using UnityEngine;
using System.Collections;

public class PlayerDamagable : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    
    [Header("Death Settings")]
    [SerializeField] private float deathDuration = .7f;
    [SerializeField] private float respawnDelay = .3f;
    
    [Header("Physics Settings")]
    [SerializeField] private float normalGravityScale = 3f;
    [SerializeField] private float deathGravityScale = 1f;
    
    [Header("Invincibility")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float respawnInvincibilityTime = 3f;
    
    [Header("Camera Control")]
    [SerializeField] private GameObject mainCamera;
    private GameObject dummyCameraTarget;
    private Transform originalCameraTarget;
    private Component cinemachineComponent;
    
    public static event System.Action<int, int> OnHealthChanged;
    public static event System.Action OnPlayerDeath;
    public static event System.Action OnPlayerRespawn;
    
    private Animator animator;
    private PlayerController playerController;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private new Camera camera;
    
    private bool isInvincible = false;
    private bool isDead = false;
    private Vector3 respawnPosition;
    private float originalGravityScale;
    private bool originalColliderTrigger;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        camera = Camera.main;
        
        currentHealth = maxHealth;
        originalGravityScale = rb.gravityScale;
        
        respawnPosition = transform.position;
        
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.updateMode = AnimatorUpdateMode.Normal;
        }
        
        SetupCameraControl();
    }
    
    private void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void SetupCameraControl()
    {
        dummyCameraTarget = new GameObject("PlayerDeathCameraTarget");
        
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindWithTag("MainCamera");
        }
        
        if (mainCamera != null)
        {
            MonoBehaviour[] allComponents = mainCamera.GetComponents<MonoBehaviour>();
            
            foreach (MonoBehaviour component in allComponents)
            {
                if (component.GetType().Name.Contains("Cinemachine") || 
                    component.GetType().Name.Contains("Camera"))
                {
                    System.Reflection.PropertyInfo followProperty = component.GetType().GetProperty("Follow");
                    
                    if (followProperty != null && followProperty.PropertyType == typeof(Transform))
                    {
                        cinemachineComponent = component;
                        originalCameraTarget = (Transform)followProperty.GetValue(component);
                        
                        Debug.Log($"Found camera control component: {component.GetType().Name}");
                        break;
                    }
                }
            }
        }
    }
    
    private void FreezeCamera()
    {
        if (mainCamera != null && cinemachineComponent != null)
        {
            dummyCameraTarget.transform.position = mainCamera.transform.position;
            
            System.Reflection.PropertyInfo followProperty = cinemachineComponent.GetType().GetProperty("Follow");
            
            if (followProperty != null)
            {
                try
                {
                    followProperty.SetValue(cinemachineComponent, dummyCameraTarget.transform);
                    Debug.Log("Camera frozen at death position");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error freezing camera: {e.Message}");
                }
            }
        }
    }
    
    private void ResetCamera()
    {
        if (mainCamera != null && cinemachineComponent != null && originalCameraTarget != null)
        {
            System.Reflection.PropertyInfo followProperty = cinemachineComponent.GetType().GetProperty("Follow");
            
            if (followProperty != null)
            {
                try
                {
                    followProperty.SetValue(cinemachineComponent, originalCameraTarget);
                    Debug.Log("Camera reset to follow player");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error resetting camera: {e.Message}");
                }
            }
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("hit");
            StartCoroutine(HandleInvincibility());
        }
    }
    
    public void Heal(int amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    public void SetRespawnPoint(Vector3 newRespawnPoint)
    {
        respawnPosition = newRespawnPoint;
        Debug.Log($"Player respawn point set to: {newRespawnPoint}");
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        OnPlayerDeath?.Invoke();
        
        if (playerController != null)
            playerController.enabled = false;
        
        if (animator != null)
        {
            animator.SetBool(AnimationStrings.isAlive, false);
            animator.SetTrigger("hurt");
            animator.applyRootMotion = false;
            
            Debug.Log("Death animation triggered - isAlive=false, hurt=true");
        }
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            rb.simulated = false;
            
            Debug.Log("Rigidbody completely frozen for death");
        }
        
        FreezeCamera();
        
        StartCoroutine(ControlledFallAndRespawn());
    }

    private IEnumerator ControlledFallAndRespawn()
    {
        transform.rotation = Quaternion.identity;
        
        Vector3 originalScale = transform.localScale;
        Vector3 startPosition = transform.position;
        
        float kickbackDirection = (transform.localScale.x > 0) ? -1f : 1f;
        
        float totalDuration = 0.85f;
        float kickbackPeakTime = 0.15f;
        
        float kickbackForce = 1.5f;
        float kickbackRise = 0.8f;
        float fallSpeed = 8f;
        
        float elapsed = 0f;
        
        while (elapsed < totalDuration)
        {
            float progress = elapsed / totalDuration;
            
            float x, y;
            
            if (progress < kickbackPeakTime)
            {
                float kickProgress = progress / kickbackPeakTime;
                
                x = startPosition.x + (kickbackDirection * kickbackForce * kickProgress);
                
                y = startPosition.y + (kickbackRise * kickProgress);
            }
            else
            {
                float fallProgress = (progress - kickbackPeakTime) / (1 - kickbackPeakTime);
                
                x = startPosition.x + (kickbackDirection * kickbackForce);
                
                float initialHeight = startPosition.y + kickbackRise;
                float fallDistance = fallSpeed * fallProgress * fallProgress;
                y = initialHeight - fallDistance;
            }
            
            transform.position = new Vector3(x, y, startPosition.z);
            
            transform.rotation = Quaternion.identity;
            transform.localScale = originalScale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        
        transform.position = respawnPosition;
        transform.rotation = Quaternion.identity;
        transform.localScale = originalScale;
        
        float quickRespawnDelay = 0.3f;
        yield return new WaitForSeconds(quickRespawnDelay);
        
        Respawn();
    }
    
    private void Respawn()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        isDead = false;
        
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        
        transform.rotation = Quaternion.identity;
        
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = originalGravityScale;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.WakeUp();
        }
        
        if (animator != null)
        {
            animator.SetBool(AnimationStrings.isAlive, true);
            animator.SetBool(AnimationStrings.isMoving, false);
            animator.SetBool(AnimationStrings.isRunning, false);
            animator.SetFloat(AnimationStrings.yVelocity, 0);
            animator.Rebind();
            animator.Update(0f);
            animator.applyRootMotion = false;
        }
        
        if (playerController != null)
        {
            playerController.ResetAllMovement();
            
            playerController.enabled = true;
            
            if (playerController._isFacingRight != true)
            {
                playerController._isFacingRight = true;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), 
                                             transform.localScale.y, 
                                             transform.localScale.z);
            }
        }
        
        ResetCamera();
        
        StartCoroutine(HandleRespawnInvincibility());
        
        OnPlayerRespawn?.Invoke();
        
        Debug.Log("Player fully respawned and reset with idle state!");
    }
    
    private IEnumerator HandleInvincibility()
    {
        isInvincible = true;
        
        float flashTimer = 0;
        while (flashTimer < invincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashDuration);
            flashTimer += flashDuration;
        }
        
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    
    private IEnumerator HandleRespawnInvincibility()
    {
        isInvincible = true;
        
        float flashTimer = 0;
        
        while (flashTimer < respawnInvincibilityTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashDuration);
            flashTimer += flashDuration;
        }
        
        spriteRenderer.enabled = true;
        isInvincible = false;
    }
    
    [ContextMenu("Trigger Death")]
    public void TriggerDeath()
    {
        currentHealth = 0;
        Die();
    }
    
    public bool IsDead => isDead;
    public bool IsInvincible => isInvincible;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}