using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    // Singleton instance
    public static ScreenFlash Instance { get; private set; }
    
    [Header("Flash Settings")]
    [SerializeField] private float defaultFlashDuration = 0.2f;
    [SerializeField] private Color defaultFlashColor = new Color(1f, 1f, 1f, 0.7f);
    
    // The UI Image used for flashing
    private Image flashImage;
    
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Get or add the Image component
        flashImage = GetComponent<Image>();
        if (flashImage == null)
        {
            flashImage = gameObject.AddComponent<Image>();
        }
        
        // Make sure image starts transparent
        if (flashImage != null)
        {
            flashImage.color = new Color(defaultFlashColor.r, defaultFlashColor.g, defaultFlashColor.b, 0);
        }
    }
    
    public void Flash()
    {
        // Use default values
        Flash(defaultFlashDuration, defaultFlashColor);
    }
    
    public void Flash(float duration)
    {
        Flash(duration, defaultFlashColor);
    }
    
    public void Flash(float duration, Color flashColor)
    {
        if (flashImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashCoroutine(duration, flashColor));
        }
    }
    
    private IEnumerator FlashCoroutine(float duration, Color flashColor)
    {
        // Set image active and color
        flashImage.gameObject.SetActive(true);
        
        // Initial flash
        flashImage.color = flashColor;
        
        // Fade out over duration
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(flashColor.a, 0, elapsed / duration);
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure completely transparent at end
        flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0);
        
        if (flashImage.color.a <= 0.01f)
        {
            flashImage.gameObject.SetActive(false);
        }
    }
}