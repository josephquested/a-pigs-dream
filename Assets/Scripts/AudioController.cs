using UnityEngine;

public class AudioController : MonoBehaviour
{
    // Singleton access from any script: AudioController.Instance
    public static AudioController Instance { get; private set; }

    [Header("Gameplay SFX")]
    public AudioSource jumpAudioSource;
    public AudioSource dashAudioSource;
    public AudioSource waterDeathAudioSource;
    public AudioSource crashIntoSomethingDeathAudioSource;
    public AudioSource applePickupAudioSource;
    public AudioSource bushBreakAudioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayJump()
    {
        PlayIfAssigned(jumpAudioSource);
    }

    public void PlayDash()
    {
        PlayIfAssigned(dashAudioSource);
    }

    public void PlayWaterDeath()
    {
        PlayIfAssigned(waterDeathAudioSource);
    }

    public void PlayCrashDeath()
    {
        PlayIfAssigned(crashIntoSomethingDeathAudioSource);
    }

    public void PlayApplePickup()
    {
        PlayIfAssigned(applePickupAudioSource);
    }

    public void PlayBushBreak()
    {
        PlayIfAssigned(bushBreakAudioSource);
    }

    void PlayIfAssigned(AudioSource source)
    {
        if (source != null)
        {
            source.Play();
        }
    }
}
