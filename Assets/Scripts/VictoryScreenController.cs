using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class VictoryScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [Header("Animation")]
    [SerializeField] private float delayBeforeShow = 1.0f;
    [SerializeField] private float fadeInTime = 0.5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private float victoryVolume = 1.0f;

    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameplayScene";
    [SerializeField] private float buttonClickSoundVolume = 0.5f;
    [SerializeField] private AudioClip buttonClickSound;
    
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "HomeScreen";
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        // Get or add a CanvasGroup to the victory panel
        if (victoryPanel != null)
        {
            canvasGroup = victoryPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = victoryPanel.AddComponent<CanvasGroup>();
            
            // Hide it initially
            canvasGroup.alpha = 0;
            victoryPanel.SetActive(false);
        }
        
        // Set up button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }
    
    public void ShowVictoryScreen()
    {
        StartCoroutine(DisplayVictoryScreen());
    }
    
    private IEnumerator DisplayVictoryScreen()
    {
        // Optional: Freeze the game
        Time.timeScale = 0;
        
        // Delay before showing
        yield return new WaitForSecondsRealtime(delayBeforeShow);
        
        // Play victory sound
        if (victorySound != null)
        {
            AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position, victoryVolume);
        }
        
        // Show the panel
        victoryPanel.SetActive(true);
        
        // Fade in
        float elapsed = 0;
        while (elapsed < fadeInTime)
        {
            canvasGroup.alpha = elapsed / fadeInTime;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        // Ensure fully visible
        canvasGroup.alpha = 1;
    }
    
    private void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1;
        
        // Reload the current scene
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    private void GoToMainMenu()
    {
        // Reset time scale
        Time.timeScale = 1;
        
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

        private void QuitGame()
    {

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}