using UnityEngine;
using TMPro;
using System.Collections;

public class LivesDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private string format = "Lives: {0}";
    
    [Header("Animation")]
    [SerializeField] private bool animateOnChange = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color loseLifeColor = Color.red;
    [SerializeField] private Color gainLifeColor = Color.green;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float pulseSize = 1.2f;
    
    private int previousLives = -1;
    private Coroutine animationCoroutine;
    
    private void Start()
    {
        // Subscribe to lives changed event
        GameManager.OnLivesChanged += UpdateLivesDisplay;
        
        // Initialize with current lives
        if (GameManager.HasInstance() && GameManager.Instance != null)
        {
            previousLives = GameManager.Instance.CurrentLives;
            UpdateLivesDisplay(previousLives);
        }
        else
        {
            Debug.LogWarning("GameManager not found!");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from event
        GameManager.OnLivesChanged -= UpdateLivesDisplay;
    }
    
    private void UpdateLivesDisplay(int lives)
    {
        if (livesText != null)
        {
            // Update text
            livesText.text = string.Format(format, lives);
            
            // Animate if enabled
            if (animateOnChange && previousLives != -1)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                
                Color targetColor = normalColor;
                if (lives < previousLives)
                {
                    targetColor = loseLifeColor;
                }
                else if (lives > previousLives)
                {
                    targetColor = gainLifeColor;
                }
                
                animationCoroutine = StartCoroutine(AnimateText(targetColor));
            }
            
            previousLives = lives;
        }
    }
    
    private IEnumerator AnimateText(Color targetColor)
    {
        // Save original values
        Vector3 originalScale = livesText.transform.localScale;
        Color originalColor = livesText.color;
        
        // Pulse out
        float elapsed = 0f;
        while (elapsed < animationDuration / 2)
        {
            float t = elapsed / (animationDuration / 2);
            livesText.transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseSize, t);
            livesText.color = Color.Lerp(originalColor, targetColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Pulse in
        elapsed = 0f;
        while (elapsed < animationDuration / 2)
        {
            float t = elapsed / (animationDuration / 2);
            livesText.transform.localScale = Vector3.Lerp(originalScale * pulseSize, originalScale, t);
            livesText.color = Color.Lerp(targetColor, normalColor, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at the original values
        livesText.transform.localScale = originalScale;
        livesText.color = normalColor;
        
        animationCoroutine = null;
    }
}