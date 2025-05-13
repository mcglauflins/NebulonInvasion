using UnityEngine;
using System.Collections;

public class ExplosionTrap : MonoBehaviour
{
    [Header("Trap Settings")]
    [SerializeField] private bool activateOnce = true;
    [SerializeField] private float explosionDelay = 0.1f;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Explosion Effects")]
    [SerializeField] private GameObject explosionAnimationPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private ParticleSystem explosionParticles;
    
    [Header("Player Knockback")]
    [SerializeField] private float knockbackForce = 15f;
    [SerializeField] private float downwardForce = 20f;
    [SerializeField] private float stunDuration = 0.5f;
    
    [Header("Floor Breaking")]
    [SerializeField] private bool breakFloor = true;
    [SerializeField] private GameObject floorToBreak;
    [SerializeField] private float floorDestroyDelay = 0.2f;
    
    private bool isActivated = false;
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag) && (!isActivated || !activateOnce))
        {
            isActivated = true;
            StartCoroutine(ActivateTrap(collision.gameObject));
        }
    }
    
    private IEnumerator ActivateTrap(GameObject player)
    {
        yield return new WaitForSeconds(explosionDelay);
        
        if (AudioManager.Instance != null)
        {
            if (explosionSound != null)
            {
                AudioManager.Instance.PlaySoundEffectAtPosition(explosionSound, transform.position);
            }
            else
            {
                AudioManager.Instance.PlayExplosionSoundAtPosition(transform.position);
            }
        }
        else if (audioSource != null && explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        if (explosionAnimationPrefab != null)
        {
            GameObject explosionObj = Instantiate(explosionAnimationPrefab, transform.position, Quaternion.identity);
            Destroy(explosionObj, 2f);
        }
        
        if (explosionParticles != null)
        {
            explosionParticles.Play();
        }
        
        if (player != null)
        {
            ApplyKnockbackToPlayer(player);
        }
        
        if (breakFloor && floorToBreak != null)
        {
            yield return new WaitForSeconds(floorDestroyDelay);
            floorToBreak.SetActive(false);
        }
        
        if (!activateOnce)
        {
            yield return new WaitForSeconds(5f);
            isActivated = false;
        }
    }
    
    private void ApplyKnockbackToPlayer(GameObject player)
    {
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        PlayerController playerController = player.GetComponent<PlayerController>();
        
        if (playerRb != null)
        {
            Vector2 explosionForce = new Vector2(0, knockbackForce);
            playerRb.linearVelocity = Vector2.zero;
            playerRb.AddForce(explosionForce, ForceMode2D.Impulse);
            
            StartCoroutine(ApplyDownwardForceAfterDelay(playerRb));
        }
        
        if (playerController != null)
        {
            StartCoroutine(TemporarilyDisablePlayerControl(playerController));
        }
    }
    
    private IEnumerator ApplyDownwardForceAfterDelay(Rigidbody2D playerRb)
    {
        yield return new WaitForSeconds(0.2f);
        
        if (playerRb != null)
        {
            Vector2 downForce = new Vector2(0, -downwardForce);
            playerRb.AddForce(downForce, ForceMode2D.Impulse);
            
            Physics2D.IgnoreLayerCollision(
                playerRb.gameObject.layer, 
                LayerMask.NameToLayer("Ground"), 
                true
            );
            
            yield return new WaitForSeconds(0.5f);
            
            Physics2D.IgnoreLayerCollision(
                playerRb.gameObject.layer, 
                LayerMask.NameToLayer("Ground"), 
                false
            );
        }
    }
    
    private IEnumerator TemporarilyDisablePlayerControl(PlayerController playerController)
    {
        playerController.enabled = false;
        
        yield return new WaitForSeconds(stunDuration);
        
        if (playerController != null && !playerController.GetComponent<PlayerDamagable>()?.IsDead == true)
        {
            playerController.enabled = true;
            playerController.ResetAllMovement();
        }
    }
}