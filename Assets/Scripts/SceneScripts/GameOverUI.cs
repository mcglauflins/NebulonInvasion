using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    
    [SerializeField] private string mainMenuSceneName = "HomeScreen";
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    
    private void Start()
    {
        Debug.Log("GameOverUI initialized");
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("Restart button listener added");
        }
        else
        {
            Debug.LogError("Restart button reference is missing!");
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(LoadMainMenu);
            Debug.Log("Main menu button listener added");
        }
        else
        {
            Debug.LogError("Main menu button reference is missing!");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
            Debug.Log("Quit button listener added");
        }
        else
        {
            Debug.LogError("Quit button reference is missing!");
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("RestartGame button clicked");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
    
    public void LoadMainMenu()
    {
        Debug.Log("LoadMainMenu button clicked");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void QuitGame()
    {
        Debug.Log("QuitGame button clicked");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}