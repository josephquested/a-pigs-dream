using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // -- SYSTEM -- //

    void Start()
    {
        
    }

    void Update()
    {
        CheckForRestart();
    }

    // -- GAME STATE -- //

    public GameObject gameOverScreen;

    bool isGameOver = false;

    public void GameOver()
    {
        isGameOver = true;
        gameOverScreen.SetActive(true);
        Debug.Log("Game Over!");
    }

    void CheckForRestart()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
