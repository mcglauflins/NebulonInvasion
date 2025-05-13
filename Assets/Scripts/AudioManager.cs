using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float musicFadeDuration = 1.5f;
    [SerializeField] private bool playOnAwake = true;
    
    [Header("Sound Effects")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip playerJumpSound;
    [SerializeField] private AudioClip playerHurtSound;
    [SerializeField] private AudioClip collectibleSound;
    [SerializeField] private AudioClip checkpointSound;
    [SerializeField] private float soundEffectVolume = 0.7f;
    
    [Header("Advanced Settings")]
    [SerializeField] private int sfxSourcesPoolSize = 5;
    
    private AudioSource musicSource;
    private List<AudioSource> sfxSources;
    private int currentSfxSourceIndex = 0;
    
    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioSources()
    {
        // Setup music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
        
        // Create a pool of audio sources for sound effects
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < sfxSourcesPoolSize; i++)
        {
            AudioSource sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = soundEffectVolume;
            sfxSources.Add(sfxSource);
        }
        
        // Start playing background music if enabled
        if (playOnAwake && backgroundMusic != null)
        {
            musicSource.Play();
        }
    }
    
    // Get the next available audio source for playing sound effects
    private AudioSource GetAvailableSfxSource()
    {
        // Try to find an audio source that is not playing
        for (int i = 0; i < sfxSources.Count; i++)
        {
            int index = (currentSfxSourceIndex + i) % sfxSources.Count;
            if (!sfxSources[index].isPlaying)
            {
                currentSfxSourceIndex = (index + 1) % sfxSources.Count;
                return sfxSources[index];
            }
        }
        
        // If all sources are playing, use the next one in rotation
        AudioSource source = sfxSources[currentSfxSourceIndex];
        currentSfxSourceIndex = (currentSfxSourceIndex + 1) % sfxSources.Count;
        return source;
    }
    
    // Play a sound effect at default volume
    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSfxSource();
        source.clip = clip;
        source.volume = soundEffectVolume;
        source.Play();
    }
    
    // Play a sound effect with custom volume
    public void PlaySoundEffect(AudioClip clip, float volume)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableSfxSource();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume * soundEffectVolume);
        source.Play();
    }
    
    // Play a sound effect at a specific position in 3D space
    public void PlaySoundEffectAtPosition(AudioClip clip, Vector3 position, float volume = 1.0f)
    {
        if (clip == null) return;
        
        AudioSource.PlayClipAtPoint(clip, position, volume * soundEffectVolume);
    }
    
    // Predefined sound effect methods for easy access
    public void PlayExplosionSound()
    {
        PlaySoundEffect(explosionSound);
    }
    
    public void PlayExplosionSoundAtPosition(Vector3 position)
    {
        PlaySoundEffectAtPosition(explosionSound, position);
    }
    
    public void PlayPlayerJumpSound()
    {
        PlaySoundEffect(playerJumpSound);
    }
    
    public void PlayPlayerHurtSound()
    {
        PlaySoundEffect(playerHurtSound);
    }
    
    public void PlayCollectibleSound()
    {
        PlaySoundEffect(collectibleSound);
    }
    
    public void PlayCheckpointSound()
    {
        PlaySoundEffect(checkpointSound);
    }
    
    // Volume control methods
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
    
    public void SetSfxVolume(float volume)
    {
        soundEffectVolume = Mathf.Clamp01(volume);
    }
}