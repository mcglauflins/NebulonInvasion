using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int checkpointIndex;
    [SerializeField] private bool activateOnTouch = true;
    [SerializeField] private GameObject activateEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip activateSound;
    
    private bool isActivated = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activateOnTouch)
        {
            ActivateCheckpoint(other.gameObject);
        }
    }
    
    private void ActivateCheckpoint(GameObject player)
    {
        PlayerDamagable playerDamagable = player.GetComponent<PlayerDamagable>();
        if (playerDamagable != null)
        {
            playerDamagable.SetRespawnPoint(transform.position);
            
            if (GameManager.HasInstance() && GameManager.Instance != null)
            {
                GameManager.Instance.ReachCheckpoint(checkpointIndex);
            }
        }
        
        if (!isActivated)
        {
            isActivated = true;
            
            if (activateEffect != null)
            {
                activateEffect.SetActive(true);
                Invoke("TurnOffEffect", 2f);
            }
            
            if (audioSource != null && activateSound != null)
            {
                audioSource.PlayOneShot(activateSound);
            }
        }
    }
    
    private void TurnOffEffect()
    {
        if (activateEffect != null)
        {
            activateEffect.SetActive(false);
        }
    }
}