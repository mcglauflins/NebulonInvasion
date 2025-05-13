using UnityEngine;
using System.Collections;

public class MusicZoneTrigger : MonoBehaviour
{
    [Header("Trigger Type")]
    [SerializeField] private bool changeBackgroundMusic = true;
    [SerializeField] private bool playSoundFX = false;
    
    [Header("Music Settings")]
    [SerializeField] private AudioClip zoneMusic;
    [SerializeField] private bool oneTimeActivation = false;
    [SerializeField] private float fadeDuration = 1.5f;
    
    [Header("Sound FX Settings")]
    [SerializeField] private AudioClip soundFX;
    [SerializeField] private float soundFXVolume = 1f;
    [SerializeField] private bool loopSoundFX = false;
    
    [Header("References")]
    [SerializeField] private string playerTag = "Player";
    
    private bool hasActivated = false;
    private AudioSource fadingAudioSource;
    private AudioSource fxAudioSource;
    
    private void Awake()
    {
        // Add a secondary audio source for crossfading music
        if (changeBackgroundMusic)
        {
            fadingAudioSource = gameObject.AddComponent<AudioSource>();
            fadingAudioSource.loop = true;
            fadingAudioSource.volume = 0f;
            fadingAudioSource.playOnAwake = false;
            fadingAudioSource.clip = zoneMusic;
        }
        
        // Add an audio source for sound FX
        if (playSoundFX)
        {
            fxAudioSource = gameObject.AddComponent<AudioSource>();
            fxAudioSource.loop = loopSoundFX;
            fxAudioSource.volume = soundFXVolume;
            fxAudioSource.playOnAwake = false;
            fxAudioSource.clip = soundFX;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (!oneTimeActivation || (oneTimeActivation && !hasActivated))
            {
                // Play sound FX if enabled
                if (playSoundFX && soundFX != null && fxAudioSource != null)
                {
                    fxAudioSource.Play();
                }
                
                // Change background music if enabled
                if (changeBackgroundMusic && zoneMusic != null)
                {
                    ChangeBackgroundMusic();
                }
                
                hasActivated = true;
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Stop looping sound FX when player leaves, if it's not set to loop
        if (other.CompareTag(playerTag) && playSoundFX && !loopSoundFX && fxAudioSource != null && fxAudioSource.isPlaying)
        {
            fxAudioSource.Stop();
        }
    }
    
    private void ChangeBackgroundMusic()
    {
        // Find the AudioManager
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("AudioManager not found!");
            return;
        }
        
        // Find the original music source
        AudioSource musicSource = audioManager.GetComponent<AudioSource>();
        if (musicSource == null || !musicSource.isPlaying)
        {
            Debug.LogWarning("AudioManager has no playing AudioSource!");
            return;
        }
        
        // Start the crossfade
        StartCoroutine(CrossFadeMusic(musicSource));
    }
    
    private IEnumerator CrossFadeMusic(AudioSource originalSource)
    {
        Debug.Log("CrossFadeMusic starting: Fading out " + (originalSource.clip != null ? originalSource.clip.name : "null") 
                 + " and fading in " + (zoneMusic != null ? zoneMusic.name : "null"));
        
        // Setup the fading in source
        fadingAudioSource.clip = zoneMusic;
        fadingAudioSource.volume = 0f;
        fadingAudioSource.Play();
        
        // Remember the original volume
        float originalVolume = originalSource.volume;
        
        // Fade out the original music while fading in the new music
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            originalSource.volume = Mathf.Lerp(originalVolume, 0f, t);
            fadingAudioSource.volume = Mathf.Lerp(0f, originalVolume, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final volumes are set correctly
        originalSource.volume = 0f;
        fadingAudioSource.volume = originalVolume;
        
        // Stop the original music to save resources
        originalSource.Stop();
        
        // Transfer the clip to the original source and start playing it
        originalSource.clip = zoneMusic;
        originalSource.volume = originalVolume;
        originalSource.Play();
        
        // Stop our temporary audio source
        fadingAudioSource.Stop();
        
        Debug.Log("Music transition complete");
    }
    
    private void OnDrawGizmos()
    {
        // Choose color based on trigger type
        Color gizmoColor;
        string label;
        
        if (changeBackgroundMusic && playSoundFX)
        {
            gizmoColor = new Color(0.8f, 0.4f, 0.8f, 0.3f); // Purple for both
            label = "♫ Music + FX Zone";
        }
        else if (changeBackgroundMusic)
        {
            gizmoColor = new Color(0.3f, 0.6f, 0.9f, 0.3f); // Blue for music
            label = "♫ Music Zone";
        }
        else
        {
            gizmoColor = new Color(0.9f, 0.6f, 0.3f, 0.3f); // Orange for FX
            label = "🔊 Sound FX Zone";
        }
        
        // Visualize the zone in the editor
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.lossyScale);
        
        // Draw outline
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.8f);
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * (transform.lossyScale.y / 2 + 0.5f), label);
        #endif
    }
}