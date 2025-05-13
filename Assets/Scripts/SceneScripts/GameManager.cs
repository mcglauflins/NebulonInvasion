using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private int playerLives = 3;
    [SerializeField] private int currentLives;
    
    [Header("Checkpoint System")]
    [SerializeField] private Transform[] checkpoints;
    [SerializeField] private int currentCheckpoint = 0;
    
    [Header("Game Over")]
    [SerializeField] private float quitDelay = 2f;
    
    [Header("Scene Management")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private static bool applicationQuitting = false;
    private static bool isQuitting = false;
    private static GameManager _instance;
    
    private float lastDeathTime = 0f;
    private float deathCooldown = 0.5f;
    
    public static GameManager Instance
    {
        get
        {
            if (applicationQuitting || isQuitting)
            {
                Debug.LogWarning("Instance accessed during quit - returning null");
                return null;
            }
            
            return _instance;
        }
    }
    
    public static event System.Action<int> OnLivesChanged;
    public static event System.Action OnGameOver;
    public static event System.Action OnGameRestart;
    public static event System.Action<Vector3> OnCheckpointChanged;
    
    private PlayerDamagable playerDamagable;
    private GameObject player;
    private Vector3 initialPlayerPosition;
    private bool hasInitialized = false;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            currentLives = playerLives;
            Debug.Log("GameManager initialized as singleton");
        }
        else if (_instance != this)
        {
            Debug.LogWarning("Duplicate GameManager found! Destroying: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        
        applicationQuitting = false;
        isQuitting = false;
    }
    
    private void OnApplicationQuit()
    {
        applicationQuitting = true;
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        applicationQuitting = false;
        
        if (scene.name == gameplaySceneName)
        {
            InitializePlayerReferences();
            isQuitting = false;
        }
    }
    
    private void Start()
    {
        InitializePlayerReferences();
    }
    
    private void InitializePlayerReferences()
    {
        if (playerDamagable != null)
        {
            try
            {
                PlayerDamagable.OnPlayerDeath -= HandlePlayerDeath;
                PlayerDamagable.OnPlayerRespawn -= HandlePlayerRespawn;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error unregistering from player events: " + e.Message);
            }
        }
        
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerDamagable = player.GetComponent<PlayerDamagable>();
            initialPlayerPosition = player.transform.position;
            
            if (checkpoints != null && checkpoints.Length > 0 && playerDamagable != null)
            {
                if (checkpoints[0] != null)
                {
                    playerDamagable.SetRespawnPoint(checkpoints[0].position);
                    if (debugMode) Debug.Log("Initial spawn point set to: " + checkpoints[0].position);
                }
            }
            else if (playerDamagable != null)
            {
                playerDamagable.SetRespawnPoint(initialPlayerPosition);
                if (debugMode) Debug.Log("Initial spawn point set to player start position: " + initialPlayerPosition);
            }
            
            if (playerDamagable != null)
            {
                PlayerDamagable.OnPlayerDeath += HandlePlayerDeath;
                PlayerDamagable.OnPlayerRespawn += HandlePlayerRespawn;
            }
        }
        
        OnLivesChanged?.Invoke(currentLives);
        
        hasInitialized = true;
    }
    
    private void OnDestroy()
    {
        if (playerDamagable != null)
        {
            try
            {
                PlayerDamagable.OnPlayerDeath -= HandlePlayerDeath;
                PlayerDamagable.OnPlayerRespawn -= HandlePlayerRespawn;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error unregistering from player events: " + e.Message);
            }
        }
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    private void HandlePlayerDeath()
    {
        if (isQuitting) return;
        
        if (Time.time - lastDeathTime < deathCooldown)
        {
            Debug.Log("Death ignored - within cooldown period");
            return;
        }
        
        lastDeathTime = Time.time;
        
        if (currentLives > 0)
        {
            currentLives--;
            OnLivesChanged?.Invoke(currentLives);
            
            if (debugMode) Debug.Log("Player died. Lives remaining: " + currentLives);
            
            if (currentLives <= 0)
            {
                if (debugMode) Debug.Log("Game Over - Quitting application");
                
                isQuitting = true;
                StartCoroutine(QuitGameSequence());
            }
        }
        else
        {
            if (debugMode) Debug.Log("Player died with no lives remaining");
            
            if (!isQuitting)
            {
                isQuitting = true;
                StartCoroutine(QuitGameSequence());
            }
        }
    }
    
    private void HandlePlayerRespawn()
    {
        if (debugMode) Debug.Log("Player respawned at checkpoint " + currentCheckpoint);
    }
    
    public void ReachCheckpoint(int checkpointIndex)
    {
        if (checkpoints == null || checkpointIndex < 0 || checkpointIndex >= checkpoints.Length)
        {
            Debug.LogWarning($"Invalid checkpoint index: {checkpointIndex}");
            return;
        }
        
        if (checkpoints[checkpointIndex] == null)
        {
            Debug.LogWarning($"Checkpoint {checkpointIndex} is null!");
            return;
        }
        
        currentCheckpoint = checkpointIndex;
        
        if (playerDamagable != null)
        {
            playerDamagable.SetRespawnPoint(checkpoints[currentCheckpoint].position);
        }
        
        OnCheckpointChanged?.Invoke(checkpoints[currentCheckpoint].position);
        
        Debug.Log($"Checkpoint {checkpointIndex} reached! Position: {checkpoints[currentCheckpoint].position}");
    }
    
    private IEnumerator QuitGameSequence()
    {
        Debug.Log("QuitGameSequence started - waiting " + quitDelay + " seconds");
        
        if (playerDamagable != null)
        {
            try
            {
                PlayerDamagable.OnPlayerDeath -= HandlePlayerDeath;
                PlayerDamagable.OnPlayerRespawn -= HandlePlayerRespawn;
                playerDamagable = null;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error unregistering from player events: " + e.Message);
            }
        }
        
        OnGameOver?.Invoke();
        
        yield return new WaitForSeconds(quitDelay);
        
        Debug.Log("Quitting application");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void RestartGame()
    {
        Debug.Log("RestartGame called");
        
        Time.timeScale = 1f;
        
        currentLives = playerLives;
        OnLivesChanged?.Invoke(currentLives);
        
        currentCheckpoint = 0;
        
        SceneManager.LoadScene(gameplaySceneName);
        
        OnGameRestart?.Invoke();
    }
    
    public void AddLife()
    {
        currentLives++;
        OnLivesChanged?.Invoke(currentLives);
        if (debugMode) Debug.Log("Life added. Current lives: " + currentLives);
    }
    
    public void RemoveLife()
    {
        if (currentLives > 1)
        {
            currentLives--;
            OnLivesChanged?.Invoke(currentLives);
            if (debugMode) Debug.Log("Life removed. Current lives: " + currentLives);
        }
        else if (currentLives == 1)
        {
            currentLives = 0;
            OnLivesChanged?.Invoke(currentLives);
            
            if (debugMode) Debug.Log("Last life removed. Game Over.");
            
            isQuitting = true;
            StartCoroutine(QuitGameSequence());
        }
    }
    
    [ContextMenu("Force Game Over")]
    public void ForceGameOver()
    {
        currentLives = 0;
        OnLivesChanged?.Invoke(currentLives);
        StartCoroutine(QuitGameSequence());
    }
    
    [ContextMenu("Add Life")]
    public void DebugAddLife()
    {
        AddLife();
    }
    
    [ContextMenu("Go to Next Checkpoint")]
    public void DebugNextCheckpoint()
    {
        if (checkpoints == null || player == null) return;
        
        if (currentCheckpoint < checkpoints.Length - 1)
        {
            ReachCheckpoint(currentCheckpoint + 1);
            
            if (checkpoints[currentCheckpoint] != null && player != null)
            {
                player.transform.position = checkpoints[currentCheckpoint].position;
            }
        }
    }
    
    [ContextMenu("Go to Previous Checkpoint")]
    public void DebugPreviousCheckpoint()
    {
        if (checkpoints == null || player == null) return;
        
        if (currentCheckpoint > 0)
        {
            ReachCheckpoint(currentCheckpoint - 1);
            
            if (checkpoints[currentCheckpoint] != null && player != null)
            {
                player.transform.position = checkpoints[currentCheckpoint].position;
            }
        }
    }
    
    public int CurrentLives => currentLives;
    public int MaxLives => playerLives;
    public int CurrentCheckpoint => currentCheckpoint;
    
    public Vector3 GetCurrentCheckpointPosition()
    {
        if (checkpoints != null && checkpoints.Length > 0 && 
            currentCheckpoint >= 0 && currentCheckpoint < checkpoints.Length && 
            checkpoints[currentCheckpoint] != null)
        {
            return checkpoints[currentCheckpoint].position;
        }
        return initialPlayerPosition;
    }
    
    private void Update()
    {
        if (SceneManager.GetActiveScene().name == gameplaySceneName &&
            (player == null || playerDamagable == null) && hasInitialized)
        {
            InitializePlayerReferences();
        }
    }
    
    private void OnValidate()
    {
        if (checkpoints != null)
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {
                if (checkpoints[i] == null)
                {
                    Debug.LogWarning($"Checkpoint {i} is not assigned!");
                }
            }
        }
    }
    
    public static bool HasInstance()
    {
        if (applicationQuitting || isQuitting)
        {
            return false;
        }
        
        return _instance != null;
    }
}