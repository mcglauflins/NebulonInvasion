using UnityEngine;

public class GateFinishTrigger : MonoBehaviour
{
    [Header("Victory Settings")]
    [SerializeField] private VictoryScreenController victoryScreen;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool playVictoryMusic = true;
    [SerializeField] private AudioClip victoryMusic;
    
    [Header("Effects")]
    [SerializeField] private GameObject victoryEffect;
    [SerializeField] private float effectDuration = 3f;
    
    private bool hasTriggered = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTriggered && other.CompareTag(playerTag))
        {
            hasTriggered = true;
            LevelComplete();
        }
    }
    
    private void LevelComplete()
    {
        Debug.Log("Level Complete! Player reached the gate!");
        
        // Play music if enabled
        if (playVictoryMusic && victoryMusic != null)
        {
            // Find AudioManager
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                // You can implement a special method in AudioManager for victory music
                // Or just create a temporary audio source
                AudioSource victorySource = gameObject.AddComponent<AudioSource>();
                victorySource.clip = victoryMusic;
                victorySource.volume = 0.7f;
                victorySource.Play();
            }
        }
        
        // Show victory effect if assigned
        if (victoryEffect != null)
        {
            GameObject effect = Instantiate(victoryEffect, transform.position, Quaternion.identity);
            Destroy(effect, effectDuration);
        }
        
        // Show victory screen
        if (victoryScreen != null)
        {
            victoryScreen.ShowVictoryScreen();
        }
        else
        {
            // Try to find the victory screen if not assigned
            VictoryScreenController screenController = FindObjectOfType<VictoryScreenController>();
            if (screenController != null)
            {
                screenController.ShowVictoryScreen();
            }
            else
            {
                Debug.LogWarning("Victory Screen not found or assigned!");
            }
        }
        
        // Optionally disable the player's movement
        GameObject player = GameObject.FindWithTag(playerTag);
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
        }
    }
}