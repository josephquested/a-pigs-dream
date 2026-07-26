using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using LootLocker.Requests;

public class GameController : MonoBehaviour
{
    const string FirstPlayTutorialSeenKey = "FIRST_PLAY_TUTORIAL_SEEN";

    // -- SYSTEM -- //

    CameraController cameraController;
    LevelController levelController;
    Pig pig;

    void Start()
    {
        cameraController = GameObject.FindFirstObjectByType<CameraController>();
        levelController = GameObject.FindFirstObjectByType<LevelController>();
        pig = GameObject.FindFirstObjectByType<Pig>();

        if (AudioController.Instance != null)
        {
            AudioController.Instance.PlayBGM();
        }

        timeRemaining = gameTime;
        score = 0;
        UpdateTimerDisplay();
        UpdateScoreDisplay();
        ShowFirstPlayTutorialIfNeeded();
    }

    void Update()
    {
        UpdateTimer();
        UpdateScore();
        CheckForRestart();
    }

    // -- GAME STATE -- //

    public GameObject gameOverScreen;
    public GameObject loadingHighscoresObj;
    public GameObject highscoresParent;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI yourScoreText;
    public TextMeshProUGUI tutorialText;
    [TextArea(2, 6)] public string firstPlayTutorialMessage;
    public float tutorialTextDisplayDuration = 8f;
    public float tutorialLineFadeInDuration = 0.35f;
    public float tutorialFirstLineFadeInMultiplier = 2f;
    public float tutorialLineStaggerDelay = 0.1f;
    public float tutorialFadeOutDuration = 0.4f;
    public List<TextMeshProUGUI> highscoreUIs = new List<TextMeshProUGUI>();
    public float gameTime = 60f;
    public float gameOverScreenDelay = 1f;

    [Header("DEBUG")]
    public bool showResetTutorialDebugButton = true;
    public Vector2 resetTutorialDebugButtonPosition = new Vector2(20f, 20f);
    public Vector2 resetTutorialDebugButtonSize = new Vector2(260f, 40f);
    public string resetTutorialDebugButtonLabel = "DEBUG: Reset First-Play Tutorial";
    public bool showMarkTutorialSeenDebugButton = true;
    public Vector2 markTutorialSeenDebugButtonOffset = new Vector2(0f, 48f);
    public string markTutorialSeenDebugButtonLabel = "DEBUG: Mark Tutorial Seen";
    public bool reloadSceneAfterTutorialReset = true;

    bool isGameOver = false;
    bool isGameOverScreenVisible = false;
    float timeRemaining;
    float timerTickCounter;
    float scoreTickCounter;
    int score;
    Coroutine tutorialTextRoutine;

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

        if (AudioController.Instance != null)
        {
            AudioController.Instance.FadeOutBGM(Mathf.Max(0f, gameOverScreenDelay));
        }

