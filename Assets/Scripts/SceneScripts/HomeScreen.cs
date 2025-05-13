using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Game Settings")]
    [SerializeField] private string gameSceneName = "GameplayScene";
    [SerializeField] private float buttonClickSoundVolume = 0.5f;
    [SerializeField] private AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        Time.timeScale = 1f;
        
        audioSource = gameObject.AddComponent<AudioSource>();
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    private void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);
    }
    
    private void StartGame()
    {
        PlayButtonSound();
        
        SceneManager.LoadScene(gameSceneName);
    }
    
    private void QuitGame()
    {
        PlayButtonSound();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void ToggleSettings()
    {
        PlayButtonSound();
        
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound, buttonClickSoundVolume);
    }
    
    private void OnDestroy()
    {
        if (playButton != null)
            playButton.onClick.RemoveAllListeners();
            
        if (quitButton != null)
            quitButton.onClick.RemoveAllListeners();
            
        if (settingsButton != null)
            settingsButton.onClick.RemoveAllListeners();
    }
}