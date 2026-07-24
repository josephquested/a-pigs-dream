using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public float gameTime = 60f;

    bool isGameOver = false;
    float timeRemaining;
    float timerTickCounter;
    float scoreTickCounter;
    int score;

    public void GameOver()
    {
        isGameOver = true;
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
}
