using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {
        timeRemaining = gameTime;
        UpdateTimerDisplay();
    }

    void Update()
    {
        UpdateTimer();
        CheckForRestart();
    }

    // -- GAME STATE -- //

    public GameObject gameOverScreen;
    public TextMeshProUGUI timerText;
    public float gameTime = 60f;

    bool isGameOver = false;
    float timeRemaining;
    float timerTickCounter;

    public void GameOver()
    {
        isGameOver = true;
        gameOverScreen.SetActive(true);
        Debug.Log("Game Over!");
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

    void CheckForRestart()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
