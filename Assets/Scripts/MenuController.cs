using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // -- SYSTEM -- //

    public GameObject startingParent;
    public GameObject nameEntryParent;
    public AudioSource confirmAudioSource;
    public AudioSource typingAudioSource;
    public AudioClip[] typingSoundEffects = new AudioClip[2];

    bool nameEntryFlowStarted;
    bool isSubmittingPlayerName;
    int nextTypingSoundIndex;

    void Start()
    {
        
    }

    void Update()
    {
        bool enterPressed = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        if (nameEntryFlowStarted && Input.anyKeyDown && !enterPressed)
        {
            PlayTypingSound();
        }

        if (enterPressed)
        {
            if (!nameEntryFlowStarted)
            {
                OpenNameEntry();
                return;
            }

            if (isSubmittingPlayerName)
                return;

            PlayIfAssigned(confirmAudioSource);
            SubmitPlayerNameAndStartGame();
        }
    }

    void PlayTypingSound()
    {
        if (typingAudioSource == null)
            return;

        if (typingSoundEffects != null && typingSoundEffects.Length > 0)
        {
            int attempts = 0;
            int clipIndex = nextTypingSoundIndex;
            AudioClip clip = null;

            while (attempts < typingSoundEffects.Length)
            {
                clipIndex %= typingSoundEffects.Length;
                clip = typingSoundEffects[clipIndex];
                clipIndex++;
                attempts++;

                if (clip != null)
                    break;
            }

            nextTypingSoundIndex = clipIndex % typingSoundEffects.Length;

            if (clip != null)
            {
                typingAudioSource.PlayOneShot(clip);
                return;
            }
        }

        typingAudioSource.Play();
    }

    void PlayIfAssigned(AudioSource source)
    {
        if (source != null)
        {
            source.Play();
        }
    }

    public void OpenNameEntry()
    {
        PlayIfAssigned(confirmAudioSource);
        nameEntryFlowStarted = true;

        if (startingParent != null)
            startingParent.SetActive(false);

        if (nameEntryParent != null)
            nameEntryParent.SetActive(true);
    }

    void SubmitPlayerNameAndStartGame()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogWarning("LootManager instance was not found.");
            return;
        }

        if (LootManager.Instance.playerNameInputField == null)
            return;

        string enteredName = LootManager.Instance.playerNameInputField.text.Trim();
        if (string.IsNullOrEmpty(enteredName))
            return;

        isSubmittingPlayerName = true;

        LootManager.Instance.UpdatePlayerName((wasUpdated) =>
        {
            isSubmittingPlayerName = false;

            if (!wasUpdated)
            {
                Debug.LogWarning("Player name update failed. Not loading game scene.");
                return;
            }

            SceneManager.LoadScene("Game");
        });
    }
}
