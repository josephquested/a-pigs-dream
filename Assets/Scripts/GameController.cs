using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using LootLocker.Requests;

public class GameController : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {
        timeRemaining = gameTime;
        score = 0;
        UpdateTimerDisplay();
        UpdateScoreDisplay();
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
    public List<TextMeshProUGUI> highscoreUIs = new List<TextMeshProUGUI>();
    public float gameTime = 60f;

    bool isGameOver = false;
    float timeRemaining;
    float timerTickCounter;
    float scoreTickCounter;
    int score;

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;

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

        gameOverScreen.SetActive(true);
        Debug.Log("Game Over!");
    }

    public void AddTime(float timeToAdd)
    {
        timeRemaining += timeToAdd;
        UpdateTimerDisplay();
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
                GameOver();
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
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
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
}
