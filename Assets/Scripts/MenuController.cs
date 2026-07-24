using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // -- SYSTEM -- //

    public GameObject nameEntryParent;
    public GameObject startGameParent;

    bool canStartGame;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!canStartGame)
            {
                SubmitPlayerName();
                return;
            }

            SceneManager.LoadScene("Game");
        }
    }

    void SubmitPlayerName()
    {
        if (LootManager.Instance == null)
        {
            Debug.LogWarning("LootManager instance was not found.");
            return;
        }

        LootManager.Instance.UpdatePlayerName();

        if (LootManager.Instance.playerNameInputField == null)
            return;

        string enteredName = LootManager.Instance.playerNameInputField.text;
        if (string.IsNullOrEmpty(enteredName) || string.IsNullOrEmpty(enteredName.Trim()))
            return;

        canStartGame = true;

        if (nameEntryParent != null)
            nameEntryParent.SetActive(false);

        if (startGameParent != null)
            startGameParent.SetActive(true);
    }
}
