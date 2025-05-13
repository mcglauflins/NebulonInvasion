using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [SerializeField] private Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color mediumHealthColor = new Color(0.8f, 0.8f, 0.2f);
    [SerializeField] private Color lowHealthColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float mediumHealthThreshold = 0.6f;
    
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private bool showNumericText = true;
    
    private float targetHealth = 1f;
    
    private void Awake()
    {
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();
            
        if (fillImage == null && healthSlider != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();
            
        if (healthText == null)
            healthText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    private void Start()
    {
        PlayerDamagable.OnHealthChanged += UpdateHealth;
        
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            PlayerDamagable playerDamagable = player.GetComponent<PlayerDamagable>();
            if (playerDamagable != null)
            {
                UpdateHealth(playerDamagable.CurrentHealth, playerDamagable.MaxHealth);
            }
        }
    }
    
    private void Update()
    {
        if (healthSlider.value != targetHealth)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth, Time.deltaTime * animationSpeed);
            
            if (Mathf.Abs(healthSlider.value - targetHealth) < 0.01f)
                healthSlider.value = targetHealth;
        }
    }
    
    private void OnDestroy()
    {
        PlayerDamagable.OnHealthChanged -= UpdateHealth;
    }
    
    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        float healthPercent = (float)currentHealth / maxHealth;
        targetHealth = healthPercent;
        
        if (fillImage != null)
        {
            if (healthPercent <= lowHealthThreshold)
                fillImage.color = lowHealthColor;
            else if (healthPercent <= mediumHealthThreshold)
                fillImage.color = mediumHealthColor;
            else
                fillImage.color = healthyColor;
        }
        
        if (healthText != null && showNumericText)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
        else if (healthText != null)
        {
            healthText.text = "";
        }
    }
}