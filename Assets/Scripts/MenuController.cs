using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // -- SYSTEM -- //

    public GameObject startingParent;
    public GameObject nameEntryParent;
    public GameObject startGameParent;

    bool canStartGame;
    bool nameEntryFlowStarted;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!nameEntryFlowStarted)
                return;

            if (!canStartGame)
            {
                SubmitPlayerName();
                return;
            }

            SceneManager.LoadScene("Game");
        }
    }

    public void OpenNameEntry()
    {
        nameEntryFlowStarted = true;

        if (startingParent != null)
            startingParent.SetActive(false);

        if (nameEntryParent != null)
            nameEntryParent.SetActive(true);

        if (startGameParent != null)
            startGameParent.SetActive(false);
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
