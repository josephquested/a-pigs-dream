using UnityEngine;

public class AudioController : MonoBehaviour
{
    // Singleton access from any script: AudioController.Instance
    public static AudioController Instance { get; private set; }

    [Header("Gameplay SFX")]
    public AudioSource bgmAudioSource;
    public AudioSource jumpAudioSource;
    public AudioClip[] jumpClips;

    [Header("Running Loop")]
    public AudioSource runningAudioSource;
    public float runningFadeInDuration = 0.12f;
    public float runningFadeOutDuration = 0.12f;

    public AudioSource dashAudioSource;
    public AudioSource waterDeathAudioSource;
    public AudioSource crashIntoSomethingDeathAudioSource;
    public AudioClip[] crashDeathClips;
    public AudioSource applePickupAudioSource;
    public AudioSource bushBreakAudioSource;
    public AudioSource starvingAudioSource;
    public AudioSource confirmAudioSource;

    float runningTargetVolume = 1f;
    bool runningLoopWantsToPlay;
    float bgmTargetVolume = 1f;
    bool isBGMMuted;
    Coroutine bgmFadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (runningAudioSource != null)
        {
            runningTargetVolume = Mathf.Clamp01(runningAudioSource.volume);
        }

        if (bgmAudioSource != null)
        {
            bgmTargetVolume = Mathf.Clamp01(bgmAudioSource.volume);
            PlayBGM();
        }
    }

    void Update()
    {
        UpdateRunningLoopFade();
        UpdateBGMMuteToggle();
    }

    public void PlayJump()
    {
        PlayRandomClipOrSource(jumpAudioSource, jumpClips);
    }

    public void PlayBGM()
    {
        if (bgmAudioSource == null)
            return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        bgmAudioSource.loop = true;
        bgmAudioSource.volume = GetCurrentBGMVolumeTarget();
        if (!bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }

    public void FadeOutBGM(float duration)
    {
        if (bgmAudioSource == null)
            return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        if (!bgmAudioSource.isPlaying)
        {
            bgmAudioSource.volume = GetCurrentBGMVolumeTarget();
            return;
        }

        if (duration <= 0f)
        {
            bgmAudioSource.Stop();
            bgmAudioSource.volume = bgmTargetVolume;
            return;
        }

        bgmFadeCoroutine = StartCoroutine(FadeOutBGMRoutine(duration));
    }

    public void PlayDash()
    {
        PlayIfAssigned(dashAudioSource);
    }

    public void StartRunningLoop()
    {
        if (runningAudioSource == null)
        {
            return;
        }

        runningLoopWantsToPlay = true;
        runningAudioSource.loop = true;
        if (!runningAudioSource.isPlaying)
        {
            runningAudioSource.volume = 0f;
            runningAudioSource.Play();
        }
    }

    public void StopRunningLoop()
    {
        if (runningAudioSource == null)
        {
            return;
        }

        runningLoopWantsToPlay = false;

        if (!runningAudioSource.isPlaying)
        {
            runningAudioSource.Stop();
            runningAudioSource.volume = runningTargetVolume;
        }
    }

    public void PlayWaterDeath()
    {
        PlayIfAssigned(waterDeathAudioSource);
    }

    public void PlayCrashDeath()
    {
        PlayRandomClipOrSource(crashIntoSomethingDeathAudioSource, crashDeathClips);
    }

    public void PlayApplePickup()
    {
        PlayIfAssigned(applePickupAudioSource);
    }

    public void PlayBushBreak()
    {
        PlayIfAssigned(bushBreakAudioSource);
    }

    public void PlayConfirm()
    {
        PlayIfAssigned(confirmAudioSource);
    }

    public void PlayStarvingTick(int hungerPoints)
    {
        if (starvingAudioSource == null)
            return;

        int safeHunger = Mathf.Max(0, hungerPoints);
        float pitch = 1f + ((8 - safeHunger) * 0.075f);
        starvingAudioSource.pitch = Mathf.Max(0f, pitch);
        starvingAudioSource.Play();
    }

    void PlayIfAssigned(AudioSource source)
    {
        if (source != null)
        {
            source.Play();
        }
    }

    void PlayRandomClipOrSource(AudioSource source, AudioClip[] clips)
    {
        if (source == null)
        {
            return;
        }

        if (clips != null && clips.Length > 0)
        {
            int index = Random.Range(0, clips.Length);
            AudioClip clip = clips[index];
            if (clip != null)
            {
                source.PlayOneShot(clip);
                return;
            }
        }

        source.Play();
    }

    void UpdateRunningLoopFade()
    {
        if (runningAudioSource == null || !runningAudioSource.isPlaying)
            return;

        float target = runningLoopWantsToPlay ? runningTargetVolume : 0f;
        float duration = runningLoopWantsToPlay ? runningFadeInDuration : runningFadeOutDuration;

        if (duration <= 0f)
        {
            runningAudioSource.volume = target;
        }
        else
        {
            float step = (runningTargetVolume / duration) * Time.deltaTime;
            runningAudioSource.volume = Mathf.MoveTowards(runningAudioSource.volume, target, step);
        }

        if (!runningLoopWantsToPlay && Mathf.Approximately(runningAudioSource.volume, 0f))
        {
            runningAudioSource.Stop();
            runningAudioSource.volume = runningTargetVolume;
        }
    }

    System.Collections.IEnumerator FadeOutBGMRoutine(float duration)
    {
        float elapsed = 0f;
        float startVolume = bgmAudioSource.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }

        bgmAudioSource.Stop();
        bgmAudioSource.volume = GetCurrentBGMVolumeTarget();
        bgmFadeCoroutine = null;
    }

    void UpdateBGMMuteToggle()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleBGMMute();
        }
    }

    void ToggleBGMMute()
    {
        if (bgmAudioSource == null)
            return;

        if (bgmFadeCoroutine != null)
        {
            StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = null;
        }

        isBGMMuted = !isBGMMuted;
        bgmAudioSource.volume = GetCurrentBGMVolumeTarget();

        if (!bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }

    float GetCurrentBGMVolumeTarget()
    {
        return isBGMMuted ? 0f : bgmTargetVolume;
    }
}