        StartCoroutine(HandleGameOverSequence());
    }

    IEnumerator HandleGameOverSequence()
    {
        float safeDelay = Mathf.Max(0f, gameOverScreenDelay);
        if (safeDelay > 0f)
        {
            yield return new WaitForSeconds(safeDelay);
        }

        gameOverScreen.SetActive(true);
        isGameOverScreenVisible = true;

        if (cameraController != null)
        {
            cameraController.ResetToStartPosition();
        }

        if (levelController != null)
        {
            levelController.DisableLevelAndPig();
        }

        Debug.Log("Game Over!");

        if (LootManager.Instance != null)
        {
            LootManager.Instance.AddScore(score, (submitSuccess) =>
            {
                if (!submitSuccess)
                {
                    Debug.LogWarning("Score submission failed; requesting leaderboard anyway.");
                }

                GetScores();
            });
        }
        else
        {
            Debug.LogWarning("LootManager instance was not found; cannot submit score before loading leaderboard.");
        }
    }

    public void AddTime(float timeToAdd)
    {
        timeRemaining += timeToAdd;
        UpdateTimerDisplay();
    }

    public void AddScorePoints(int pointsToAdd)
    {
        if (isGameOver || pointsToAdd <= 0)
            return;

        score += pointsToAdd;
        UpdateScoreDisplay();
    }

    void UpdateTimer()
    {
        if (isGameOver)
            return;

        timerTickCounter += Time.deltaTime;
        if (timerTickCounter >= 1f)
        {
            timeRemaining -= 1f;
            timerTickCounter = 0f;
            UpdateTimerDisplay();
            
            if (timeRemaining <= 0)
            {
                if (pig != null)
                {
                    pig.TriggerHungerDeath();
                }
                else
                {
                    GameOver();
                }
            }
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Max(0, Mathf.FloorToInt(timeRemaining)).ToString();
        }
    }

    void UpdateScore()
    {
        if (isGameOver)
            return;

        scoreTickCounter += Time.deltaTime;
        if (scoreTickCounter >= 1f)
        {
            score += 1;
            scoreTickCounter = 0f;
            UpdateScoreDisplay();
        }
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    void CheckForRestart()
    {
        if (isGameOverScreenVisible && Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void GetScores()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogWarning("LootManager instance was not found.");
            return;
        }

        if (loadingHighscoresObj != null)
            loadingHighscoresObj.SetActive(true);

        if (highscoresParent != null)
            highscoresParent.SetActive(false);

        LootManager.Instance.GetScores(DisplayHighScores);
    }

    void DisplayHighScores(LootLockerLeaderboardMember[] items)
    {
        if (loadingHighscoresObj != null)
            loadingHighscoresObj.SetActive(false);

        if (highscoresParent != null)
            highscoresParent.SetActive(true);

        string currentPlayerName = "YOU";
        if (LootManager.Instance != null && !string.IsNullOrEmpty(LootManager.Instance.PlayerName))
        {
            currentPlayerName = LootManager.Instance.PlayerName;
        }

        if (yourScoreText != null)
            yourScoreText.text = currentPlayerName + ": " + score;

        int i = 0;
        foreach (TextMeshProUGUI ui in highscoreUIs)
        {
            if (ui == null)
            {
                i++;
                continue;
            }

            if (items != null && i < items.Length)
            {
                ui.gameObject.SetActive(true);
                string playerDisplayName = items[i].player != null ? items[i].player.name : "UNKNOWN";
                ui.text = items[i].rank.ToString() + ". " + playerDisplayName + ": " + items[i].score;
            }
            else
            {
                ui.gameObject.SetActive(false);
            }

            i++;
        }
    }

    void ShowFirstPlayTutorialIfNeeded()
    {
        bool shouldShowTutorial = levelController != null
            ? levelController.IsFirstPlayRun
            : PlayerPrefs.GetInt(FirstPlayTutorialSeenKey, 0) == 0;

        if (!shouldShowTutorial || tutorialText == null)
            return;

        if (tutorialTextRoutine != null)
        {
            StopCoroutine(tutorialTextRoutine);
        }

        tutorialTextRoutine = StartCoroutine(PlayTutorialTextSequence());
    }

    IEnumerator PlayTutorialTextSequence()
    {
        string tutorialMessage = firstPlayTutorialMessage == null ? string.Empty : firstPlayTutorialMessage;
        string[] lines = tutorialMessage.Split('\n');
        if (lines.Length == 0)
        {
            lines = new string[] { string.Empty };
        }

        float[] lineAlphas = new float[lines.Length];
        tutorialText.gameObject.SetActive(true);
        tutorialText.text = BuildTutorialRichText(lines, lineAlphas);

        float safeLineFadeInDuration = Mathf.Max(0f, tutorialLineFadeInDuration);
        float safeLineStaggerDelay = Mathf.Max(0f, tutorialLineStaggerDelay);

        for (int i = 0; i < lines.Length; i++)
        {
            float lineFadeDuration = safeLineFadeInDuration;
            if (i == 0)
            {
                lineFadeDuration *= Mathf.Max(0f, tutorialFirstLineFadeInMultiplier);
            }

            if (lineFadeDuration <= 0f)
            {
                lineAlphas[i] = 1f;
                tutorialText.text = BuildTutorialRichText(lines, lineAlphas);
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < lineFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    lineAlphas[i] = Mathf.Clamp01(elapsed / lineFadeDuration);
                    tutorialText.text = BuildTutorialRichText(lines, lineAlphas);
                    yield return null;
                }
                lineAlphas[i] = 1f;
                tutorialText.text = BuildTutorialRichText(lines, lineAlphas);
            }

            if (i < lines.Length - 1 && safeLineStaggerDelay > 0f)
            {
                yield return new WaitForSeconds(safeLineStaggerDelay);
            }
        }

        float firstLineDuration = lines.Length > 0
            ? safeLineFadeInDuration * Mathf.Max(0f, tutorialFirstLineFadeInMultiplier)
            : 0f;
        float remainingLinesDuration = safeLineFadeInDuration * Mathf.Max(0, lines.Length - 1);
        float introDuration = firstLineDuration + remainingLinesDuration + (safeLineStaggerDelay * Mathf.Max(0, lines.Length - 1));
        float safeFadeOutDuration = Mathf.Max(0f, tutorialFadeOutDuration);
        float holdDuration = Mathf.Max(0f, tutorialTextDisplayDuration - introDuration - safeFadeOutDuration);
        if (holdDuration > 0f)
        {
            yield return new WaitForSeconds(holdDuration);
        }

        if (safeFadeOutDuration > 0f)
        {
            float fadeElapsed = 0f;
            while (fadeElapsed < safeFadeOutDuration)
            {
                fadeElapsed += Time.deltaTime;
                float fadeMultiplier = 1f - Mathf.Clamp01(fadeElapsed / safeFadeOutDuration);
                for (int i = 0; i < lineAlphas.Length; i++)
                {
                    lineAlphas[i] = fadeMultiplier;
                }

                tutorialText.text = BuildTutorialRichText(lines, lineAlphas);
                yield return null;
            }
        }

        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(false);
        }

        tutorialTextRoutine = null;
    }

    string BuildTutorialRichText(string[] lines, float[] lineAlphas)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < lines.Length; i++)
        {
            int alphaByte = Mathf.Clamp(Mathf.RoundToInt(lineAlphas[i] * 255f), 0, 255);
            sb.Append("<color=#FFFFFF");
            sb.Append(alphaByte.ToString("X2"));
            sb.Append(">");
            sb.Append(lines[i]);
            sb.Append("</color>");

            if (i < lines.Length - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    void OnGUI()
    {
        if (!showResetTutorialDebugButton)
            return;

        if (!Application.isEditor && !Debug.isDebugBuild)
            return;

        Rect buttonRect = new Rect(
            resetTutorialDebugButtonPosition.x,
            resetTutorialDebugButtonPosition.y,
            resetTutorialDebugButtonSize.x,
            resetTutorialDebugButtonSize.y
        );

        if (GUI.Button(buttonRect, resetTutorialDebugButtonLabel))
        {
            ResetFirstPlayTutorialDebug();
        }

        if (!showMarkTutorialSeenDebugButton)
            return;

        Rect markSeenButtonRect = new Rect(
            resetTutorialDebugButtonPosition.x + markTutorialSeenDebugButtonOffset.x,
            resetTutorialDebugButtonPosition.y + markTutorialSeenDebugButtonOffset.y,
            resetTutorialDebugButtonSize.x,
            resetTutorialDebugButtonSize.y
        );

        if (GUI.Button(markSeenButtonRect, markTutorialSeenDebugButtonLabel))
        {
            MarkFirstPlayTutorialSeenDebug();
        }
    }

    public void ResetFirstPlayTutorialDebug()
    {
        PlayerPrefs.DeleteKey(FirstPlayTutorialSeenKey);
        PlayerPrefs.Save();
        Debug.Log("Reset first-play tutorial flag.");

        if (reloadSceneAfterTutorialReset)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void MarkFirstPlayTutorialSeenDebug()
    {
        PlayerPrefs.SetInt(FirstPlayTutorialSeenKey, 1);
        PlayerPrefs.Save();
        Debug.Log("Marked first-play tutorial as seen.");

        if (reloadSceneAfterTutorialReset)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
